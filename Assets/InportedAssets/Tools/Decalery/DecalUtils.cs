#define USE_TERRAINS
//#define LONG_DECALID

#if UNITY_2018_1_OR_NEWER
#if USE_BURST
#if USE_NEWMATHS
#define USE_BURST_REALLY
#endif
#endif
#endif

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
#endif

#if USE_BURST_REALLY
using Unity.Mathematics;
#endif

public class DecalUtils
{
	public struct GroupDesc
	{
		// Material used by all decals in the group
		// Use shaders made specifically for decals (offsetting position to camera to prevent Z-fighting)
		public Material material;

		// Maximum amount of triangles in the group.
		// Decal triangle count depends on receiver geometry detail and decal size.
		// Older decals will disappear when new decals are added above the limit.
		public int maxTrisTotal;

		// Maximum allowed triangle count for one decal.
		// Decal triangle count depends on receiver geometry detail and decal size.
		// While this number can be set to totalTris, using a realistic limit for your game will reduce processing time.
		// If a decal crosses this threshold, some of its triangles may disappear.
		public int maxTrisInDecal;

		// If decals need to be a part of a movable object, assign it here.
		public Renderer parent;

		// Lightmap ID used by decals. Normally obtained from gameObject.GetComponent<Renderer>().lightmapIndex.
		public int lightmapID, realtimeLightmapID;

		// Is this group supposed to be rendered with DrawProceduralIndirect?
		// Such groups don't need VRAM->RAM->VRAM memory transfers, as the generated buffer used directly by the drawing shader.
		// Shader must be aware of this method.
		public bool indirectDraw;

		// Is this decal a trail (e.g. a tire track)?
		// Trails connect edge-to-edge instead of being separated quads.
		// Trails also have a unique continuous UV generation style. 
		public bool isTrail;

		// If isTrail is enabled, controls vertical texture coordinate tiling. 
		public float trailVScale;

        // Should this group have tangents (does it need normal mapping)?
        public bool tangents;

        public Vector4 realtimeLightmapScaleOffset;

        public MaterialPropertyBlock materialPropertyBlock;

		//
		public void SetDefaults()
		{
			lightmapID = -1;
			trailVScale = 1.0f;
		}
	}

	public struct DecalDesc
	{
		// Decal projector origin.
		public Vector3 position;

		// Decal projector rotation.
		public Quaternion rotation;

		// Decal width
		public float sizeX;

		// Decal height
		public float sizeY;

		// Projection distance
		public float distance;

		// At which angle should the decal polygons be removed?
		// Range is from -1 (facing away) to 1 (facing towards)
		public float angleClip;

        // Opacity multiplier for the decal.
        public float opacity;

		// Receiving object's mesh.
		public Mesh mesh;

		// Receivng object's matrix.
		public Matrix4x4 worldMatrix;

		// Receiving object's lightmap scaling/offset. Normally obtained from gameObject.GetComponent<Renderer>().lightmapScaleOffset.
		public Vector4 lightmapScaleOffset;

        // Texture2DArray slice (optional). Set to -1 to disable.
        public int texArraySlice;

        // Decal texture scale/offset
        public float atlasMinX, atlasMinY, atlasMaxX, atlasMaxY;

        //
        public void SetDefaults()
        {
            opacity = 1.0f;
            texArraySlice = -1;
            atlasMinX = atlasMinY = 0;
            atlasMaxX = atlasMaxY = 1;
        }
	}

    public class Group
    {
        public Mode mode;
        public ComputeBuffer vbuffer;
        public ComputeBuffer countBuffer, argBuffer;
        public Material material;
        public Renderer parent;
        public Transform parentTform;
        public GameObject go;
        public DecalGroup decalGroup;
        public Mesh mesh;
        public CommandBuffer drawCmd;
        public Bounds bounds, nextBounds;
        public Vector3 prevDecalEdgeA, prevDecalEdgeB;
        public Vector4 prevDecalEdgePlane;
        public int numDecals, maxTrisInDecal, totalTris, boundsCounter, boundsMinDecalCounter;
        public int lightmapID;
        public int decalIDCounter;
        public bool indirectDraw;
        public bool isTrail;
        public float trailV;
        public float trailVScale;
        public bool tangents;
        public bool isSkinned;
        public MaterialPropertyBlock materialPropertyBlock;

        public int fixedOffset;//V, fixedOffsetI;
        public Vector3[] vPos, vNormal;
        public Vector2[] vUV, vUV2;
        public Color[] vColor;
        public BoneWeight[] vSkin;
        public Vector4[] vTangents;
#if LONG_DECALID
        public int[] vDecalID;
#else
        public byte[] vDecalID;
#endif

        public DecalSpawner spawnedBy;

#if UNITY_2018_1_OR_NEWER
        public NativeArray<Vector3> nvPos;
        public NativeArray<Vector3> nvNormal;
        public NativeArray<Color> nvColor;
        public NativeArray<Vector2> nvUV2;
        public NativeArray<Vector2> nvUV;
        public NativeArray<BoneWeight> nvSkin;
#if LONG_DECALID
        public NativeArray<int> nvDecalID;
#else
        public NativeArray<byte> nvDecalID;
#endif
        public NativeArray<Vector4> nvTangents;
#endif

#if UNITY_2019_3_OR_NEWER
        public GPUDecalUtils.ReadableVertex[] buff;
        public GPUDecalUtils.ReadableVertexTangents[] buffWithTangents;

