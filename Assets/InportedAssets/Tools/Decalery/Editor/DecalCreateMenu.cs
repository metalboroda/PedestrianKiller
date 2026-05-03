#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class DecalCreateMenu
{
    [MenuItem("Decalery/Create/Quad decal", false, 20)]
    private static void CreateQuadDecal()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Undo.RegisterCreatedObjectUndo(go, "Create quad decal");
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        go.AddComponent<DecalGroup>();
        go.name = "Decal";
        var ecam = SceneView.lastActiveSceneView.camera.transform;
        go.transform.position = ecam.position + ecam.forward;
        //go.transform.eulerAngles = new Vector3(50, -30, 0);
        var arr = new GameObject[1];
        arr[0] = go;
        Selection.objects = arr;
    }
}

#endif