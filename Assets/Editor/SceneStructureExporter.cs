using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStructureExporter
{
    [MenuItem("Tools/Export Scene Structure to JSON")]
    public static void ExportSceneStructure()
    {
        var exportData = new List<SceneObjectData>();

        var scene = SceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            ExportRecursively(root.transform, exportData);
        }

        SceneObjectWrapper wrapper = new SceneObjectWrapper { objects = exportData };
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.dataPath, "../SceneStructureExport.json");
        File.WriteAllText(path, json);

        Debug.Log($"✔ Struktura sceny wyeksportowana do {path}");
    }

    private static void ExportRecursively(Transform transform, List<SceneObjectData> exportData)
    {
        var components = transform.GetComponents<Component>()
            .Where(c => c != null) // null gdy np. brakujący MonoBehaviour
            .Select(c => c.GetType().ToString())
            .ToList();

        var entry = new SceneObjectData
        {
            name = transform.name,
            path = GetHierarchyPath(transform),
            components = components.ToArray(),
            hasCustomScript = components.Any(c =>
                c.StartsWith("ScriptsOfTribute") || c.Contains("GameManager") || c.Contains("Manager"))
        };
        exportData.Add(entry)
;        foreach (Transform child in transform)
        {
            ExportRecursively(child, exportData);
        }
    }

    private static string GetHierarchyPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetHierarchyPath(t.parent) + "/" + t.name;
    }

    [System.Serializable]
    private class SceneObjectData
    {
        public string name;
        public string path;
        public string[] components;
        public bool hasCustomScript;
    }

    [System.Serializable]
    private class SceneObjectWrapper
    {
        public List<SceneObjectData> objects;
    }
}