        public NativeArray<GPUDecalUtils.ReadableVertex> buffNative;
        public NativeArray<GPUDecalUtils.ReadableVertexTangents> buffWithTangentsNative;

        public bool requestInProgress;

        public void UpdateMeshAsync(AsyncGPUReadbackRequest req)
        {
            if (req.hasError)
            {
                Debug.LogError("AsyncGPUReadbackRequest error");
                requestInProgress = false;
                return;
            }

            if (tangents)
            {
                mesh.SetVertexBufferData(buffWithTangentsNative, 0, 0, totalTris*3, 0, GPUDecalUtils.meshFlags);
            }
            else
            {
                mesh.SetVertexBufferData(buffNative, 0, 0, totalTris*3, 0, GPUDecalUtils.meshFlags);
            }

            requestInProgress = false;
        }
#endif
    }

    public struct Handle
    {
        public DecalUtils.Group group;
        public Mode mode;
        public int index;
    }

    public enum Mode
    {
        CPU,
        GPU,
        CPUBurst
    }

    public const int extraUV3 = 0;
    public const int extraUV4 = 1;
    public const int extraUV5 = 2;
    public const int extraUV6 = 3;
    public const int extraUV7 = 4;
    public const int extraUV8 = 5;
    public const int extraUV1 = 6;
    public const int extraUVCount = 7;

    public static Vector3 allNormalsAverage;

#if UNITY_EDITOR
    static MethodInfo _IntersectRayMesh;
    static object[] _IntersectRayMeshArgs = new object[4];
#endif

#if UNITY_EDITOR
    public static HashSet<string> meshAssetNames = new HashSet<string>();
#endif

    public static DecalUtils.Group CreateGroup(DecalUtils.GroupDesc desc, Mode mode)
    {
        if (mode == Mode.GPU)
        {
            return GPUDecalUtils.CreateGroup(desc);
        }
        else if (mode == Mode.CPUBurst)
        {
            return CPUBurstDecalUtils.CreateGroup(desc);
        }
        else
        {
            return CPUDecalUtils.CreateGroup(desc);
        }
    }

    public static void AddDecal(DecalUtils.Group group, DecalUtils.DecalDesc desc, GameObject receiver, Mode mode)
    {
        if (mode == Mode.GPU)
        {
            GPUDecalUtils.AddDecal(group, desc, receiver);
        }
        else if (mode == Mode.CPUBurst)
        {
            CPUBurstDecalUtils.AddDecal(group, desc, receiver);
        }
        else
        {
            CPUDecalUtils.AddDecal(group, desc, receiver);
        }
    }

    public static void ClearDecals(DecalUtils.Group group, Mode mode)
    {
        if (mode == Mode.GPU)
        {
            GPUDecalUtils.ClearDecals(group);
        }
        else if (mode == Mode.CPUBurst)
        {
            CPUBurstDecalUtils.ClearDecals(group);
        }
        else
        {
            CPUDecalUtils.ClearDecals(group);
        }
    }

    static Vector3 Vector3Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    internal static void ExpandBounds(Group group, Matrix4x4 decalMatrix, DecalDesc desc)
    {
        Vector3 row0 = decalMatrix.GetColumn(0);
        Vector3 row1 = decalMatrix.GetColumn(1);
        Vector3 row2 = decalMatrix.GetColumn(2);
        var center = desc.position + row2 * desc.distance * 0.5f;
        row0 = Vector3Abs(row0);
        row1 = Vector3Abs(row1);
        row2 = Vector3Abs(row2);
        var minPoint = center - row0 - row1 - row2;
        var maxPoint = center + row0 + row1 + row2;

        var bounds = group.bounds;
        bounds.SetMinMax(minPoint, maxPoint);

        if (group.numDecals == 0)
        {
            group.bounds = bounds;
        }
        else
        {
            group.bounds.Encapsulate(bounds);
        }

        if(group.boundsMinDecalCounter > group.totalTris) // each decal is minimum 1 tri (actual count is unknown / on the GPU) ..... can be 0 actually .... but let's keep it for now
        {
            if (group.boundsCounter == 0)
            {
                group.nextBounds = bounds;
                group.boundsCounter++;
            }
            else
            {
                group.nextBounds.Encapsulate(bounds);
            }

            if (group.boundsMinDecalCounter > group.totalTris * 2)
            {
                group.bounds = group.nextBounds;
                group.boundsCounter = 0;
                group.boundsMinDecalCounter = 0;
            }
        }
        group.boundsMinDecalCounter++;
    }

    public static void RemoveDecal(Handle handle)
    {
        if (handle.mode == Mode.GPU)
        {
            GPUDecalUtils.RemoveDecal(handle);
        }
        else if (handle.mode == Mode.CPUBurst)
        {
            CPUBurstDecalUtils.RemoveDecal(handle);
        }
        else
        {
            CPUDecalUtils.RemoveDecal(handle);
        }
    }

