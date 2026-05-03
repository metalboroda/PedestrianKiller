using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


public class CPUDecalOptimizer
{
    const int GLU_TESS_VERTEX = 100101;
    const int GLU_TESS_ERROR = 100103;
    const int GLU_TESS_EDGE_FLAG = 100104;
    const int GLU_TESS_COMBINE = 100105;

    const int GLU_TESS_NEED_COMBINE_CALLBACK = 100156;
    
    [DllImport ("glu32.dll")]
    private static extern System.IntPtr gluNewTess();

    [DllImport ("glu32.dll")]
    private static extern void gluDeleteTess(System.IntPtr tess);

    [DllImport ("glu32.dll")]
    private static extern void gluTessBeginPolygon(System.IntPtr tess, System.IntPtr userData);

    [DllImport ("glu32.dll")]
    private static extern void gluTessEndPolygon(System.IntPtr tess);

    [DllImport ("glu32.dll")]
    private static extern void gluTessBeginContour(System.IntPtr tess);

    [DllImport ("glu32.dll")]
    private static extern void gluTessEndContour(System.IntPtr tess);

    [DllImport ("glu32.dll")]
    private static extern void gluTessVertex(System.IntPtr tess, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)]double[] coords, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]int[] data);

    [DllImport ("glu32.dll")]
    private static extern void gluTessCallback(System.IntPtr tess, int which, System.Delegate fn);

    public delegate void TessVertexCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]int[] data);
    public delegate void TessCombineCallback([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)]double[] coords, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)]System.IntPtr[] data, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)]float[] weight, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]int[] outData);
    public delegate void TessErrorCallback(uint errorCode);
    public delegate void TessEdgeCallback(byte flag);

    static List<int> tessGeneratedList;
    static uint tessLastError = 0;

    static TessVertexCallback tessVertexCallback = (data) =>
    {
        tessGeneratedList.Add(data[0]);
    };

    static TessVertexCallback tessEdgeCallback = (flag) =>
    {
    };

    static TessErrorCallback tessErrorCallback = (errorCode) =>
    {
        Debug.LogWarning("Tessellator failed on a polygon with error code: "+errorCode+" (will be kept as original triangles)");
        tessLastError = errorCode;
    };

    public static void Optimize(ref List<Vector3> newPos, ref List<Vector3> newNormal, ref List<Vector2> newUV, ref List<Vector2> newUV2, ref List<BoneWeight> newSkin, ref List<Color> newColor, ref List<int> newIndices)
    {
        int numVerts = newPos.Count;
        int numTris = newIndices.Count / 3;
        int numIntsForTris = numTris / 32 + 1;
        tessGeneratedList = new List<int>();

        // Collect edges and tri normals
        var triEdges = new int[numTris * 3];
        var triNormals = new Vector3[numTris];
        for(int t=0; t<numTris; t++)
        {
            int v0 = newIndices[t*3];
            int v1 = newIndices[t*3+1];
            int v2 = newIndices[t*3+2];
            int edge0 = v0<v1? (v0|(v1<<16)) : (v1|(v0<<16));
            int edge1 = v1<v2? (v1|(v2<<16)) : (v2|(v1<<16));
            int edge2 = v2<v0? (v2|(v0<<16)) : (v0|(v2<<16));
            triEdges[t*3] = edge0;
            triEdges[t*3+1] = edge1;
            triEdges[t*3+2] = edge2;
            triNormals[t] = Vector3.Cross(newPos[v0] - newPos[v1], newPos[v1] - newPos[v2]).normalized;
        }

#if DEBUGTESS
        var debugLines = new List<List<Vector3>>();
#endif
        // Iterate over tris
        var processedTris = new int[numIntsForTris];
        bool anyTrisLeft = true;
        while(true)
        {
            int startTri = -1;
            anyTrisLeft = false;
            for(int t=0; t<numTris; t++)
            {
                if ((processedTris[t>>5] & (1 << (t&31))) == 0)
                {
                    startTri = t;
                    anyTrisLeft = true;
                    break;
                }
            }
            if (!anyTrisLeft) break;

            var polygonTriList = new List<int>();
            polygonTriList.Add(startTri);
            int polyProcessed = 0;
#if DEBUGTESS
            var debugLine = new List<Vector3>();
#endif
            // Collect tris over edges until full poly
            while(polyProcessed < polygonTriList.Count)
            {
                int currentTri = polygonTriList[polyProcessed];
                processedTris[currentTri>>5] |= 1 << (currentTri&31);

                /*if (triNormals[currentTri] == Vector3.zero) // degenerate triangle
                {
                    polyProcessed++;
                    continue;
                }*/

                // Iterate over tri edges
                for(int e=0; e<3; e++)
                {
                    int edge = triEdges[currentTri*3+e];
                    bool foundOpposite = false;
                    for(int t=0; t<numTris; t++)
                    {
                        if (t==currentTri) continue;
                        if ((processedTris[t>>5] & (1 << (t&31))) != 0) continue;
                        for(int e2=0; e2<3; e2++)
                        {
                            int edge2 = triEdges[t*3+e2];
                            if (edge == edge2)
                            {
                                if (Vector3.Dot(triNormals[currentTri], triNormals[t]) < 0.999999f) continue; // don't go across normal edges

                                //if (triNormals[t] == Vector3.zero) continue; // degenerate triangle

                                polygonTriList.Add(t);
                                processedTris[t>>5] |= 1 << (t&31);
                                
                                //var a = newPos[edge & 0xFFFF];
                                //var b = newPos[edge >> 16];
                                //debugLine.Add(a);
                                //debugLine.Add(b);

                                foundOpposite = true;
                                break;
                            }
                        }
                        if (foundOpposite) break;
                    }
                }

                polyProcessed++;
            }

            // Find open edges
            var openEdges = new List<int>();
            for(int i=0; i<polygonTriList.Count; i++)
            {
                int t = polygonTriList[i];
                for(int e=0; e<3; e++)
                {
                    int edge = triEdges[t*3+e];
                    bool openEdge = true;
                    for(int j=0; j<polygonTriList.Count; j++)
                    {
                        if (i==j) continue;
                        int t2 = polygonTriList[j];
                        for(int e2=0; e2<3; e2++)
                        {
                            int edge2 = triEdges[t2*3+e2];
                            if (edge == edge2)
                            {
                                openEdge = false;
                                break;
                            }
                        }
                        if (!openEdge) break;
                    }
                    if (openEdge)
                    {
                        openEdges.Add(edge);
                        //var a = newPos[edge & 0xFFFF];
                        //var b = newPos[edge >> 16];
                        //debugLine.Add(a);
                        //debugLine.Add(b);
                    }
                }
            }

            // Sort open edges
            var sortedVerts = new List<int>();
            var sortedVertsContourID = new List<int>();
            int edgesLeft = openEdges.Count;
            var processedEdges = new int[edgesLeft/32+1];
            int curEdgeIndex = 0;
            int contourID = 0;
            bool reverse = false;
            while(edgesLeft > 0)
            {
                int edge = openEdges[curEdgeIndex];
                var a = edge & 0xFFFF;
                var b = edge >> 16;
                if (reverse)
                {
                    int tmp = b;
                    b = a;
                    a = tmp;
                }
                sortedVerts.Add(a);
                sortedVertsContourID.Add(contourID);
                //sortedVerts.Add(b);

                //var va = newPos[a];
                //debugLine.Add(va);

                edgesLeft--;
                processedEdges[curEdgeIndex>>5] |= 1<<(curEdgeIndex&31);
                bool foundNextEdge = false;
                reverse = false;
                for(int j=0; j<openEdges.Count; j++)
                {
                    if (curEdgeIndex==j) continue;
                    int edge2 = openEdges[j];
                    if ((processedEdges[j>>5] & (1<<(j&31))) != 0) continue;

                    var a2 = edge2 & 0xFFFF;
                    var b2 = edge2 >> 16;
                    if (b == a2)
                    {
                        curEdgeIndex = j;
                        foundNextEdge = true;
                        break;
                    }
                    else if (b == b2)
                    {
                        curEdgeIndex = j;
                        foundNextEdge = true;
                        reverse = true;
                        break;
                    }
                }
                if (!foundNextEdge)
                {
                    contourID++;
                    for(int i=0; i<openEdges.Count; i++)
                    {
                        if ((processedEdges[i>>5] & (1<<(i&31))) == 0)
                        {
                            foundNextEdge = true;
                            curEdgeIndex = i;
                        }
                    }
                    if (!foundNextEdge) break;
                }
            }

            // Clean open edges
            var cleanedVerts = new List<int>();
            var cleanedVertsContourID = new List<int>();
            for(int i=0; i<sortedVerts.Count; i++)
            {
                int curID = i;
                int prevID = i-1;
                if (prevID < 0) prevID = sortedVerts.Count-1;
                int nextID = i+1;
                if (nextID == sortedVerts.Count) nextID = 0;

                var prevPos = newPos[sortedVerts[prevID]];
                var curPos =  newPos[sortedVerts[curID]];
                var nextPos = newPos[sortedVerts[nextID]];

                var deltaA = curPos - prevPos;
                //if (deltaA.sqrMagnitude < 0.0001f) continue;

                var deltaB = nextPos - curPos;
                var dirA = deltaA.normalized;
                var dirB = deltaB.normalized;
                float dt = Vector3.Dot(dirA, dirB);
                if (dt > 0.9999f) continue;
                //if (dt < -0.99999f) continue;

                cleanedVerts.Add(sortedVerts[curID]);
                cleanedVertsContourID.Add(sortedVertsContourID[curID]);
#if DEBUGTESS
                debugLine.Add(curPos);
#endif
            }

            // Run GLU tessellator
            int numContours = contourID;
            contourID = 0;
            int voffset = 0;
            var tess = gluNewTess();
            gluTessCallback(tess, GLU_TESS_VERTEX, tessVertexCallback);
            gluTessCallback(tess, GLU_TESS_EDGE_FLAG, tessEdgeCallback); // forces tri list topology
            //gluTessCallback(tess, GLU_TESS_COMBINE, tessCombineCallback);
            gluTessCallback(tess, GLU_TESS_ERROR, tessErrorCallback);
            gluTessBeginPolygon(tess, (System.IntPtr)null);
            var coordsList = new List<double[]>();
            var indexList = new List<int[]>();
            int offsetInTessGeneratedList = tessGeneratedList.Count;
            for(int c=0; c<numContours; c++)
            {
                gluTessBeginContour(tess);
                for(int v=voffset; v<cleanedVerts.Count; v++)
                {
                    var coords = new double[3];
                    coords[0] = (double)newPos[cleanedVerts[v]].x;
                    coords[1] = (double)newPos[cleanedVerts[v]].y;
                    coords[2] = (double)newPos[cleanedVerts[v]].z;
                    coordsList.Add(coords);
                    var indexArr = new int[1];
                    indexArr[0] = cleanedVerts[v];
                    indexList.Add(indexArr);
                    //Debug.LogError("Push "+v);
                    gluTessVertex(tess, coordsList[coordsList.Count-1],   indexList[indexList.Count-1]);
                    //Debug.LogError("? "+coords[0]+" "+coords[1]+" "+coords[2]);

                    if (cleanedVertsContourID[v] != contourID)
                    {
                        voffset = v;
                        break;
                    }
                }
                gluTessEndContour(tess);
            }
            gluTessEndPolygon(tess);
            gluDeleteTess(tess);

            int o = offsetInTessGeneratedList;
            bool useOrig = false;
            if (o == tessGeneratedList.Count)
            {
                // bugged polygon - revert to standard
                useOrig = true;
            }
            else
            {
                // Fix winding after tessellation
                var origNormal = triNormals[polygonTriList[0]];
                var testNormal = Vector3.Cross(newPos[tessGeneratedList[o]] - newPos[tessGeneratedList[o+1]], newPos[tessGeneratedList[o+1]] - newPos[tessGeneratedList[o+2]]).normalized;
                if (testNormal == Vector3.zero)
                {
                    // tessellator produced degenerate triangle - revert to standard
                    useOrig = true;
                }
                else
                {
                    if (Vector3.Dot(origNormal, testNormal) < 0)
                    {
                        for(int i=o; i<tessGeneratedList.Count; i+=3)
                        {
                            int a = tessGeneratedList[i];
                            //int b = tessGeneratedList[i+1];
                            int c = tessGeneratedList[i+2];
                            tessGeneratedList[i] = c;
                            tessGeneratedList[i+2] = a;
                        }
                    }
                }
            }
            if (useOrig)
            {
                for(int i=0; i<polygonTriList.Count; i++)
                {
                    int t = polygonTriList[i];
                    for(int v=0; v<3; v++)
                    {
                        tessGeneratedList.Add(newIndices[t*3+v]);
                    }
                }
            }
            tessLastError = 0;

#if DEBUGTESS
            //string str = "";
            //for(int i=0; i<polygonTriList.Count; i++) str += polygonTriList[i]+", ";
            //Debug.LogError("Poly "+polygons.Count+": "+str);
            //polygons.Add(polygonTriList);
            debugLines.Add(debugLine);
#endif
        }

        // Assign from the tessellator
        var newPos2 = new List<Vector3>();
        var newNormal2 = new List<Vector3>();
        var newUV_2 = new List<Vector2>();
        var newUV2_2 = new List<Vector2>();
        var newSkin2 = new List<BoneWeight>();
        var newColor2 = new List<Color>();
        var newIndices2 = new List<int>();
        //Debug.LogError("Polys: "+tessGeneratedList.Count);
        for(int i=0; i<tessGeneratedList.Count; i++)
        {
            int idx = tessGeneratedList[i];
            newPos2.Add(newPos[idx]);
            newNormal2.Add(newNormal[idx]);
            newUV_2.Add(newUV[idx]);
            newUV2_2.Add(newUV2[idx]);
            if (newSkin != null && newSkin.Count > 0) newSkin2.Add(newSkin[idx]);
            if (newColor != null && newColor.Count > 0) newColor2.Add(newColor[idx]);
            newIndices2.Add(i);
        }
        newPos = newPos2;
        newNormal = newNormal2;
        newUV = newUV_2;
        newUV2 = newUV2_2;
        newSkin = newSkin2;
        newColor = newColor2;
        newIndices = newIndices2;

    }
}

