#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GizmoResourceLoader
{
#if UNITY_EDITOR
    private static GizmoResourceSO _cached;

    public static GizmoResourceSO Get()
    {
        if (_cached == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:GizmoResourceSO");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cached = AssetDatabase.LoadAssetAtPath<GizmoResourceSO>(path);
            }
        }
        return _cached;
    }
#endif
}
