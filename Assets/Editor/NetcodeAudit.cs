#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Unity.Netcode.Components;

public static class NetcodeAudit
{
    [MenuItem("QuirkyCooks/Netcode/Print NetworkTransforms (Scene)")]
    static void PrintScene()
    {
#if UNITY_2023_1_OR_NEWER
        var nts = Object.FindObjectsByType<NetworkTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var nts = Object.FindObjectsOfType<NetworkTransform>(true);
#endif
        Debug.Log($"[Audit] NetworkTransform in SCENE = {nts.Length}");
        foreach (var nt in nts)
        {
            string path = GetPath(nt.transform);
            Debug.Log($"[Audit][Scene] {path}", nt.gameObject);
        }
    }

    [MenuItem("QuirkyCooks/Netcode/Find Prefabs with NetworkTransform")]
    static void FindPrefabs()
    {
        int count = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!go) continue;
            if (go.GetComponentInChildren<NetworkTransform>(true))
            {
                Debug.Log($"[Audit][Prefab] {path}", go);
                count++;
            }
        }
        Debug.Log($"[Audit] Prefabs with NetworkTransform = {count}");
    }

    static string GetPath(Transform t)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
        while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
        return sb.ToString();
    }
}
#endif