    public static void ReleaseDecals(DecalUtils.Group group, bool affectScene = true)
    {
        if (group == null) return;
        if (group.vbuffer != null)
        {
            group.vbuffer.Release();
            group.vbuffer = null;
        }
        group.totalTris = 0;
        group.maxTrisInDecal = 0;
        if (group.argBuffer != null)
        {
            group.argBuffer.Release();
            group.argBuffer = null;
        }
        if (group.countBuffer != null)
        {
            group.countBuffer.Release();
            group.countBuffer = null;
        }
        group.parent = null;
        group.parentTform = null;
        if (affectScene)
        {
            if (group.go != null)
            {
                UnityEngine.Object.Destroy(group.go);
                group.go = null;
            }
            if (group.mesh != null)
            {
                UnityEngine.Object.Destroy(group.mesh);
                group.mesh = null;
            }
        }
        group.lightmapID = -1;

#if UNITY_2018_1_OR_NEWER
        if (group.nvPos.IsCreated) group.nvPos.Dispose();
        if (group.nvNormal.IsCreated) group.nvNormal.Dispose();
        if (group.nvColor.IsCreated) group.nvColor.Dispose();
        if (group.nvUV2.IsCreated) group.nvUV2.Dispose();
        if (group.nvUV.IsCreated) group.nvUV.Dispose();
        if (group.nvSkin.IsCreated) group.nvSkin.Dispose();
        if (group.nvDecalID.IsCreated) group.nvDecalID.Dispose();
        if (group.nvTangents.IsCreated) group.nvTangents.Dispose();

        if (group.buffWithTangentsNative.IsCreated) group.buffWithTangentsNative.Dispose();
        if (group.buffNative.IsCreated) group.buffNative.Dispose();
#endif
    }

#if USE_TERRAINS
    public class CachedTerrain
    {
        public Vector3[] pos, norm;
        public Vector2[] uv;
        public int[] indices;

#if USE_BURST_REALLY
        public NativeArray<float3> nStaticPos, nStaticNorm;
        public NativeArray<float2> nStaticUV2;
        public NativeArray<int> nStaticTris;
#endif

        public Mesh tempMesh;
    }

    public static Dictionary<TerrainData, CachedTerrain> cachedTerrains = new Dictionary<TerrainData, CachedTerrain>();

    public static CachedTerrain PrepareTerrain(TerrainData terrainData, Vector3 posOffset, bool useBurst = false, bool createMesh = false)
    {
        var cterrain = new DecalUtils.CachedTerrain();
        int res = terrainData.heightmapResolution;
        var heightmap = terrainData.GetHeights(0, 0, res, res);

        float scaleX = terrainData.size.x / (res-1);
        float scaleY = terrainData.size.y;
        float scaleZ = terrainData.size.z / (res-1);
        var uvscale = new Vector2(1,1) / (res-1);

        int vertOffset = 0;
        int indexOffset = 0;

        var staticPos = new Vector3[res*res];
        var staticNorm = new Vector3[res*res];
        var staticUV2 = new Vector2[res*res];
        var staticTris = new int[(res-1) * (res-1) * 2 * 3];
        for (int y=0;y<res;y++)
        {
            for (int x=0;x<res;x++)
            {
                //int index = x * patchResY + y;
                int index = y * res + x;
                float height = heightmap[y,x];

                staticPos[index] = new Vector3(x * scaleX, height * scaleY, y * scaleZ) + posOffset;
                staticUV2[index] = new Vector2(x * uvscale.x, y * uvscale.y);

                staticNorm[index] = terrainData.GetInterpolatedNormal(x / (float)res, y / (float)res);

                if (x < res-1 && y < res-1)
                {
                    staticTris[indexOffset] = vertOffset;
                    staticTris[indexOffset + 1] = vertOffset + res;
                    staticTris[indexOffset + 2] = vertOffset + res + 1;

                    staticTris[indexOffset + 3] = vertOffset;
                    staticTris[indexOffset + 4] = vertOffset + res + 1;
                    staticTris[indexOffset + 5] = vertOffset + 1;

                    indexOffset += 6;
                }

                vertOffset++;
            }
        }

        cterrain.pos = staticPos;
        cterrain.norm = staticNorm;
        cterrain.uv = staticUV2;
        cterrain.indices = staticTris;

#if USE_BURST_REALLY
        if (useBurst)
        {
            int vcount = staticPos.Length;
            var nStaticPosV3  = new NativeArray<Vector3>(vcount*2, Allocator.Persistent); // second half is a temporary scratch buffer
            var nStaticNormV3 = new NativeArray<Vector3>(vcount*2, Allocator.Persistent);
            var nStaticUV2V2  = new NativeArray<Vector2>(vcount*2, Allocator.Persistent);
            NativeArray<Vector3>.Copy(staticPos,  0, nStaticPosV3,  vcount, vcount);
            NativeArray<Vector3>.Copy(staticNorm, 0, nStaticNormV3, vcount, vcount);
            NativeArray<Vector2>.Copy(staticUV2,  0, nStaticUV2V2,  vcount, vcount);
            cterrain.nStaticPos =  nStaticPosV3.Reinterpret<float3>();
            cterrain.nStaticNorm = nStaticNormV3.Reinterpret<float3>();
            cterrain.nStaticUV2 =  nStaticUV2V2.Reinterpret<float2>();

            int icount = staticTris.Length;
            cterrain.nStaticTris  = new NativeArray<int>(icount*2, Allocator.Persistent);
            NativeArray<int>.Copy(staticTris, cterrain.nStaticTris, icount);
        }
#endif

        if (createMesh)
        {
            var mesh = new Mesh();
#if UNITY_2017_3_OR_NEWER
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#endif
            mesh.vertices = staticPos;
            mesh.triangles = staticTris;
            mesh.normals = staticNorm;
            mesh.uv = staticUV2;
            mesh.uv2 = staticUV2;
            cterrain.tempMesh = mesh;
        }

        cachedTerrains[terrainData] = cterrain;
        return cterrain;
    }
#endif

#if USE_TERRAINS
    public static Mesh GetSharedMesh(GameObject obj, out Terrain terrain)
    {
        terrain = null;
        var mf = obj.GetComponent<MeshFilter>();
        if (mf != null)
        {
            return mf.sharedMesh;
        }
        var mrSkin = obj.GetComponent<SkinnedMeshRenderer>();
        if (mrSkin != null)
        {
            return mrSkin.sharedMesh;
        }
        terrain = obj.GetComponent<Terrain>();
        return null;
    }
#else
    public static Mesh GetSharedMesh(GameObject obj)
    {
        var mf = obj.GetComponent<MeshFilter>();
        if (mf != null)
        {
            return mf.sharedMesh;
        }
        var mrSkin = obj.GetComponent<SkinnedMeshRenderer>();
        if (mrSkin != null)
        {
            return mrSkin.sharedMesh;
        }
        return null;
    }
#endif

