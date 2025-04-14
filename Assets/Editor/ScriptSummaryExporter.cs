using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ScriptSummaryExporter
{
    [MenuItem("Tools/Export Script Summary to JSON")]
    public static void ExportScriptSummary()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        if (!Directory.Exists(scriptsPath))
        {
            Debug.LogError("Brak folderu Assets/Scripts!");
            return;
        }

        var summary = new List<ScriptInfo>();
        ScriptInfo current = null;
        string currentNamespace = "";

        foreach (string file in Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories))
        {
            string[] lines = File.ReadAllLines(file);
            current = null;
            currentNamespace = "";
            List<string> pendingAttributes = new(); // atrybuty nad polem/metodą

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.StartsWith("namespace "))
                {
                    currentNamespace = line.Substring("namespace ".Length).Trim();
                }

                // Zbieraj atrybuty
                if (line.StartsWith("["))
                {
                    pendingAttributes.Add(line);
                    continue;
                }

                // Nowa klasa / struct / interface / enum
                Match classMatch = Regex.Match(line, @"(public|internal|private|protected)?\s*(class|struct|interface|enum)\s+(\w+)");
                if (classMatch.Success)
                {
                    current = new ScriptInfo
                    {
                        fileName = Path.GetFileName(file),
                        path = file.Replace(Application.dataPath, "Assets"),
                        namespaceName = currentNamespace,
                        kind = classMatch.Groups[2].Value,
                        typeName = classMatch.Groups[3].Value,
                        methods = new List<string>(),
                        fields = new List<string>()
                    };
                    summary.Add(current);
                    pendingAttributes.Clear();
                    continue;
                }

                if (current == null) continue;

                // Metody (public, internal, private, protected)
                Match methodMatch = Regex.Match(line, @"(public|internal|private|protected)\s+[\w<>\[\],]+\s+(\w+)\s*\(");
                if (methodMatch.Success)
                {
                    string attrPrefix = pendingAttributes.Count > 0 ? string.Join(" ", pendingAttributes) + " " : "";
                    current.methods.Add(attrPrefix + line.Replace("{", "").Trim());
                    pendingAttributes.Clear();
                    continue;
                }

                // Pola
                Match fieldMatch = Regex.Match(line, @"(public|private|protected|internal)\s+[\w<>\[\],]+\s+\w+\s*(=|;)");
                if (fieldMatch.Success)
                {
                    string attrPrefix = pendingAttributes.Count > 0 ? string.Join(" ", pendingAttributes) + " " : "";
                    current.fields.Add(attrPrefix + line.Trim());
                    pendingAttributes.Clear();
                    continue;
                }

                // Jeśli nie metoda i nie pole — atrybut zostaje
            }
        }

        string json = JsonUtility.ToJson(new Wrapper { scripts = summary }, true);
        string outputPath = Path.Combine(Application.dataPath, "../ScriptSummary.json");
        File.WriteAllText(outputPath, json);

        Debug.Log($"✔ Wyeksportowano {summary.Count} typów do {outputPath}");
    }

    [System.Serializable]
    private class ScriptInfo
    {
        public string fileName;
        public string path;
        public string namespaceName;
        public string kind;  // class, struct, interface, enum
        public string typeName;
        public List<string> methods;
        public List<string> fields;
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<ScriptInfo> scripts;
    }
}