    public static void AverageNormals(List<Mesh> meshes, bool tangents)
    {
        allNormalsAverage = allNormalsAverage.normalized;
        for(int i=0; i<meshes.Count; i++)
        {
            if (meshes[i] == null) continue;
            var normals = meshes[i].normals;
            int numVerts = normals.Length;
            for(int v=0; v<numVerts; v++)
            {
                normals[v] = allNormalsAverage;
            }
            meshes[i].normals = normals;
            if (tangents) meshes[i].RecalculateTangents();
        }
    }

    public static bool IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, ref RaycastHit hit)
    {
#if UNITY_EDITOR        
        if (_IntersectRayMesh == null)
        {
            var editorTypes = typeof(UnityEditor.Editor).Assembly.GetTypes();
            int numTypes = editorTypes.Length;
            System.Type handleUtilityType = null;
            for(int i=0; i<numTypes; i++)
            {
                if (editorTypes[i].Name == "HandleUtility")
                {
                    handleUtilityType = editorTypes[i];
                    break;
                }
            }
            if (handleUtilityType == null)
            {
                Debug.LogError("Can't find HandleUtility");
                return false;
            }
            _IntersectRayMesh = handleUtilityType.GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic);
            if (_IntersectRayMesh == null)
            {
                Debug.LogError("Can't find IntersectRayMesh in HandleUtility");
                return false;
            }
        }

        _IntersectRayMeshArgs[0] = ray;
        _IntersectRayMeshArgs[1] = mesh;
        _IntersectRayMeshArgs[2] = matrix;
        _IntersectRayMeshArgs[3] = null;

        bool result = (bool)_IntersectRayMesh.Invoke(null, _IntersectRayMeshArgs);
        hit = (RaycastHit)_IntersectRayMeshArgs[3];
        return result;
#else
        return false;
#endif
    }

    public static Color[] ProjectNormals(Vector3[] decalPosW, List<GameObject> parents, Vector3[] decalNorm, Transform decalTform, Color[] decalColor)
    {
        int numVerts = decalPosW.Length;
        int numParents = parents.Count;
        var ray = new Ray();
        var decalNormW = new Vector3[numVerts];
        var hitDist = new float[numVerts];
        var hit2 = new RaycastHit();
        for(int i=0; i<numVerts; i++)
        {
            decalNormW[i] = -decalTform.TransformDirection(decalNorm[i]);
        }
        // Pass new normals to vertex color because alpha only uses R and the whole color is properly projected
        if (decalColor == null || decalColor.Length == 0)
        {
            decalColor = new Color[numVerts];
            var defaultValue = new Color(1,0,0,0);
            for(int i=0; i<numVerts; i++)
            {
                decalColor[i] = defaultValue;
            }
        }
        for(int p=0; p<numParents; p++)
        {
#if USE_TERRAINS
            Terrain terrain;
            var staticMesh = DecalUtils.GetSharedMesh(parents[p], out terrain);
            if (staticMesh == null && terrain == null) continue;
            if (terrain != null) continue; // TODO: use terrains in this mode
#else
            var staticMesh = DecalUtils.GetSharedMesh(parents[p]);
            if (staticMesh == null) continue;
#endif
            var matrix = parents[p].transform.localToWorldMatrix;

            for(int v=0; v<numVerts; v++)
            {
                ray.origin = decalPosW[v];
                ray.direction = decalNormW[v];
                if (DecalUtils.IntersectRayMesh(ray, staticMesh, matrix, ref hit2))
                {
                    float h = hit2.distance;
                    if (hitDist[v] == 0 || h < hitDist[v])
                    {
                        var n = hit2.normal;
                        var clr = decalColor[v];
                        clr.g = n.x;
                        clr.b = n.y;
                        clr.a = n.z;
                        decalColor[v] = clr;
                        hitDist[v] = h;
                    }
                }
            }
        }
        return decalColor;
    }

    public static Vector2[] GetExtraUV(Mesh mesh, int vertCount, int u)
    {
        Vector2[] extraData = null;
        if (u == extraUV3) {
            extraData = mesh.uv3;
        } else if (u == extraUV4) {
            extraData = mesh.uv4;
        } else if (u == extraUV5) {
            extraData = mesh.uv5;
        } else if (u == extraUV6) {
            extraData = mesh.uv6;
        } else if (u == extraUV7) {
            extraData = mesh.uv7;
        } else if (u == extraUV8) {
            extraData = mesh.uv8;
        } else if (u == extraUV1) {
            extraData = mesh.uv;
        }
        if (extraData == null || extraData.Length == 0)
        {
            extraData = new Vector2[vertCount];
        }
        return extraData;
    }

    public static void SetExtraUV(Mesh mesh, int u, Vector2[] src)
    {
        if (u == extraUV3) {
            mesh.uv3 = src;
        } else if (u == extraUV4) {
            mesh.uv4 = src;
        } else if (u == extraUV5) {
            mesh.uv5 = src;
        } else if (u == extraUV6) {
            mesh.uv6 = src;
        } else if (u == extraUV7) {
            mesh.uv7 = src;
        } else if (u == extraUV8) {
            mesh.uv8 = src;
        } else if (u == extraUV1) {
            mesh.uv = src;
        }
    }

    public static void ProjectLightmapUV(Mesh mesh, List<GameObject> parents, bool takeSeamsIntoAccount)
    {
        var decalPosW = mesh.vertices;
        var decalNormW = mesh.normals;

        int numVerts = decalPosW.Length;
        int numParents = parents.Count;
        var ray = new Ray();
        var hit2 = new RaycastHit();

        Vector2[] extraDataIn;
        var extraDataOut = new Vector2[numVerts];

        var bias = Vector3.one * 0.001f;
        const float dirSign = -1;

        for(int p=0; p<numParents; p++)
        {
#if USE_TERRAINS
            Terrain terrain;
            var staticMesh = DecalUtils.GetSharedMesh(parents[p], out terrain);
#else
            var staticMesh = DecalUtils.GetSharedMesh(parents[p]);
#endif
            if (staticMesh == null) continue;

            int staticVertCount = staticMesh.vertexCount;
            var staticTris = staticMesh.triangles;
            Vector3[] staticPosW = null;

            extraDataIn = staticMesh.uv2;
            if (extraDataIn == null || extraDataIn.Length == 0)
            {
                extraDataIn = staticMesh.uv;
            }
            if (extraDataIn == null || extraDataIn.Length == 0)
            {
                Debug.LogError("No uv on "+parents[p].name);
                continue;
            }

            var matrix = parents[p].transform.localToWorldMatrix;

            for(int v=0; v<numVerts; v++)
            {
                ray.origin = decalPosW[v] + decalNormW[v] * 0.01f;// + bias;

                if (takeSeamsIntoAccount)
                {
                    int a = (v/3)*3;
                    int b = a+1;
                    int c = a+2;
                    var triCenter = (decalPosW[a] + decalPosW[b] + decalPosW[c]) / 3.0f;
                    var dirToCenter = (triCenter - decalPosW[v]).normalized;
                    ray.origin += dirToCenter * 0.01f;

                    //var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //g.transform.localScale *= 0.01f;
                    //g.transform.position = ray.origin;
                }

                ray.direction = decalNormW[v] * dirSign;
                bool isHit = DecalUtils.IntersectRayMesh(ray, staticMesh, matrix, ref hit2);
                if (!isHit)
                {
                    //ray.origin -= bias*2;
                    isHit = DecalUtils.IntersectRayMesh(ray, staticMesh, matrix, ref hit2);
                    if (!isHit)
                    {
                        if (staticPosW == null)
                        {
                            staticPosW = staticMesh.vertices;
                            for(int v2=0; v2<staticVertCount; v2++)
                            {
                                staticPosW[v2] = matrix.MultiplyPoint3x4(staticPosW[v2]);
                            }
                        }
                        float closestDist = float.MaxValue;
                        int closest = 0;
                        for(int v2=0; v2<staticVertCount; v2++)
                        {
                            float dist = (ray.origin - staticPosW[v2]).sqrMagnitude;
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closest = v2;
                            }
                        }
                        extraDataOut[v] = extraDataIn[closest];
                    }
                }
                if (isHit)
                {
                    int firstIndex = hit2.triangleIndex * 3;
                    var bary = hit2.barycentricCoordinate;
                    var _b = staticTris[firstIndex];
                    var b = extraDataIn[_b];
                    var c = extraDataIn[staticTris[firstIndex + 1]];
                    var a = extraDataIn[staticTris[firstIndex + 2]];
                    extraDataOut[v] = a*bary.x + b*bary.y + c*bary.z;
                }
            }
        }

        mesh.uv2 = extraDataOut;
    }

    public static void ProjectExtraData(Mesh mesh, List<GameObject> parents, int extraDataMask, float dirSign, int moveUV0To = extraUV1)
    {
        var decalPosW = mesh.vertices;
        var decalNormW = mesh.normals;

        int numVerts = decalPosW.Length;
        int numParents = parents.Count;
        var ray = new Ray();
        var hit2 = new RaycastHit();

        var extraDataIn = new Vector2[extraUVCount][];
        var extraDataOut = new Vector2[extraUVCount][];
        for(int u=0; u<extraUVCount; u++)
        {
            if ((extraDataMask & (1 << u)) != 0)
            {
                extraDataOut[u] = new Vector2[numVerts];
            }
        }

        var bias = Vector3.one * 0.001f;

        for(int p=0; p<numParents; p++)
        {
#if USE_TERRAINS
            Terrain terrain;
            var staticMesh = DecalUtils.GetSharedMesh(parents[p], out terrain);
#else
            var staticMesh = DecalUtils.GetSharedMesh(parents[p]);
#endif
            if (staticMesh == null) continue;

            int staticVertCount = staticMesh.vertexCount;
            var staticTris = staticMesh.triangles;
            Vector3[] staticPosW = null;

            for(int u=0; u<extraUVCount; u++)
            {
                if ((extraDataMask & (1 << u)) != 0)
                {
                    extraDataIn[u] = GetExtraUV(staticMesh, staticVertCount, u);
                }
            }

            var matrix = parents[p].transform.localToWorldMatrix;

            for(int v=0; v<numVerts; v++)
            {
                ray.origin = decalPosW[v] + decalNormW[v] * 0.01f + bias;
                ray.direction = decalNormW[v] * dirSign;
                bool isHit = DecalUtils.IntersectRayMesh(ray, staticMesh, matrix, ref hit2);
                if (!isHit)
                {
                    ray.origin -= bias*2;
                    isHit = DecalUtils.IntersectRayMesh(ray, staticMesh, matrix, ref hit2);
                    if (!isHit)
                    {
                        if (staticPosW == null)
                        {
                            staticPosW = staticMesh.vertices;
                            for(int v2=0; v2<staticVertCount; v2++)
                            {
                                staticPosW[v2] = matrix.MultiplyPoint3x4(staticPosW[v2]);
                            }
                        }
                        float closestDist = float.MaxValue;
                        int closest = 0;
                        for(int v2=0; v2<staticVertCount; v2++)
                        {
                            float dist = (ray.origin - staticPosW[v2]).sqrMagnitude;
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closest = v2;
                            }
                        }
                        for(int u=0; u<extraUVCount; u++)
                        {
                            if ((extraDataMask & (1 << u)) != 0)
                            {
                                var dataIn = extraDataIn[u];
                                var dataOut = extraDataOut[u];
                                dataOut[v] = dataIn[closest];
                            }
                        }
                    }
                }
                if (isHit)
                {
                    int firstIndex = hit2.triangleIndex * 3;
                    var bary = hit2.barycentricCoordinate;
                    for(int u=0; u<extraUVCount; u++)
                    {
                        if ((extraDataMask & (1 << u)) != 0)
                        {
                            var dataIn = extraDataIn[u];
                            var dataOut = extraDataOut[u];
                            var b = dataIn[staticTris[firstIndex]];
                            var c = dataIn[staticTris[firstIndex + 1]];
                            var a = dataIn[staticTris[firstIndex + 2]];
                            dataOut[v] = a*bary.x + b*bary.y + c*bary.z;
                        }
                    }
                }
            }
        }

        if ((extraDataMask & (1<<extraUV1)) != 0 && moveUV0To != extraUV1)
        {
            SetExtraUV(mesh, moveUV0To, mesh.uv);
        }

        for(int u=0; u<extraUVCount; u++)
        {
            if ((extraDataMask & (1 << u)) != 0)
            {
                SetExtraUV(mesh, u, extraDataOut[u]);
            }
        }
    }
/*
    static void SaveArray(BinaryWriter f, Vector4[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].x);
            f.Write(arr[i].y);
            f.Write(arr[i].z);
            f.Write(arr[i].w);
        }
    }

    static void SaveArray(BinaryWriter f, Vector3[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].x);
            f.Write(arr[i].y);
            f.Write(arr[i].z);
        }
    }

    static void SaveArray(BinaryWriter f, Vector2[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].x);
            f.Write(arr[i].y);
        }
    }

    static void SaveArray(BinaryWriter f, Color[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].r);
            f.Write(arr[i].g);
            f.Write(arr[i].b);
            f.Write(arr[i].a);
        }
    }

    static void SaveArray(BinaryWriter f, BoneWeight[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].boneIndex0);
            f.Write(arr[i].boneIndex1);
            f.Write(arr[i].boneIndex2);
            f.Write(arr[i].boneIndex3);
            f.Write(arr[i].weight0);
            f.Write(arr[i].weight1);
            f.Write(arr[i].weight2);
            f.Write(arr[i].weight3);
        }
    }

    static void SaveArray(BinaryWriter f, int[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i]);
        }
    }

    static void SaveArray(BinaryWriter f, byte[] arr)
    {
        if (arr == null) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i]);
        }
    }

#if UNITY_2018_1_OR_NEWER
    static void SaveArray(BinaryWriter f, NativeArray<Vector4> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].x);
            f.Write(arr[i].y);
            f.Write(arr[i].z);
            f.Write(arr[i].w);
        }
    }

    static void SaveArray(BinaryWriter f, NativeArray<Vector3> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].x);
            f.Write(arr[i].y);
            f.Write(arr[i].z);
        }
    }

    static void SaveArray(BinaryWriter f, NativeArray<Vector2> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].x);
            f.Write(arr[i].y);
        }
    }

    static void SaveArray(BinaryWriter f, NativeArray<Color> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].r);
            f.Write(arr[i].g);
            f.Write(arr[i].b);
            f.Write(arr[i].a);
        }
    }

    static void SaveArray(BinaryWriter f, NativeArray<BoneWeight> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i].boneIndex0);
            f.Write(arr[i].boneIndex1);
            f.Write(arr[i].boneIndex2);
            f.Write(arr[i].boneIndex3);
            f.Write(arr[i].weight0);
            f.Write(arr[i].weight1);
            f.Write(arr[i].weight2);
            f.Write(arr[i].weight3);
        }
    }

    static void SaveArray(BinaryWriter f, NativeArray<int> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i]);
        }
    }

    static void SaveArray(BinaryWriter f, NativeArray<byte> arr)
    {
        if (arr == null || !arr.IsCreated) { f.Write(0); return; }
        int cnt = arr.Length;
        f.Write(cnt);
        for(int i=0; i<cnt; i++)
        {
            f.Write(arr[i]);
        }
    }
#endif


    static Vector4[] LoadArray4(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return null;
        var arr = new Vector4[cnt];
        var v = new Vector4();
        for(int i=0; i<cnt; i++)
        {
            v.x = f.ReadSingle();
            v.y = f.ReadSingle();
            v.z = f.ReadSingle();
            v.w = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static Vector3[] LoadArray3(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        Debug.LogError(cnt);
        if (cnt == 0) return null;
        var arr = new Vector3[cnt];
        var v = new Vector3();
        for(int i=0; i<cnt; i++)
        {
            v.x = f.ReadSingle();
            v.y = f.ReadSingle();
            v.z = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static Vector2[] LoadArray2(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return null;
        var arr = new Vector2[cnt];
        var v = new Vector2();
        for(int i=0; i<cnt; i++)
        {
            v.x = f.ReadSingle();
            v.y = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static Color[] LoadArrayC(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return null;
        var arr = new Color[cnt];
        var v = new Color();
        for(int i=0; i<cnt; i++)
        {
            v.r = f.ReadSingle();
            v.g = f.ReadSingle();
            v.b = f.ReadSingle();
            v.a = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static BoneWeight[] LoadArrayB(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return null;
        var arr = new BoneWeight[cnt];
        var v = new Color();
        for(int i=0; i<cnt; i++)
        {
            arr[i].boneIndex0 = f.ReadInt32();
            arr[i].boneIndex1 = f.ReadInt32();
            arr[i].boneIndex2 = f.ReadInt32();
            arr[i].boneIndex3 = f.ReadInt32();
            arr[i].weight0 = f.ReadSingle();
            arr[i].weight1 = f.ReadSingle();
            arr[i].weight2 = f.ReadSingle();
            arr[i].weight3 = f.ReadSingle();
        }
        return arr;
    }

    static int[] LoadArrayInt(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return null;
        var arr = new int[cnt];
        for(int i=0; i<cnt; i++)
        {
            arr[i] = f.ReadInt32();
        }
        return arr;
    }

    static byte[] LoadArrayByte(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return null;
        var arr = new byte[cnt];
        for(int i=0; i<cnt; i++)
        {
            arr[i] = f.ReadByte();
        }
        return arr;
    }


#if UNITY_2018_1_OR_NEWER
    static NativeArray<Vector4> LoadArrayN4(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<Vector4>();
        var arr = new NativeArray<Vector4>(cnt, Allocator.Persistent);
        var v = new Vector4();
        for(int i=0; i<cnt; i++)
        {
            v.x = f.ReadSingle();
            v.y = f.ReadSingle();
            v.z = f.ReadSingle();
            v.w = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static NativeArray<Vector3> LoadArrayN3(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<Vector3>();
        var arr = new NativeArray<Vector3>(cnt, Allocator.Persistent);
        var v = new Vector3();
        for(int i=0; i<cnt; i++)
        {
            v.x = f.ReadSingle();
            v.y = f.ReadSingle();
            v.z = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static NativeArray<Vector2> LoadArrayN2(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<Vector2>();
        var arr = new NativeArray<Vector2>(cnt, Allocator.Persistent);
        var v = new Vector2();
        for(int i=0; i<cnt; i++)
        {
            v.x = f.ReadSingle();
            v.y = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static NativeArray<Color> LoadArrayNC(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<Color>();
        var arr = new NativeArray<Color>(cnt, Allocator.Persistent);
        var v = new Color();
        for(int i=0; i<cnt; i++)
        {
            v.r = f.ReadSingle();
            v.g = f.ReadSingle();
            v.b = f.ReadSingle();
            v.a = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static NativeArray<BoneWeight> LoadArrayNB(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<BoneWeight>();
        var arr = new NativeArray<BoneWeight>(cnt, Allocator.Persistent);
        var v = new BoneWeight();
        for(int i=0; i<cnt; i++)
        {
            v.boneIndex0 = f.ReadInt32();
            v.boneIndex1 = f.ReadInt32();
            v.boneIndex2 = f.ReadInt32();
            v.boneIndex3 = f.ReadInt32();
            v.weight0 = f.ReadSingle();
            v.weight1 = f.ReadSingle();
            v.weight2 = f.ReadSingle();
            v.weight3 = f.ReadSingle();
            arr[i] = v;
        }
        return arr;
    }

    static NativeArray<int> LoadArrayNInt(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<int>();
        var arr = new NativeArray<int>(cnt, Allocator.Persistent);
        for(int i=0; i<cnt; i++)
        {
            arr[i] = f.ReadInt32();
        }
        return arr;
    }

    static NativeArray<byte> LoadArrayNByte(BinaryReader f)
    {
        int cnt = f.ReadInt32();
        if (cnt == 0) return new NativeArray<byte>();
        var arr = new NativeArray<byte>(cnt, Allocator.Persistent);
        for(int i=0; i<cnt; i++)
        {
            arr[i] = f.ReadByte();
        }
        return arr;
    }
#endif



    public static byte[] SaveState(List<DecalSpawner> spawners)
    {
        byte[] buffer = new byte[32 * 1024 * 1024];
        var ms = new MemoryStream(buffer);
        var f = new BinaryWriter(ms);

        int count = spawners.Count;
        f.Write(count);

        for(int i=0; i<count; i++)
        {
            var spawner = spawners[i];
            int numStatic = spawner.staticGroups.Count;
            f.Write(numStatic);

            foreach(var pair in spawner.staticGroups)
            {
                //f.Write(pair.Key);

                var grp = pair.Value;

                f.Write(grp.lightmapID);
                f.Write((int)grp.mode);

                f.Write(grp.numDecals);
                f.Write(grp.maxTrisInDecal);
                f.Write(grp.totalTris);
                f.Write(grp.boundsCounter);
                f.Write(grp.boundsMinDecalCounter);
                f.Write(grp.decalIDCounter);
                f.Write(grp.fixedOffset);

                f.Write(grp.isTrail);
                f.Write(grp.tangents);
                f.Write(grp.isSkinned);
                f.Write(grp.indirectDraw);

                f.Write(grp.trailV);
                f.Write(grp.trailVScale);

                f.Write(grp.bounds.center.x);
                f.Write(grp.bounds.center.y);
                f.Write(grp.bounds.center.z);
                f.Write(grp.bounds.size.x);
                f.Write(grp.bounds.size.y);
                f.Write(grp.bounds.size.z);

                f.Write(grp.nextBounds.center.x);
                f.Write(grp.nextBounds.center.y);
                f.Write(grp.nextBounds.center.z);
                f.Write(grp.nextBounds.size.x);
                f.Write(grp.nextBounds.size.y);
                f.Write(grp.nextBounds.size.z);

                SaveArray(f, grp.vPos);
                SaveArray(f, grp.vNormal);
                SaveArray(f, grp.vUV);
                SaveArray(f, grp.vUV2);
                SaveArray(f, grp.vColor);
                SaveArray(f, grp.vSkin);
                SaveArray(f, grp.vTangents);
                SaveArray(f, grp.vDecalID);

#if UNITY_2018_1_OR_NEWER
                SaveArray(f, grp.nvPos);
                SaveArray(f, grp.nvNormal);
                SaveArray(f, grp.nvColor);
                SaveArray(f, grp.nvUV);
                SaveArray(f, grp.nvUV2);
                SaveArray(f, grp.nvSkin);
                SaveArray(f, grp.nvTangents);
                SaveArray(f, grp.nvDecalID);
#endif
            }

            int numMovable = spawner.movableGroups.Count;
            f.Write(numMovable);

            foreach(var pair in spawner.movableGroups)
            {
                
            }
        }

        f.Close();
        return buffer;
    }

    public static void LoadState(List<DecalSpawner> spawners, byte[] state)
    {
        var ms = new MemoryStream(state);
        var f = new BinaryReader(ms);

        int count = f.ReadInt32();
        if (count != spawners.Count)
        {
            Debug.LogError("Saved spawners count is different from current count.");
            return;
        }

        Vector3 center, size;

        for(int i=0; i<count; i++)
        {
            spawners[i].Clear();
        }

        for(int i=0; i<count; i++)
        {
            int numStatic = f.ReadInt32();
            for(int s=0; s<numStatic; s++)
            {
                int lmid = f.ReadInt32();
                Mode mode = (Mode)f.ReadInt32();

                int j = 0;
                DecalUtils.Group grp = null;
                if (!spawners[j].staticGroups.TryGetValue(lmid, out grp))
                {
                    spawners[j].initDesc.lightmapID = lmid;
                    spawners[j].initDesc.materialPropertyBlock = null;
                    grp = DecalUtils.CreateGroup(spawners[j].initDesc, mode);
                }

                grp.numDecals = f.ReadInt32();
                grp.maxTrisInDecal = f.ReadInt32();
                grp.totalTris = f.ReadInt32();
                grp.boundsCounter = f.ReadInt32();
                grp.boundsMinDecalCounter = f.ReadInt32();
                grp.decalIDCounter = f.ReadInt32();
                grp.fixedOffset = f.ReadInt32();

                grp.isTrail = f.ReadBoolean();
                grp.tangents = f.ReadBoolean();
                grp.isSkinned = f.ReadBoolean();
                grp.indirectDraw = f.ReadBoolean();

                grp.trailV = f.ReadSingle();
                grp.trailVScale = f.ReadSingle();

                center.x = f.ReadSingle();
                center.y = f.ReadSingle();
                center.z = f.ReadSingle();
                size.x = f.ReadSingle();
                size.y = f.ReadSingle();
                size.z = f.ReadSingle();
                grp.bounds = new Bounds(center, size);

                center.x = f.ReadSingle();
                center.y = f.ReadSingle();
                center.z = f.ReadSingle();
                size.x = f.ReadSingle();
                size.y = f.ReadSingle();
                size.z = f.ReadSingle();
                grp.nextBounds = new Bounds(center, size);

                grp.vPos = LoadArray3(f);
                grp.vNormal = LoadArray3(f);
                grp.vUV = LoadArray2(f);
                grp.vUV2 = LoadArray2(f);
                grp.vColor = LoadArrayC(f);
                grp.vSkin = LoadArrayB(f);
                grp.vTangents = LoadArray4(f);
    #if LONG_DECALID
                grp.vDecalID = LoadArrayInt(f);
    #else
                grp.vDecalID = LoadArrayByte(f);
    #endif

    #if UNITY_2018_1_OR_NEWER
                grp.nvPos = LoadArrayN3(f);
                grp.nvNormal = LoadArrayN3(f);
                grp.nvColor = LoadArrayNC(f);
                grp.nvUV = LoadArrayN2(f);
                grp.nvUV2 = LoadArrayN2(f);
                grp.nvSkin = LoadArrayNB(f);
                grp.nvTangents = LoadArrayN4(f);
    #if LONG_DECALID
                grp.nvDecalID = LoadArrayNInt(f);
    #else
                grp.nvDecalID = LoadArrayNByte(f);
    #endif
    #endif
            }
        }

        int numMovable = f.ReadInt32();

        f.Close();
    }
*/
}

