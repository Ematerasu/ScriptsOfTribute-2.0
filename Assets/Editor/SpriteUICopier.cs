using UnityEditor;
using UnityEngine;
using System.IO;

public class SpriteUICopier
{
    [MenuItem("Tools/Generate UI Sprite Copies")]
    public static void GenerateCompressedUICopies()
    {
        string sourceRoot = "Assets/Resources/Sprites/Cards";
        string targetRoot = "Assets/Resources/Sprites_UI/Cards";

        string[] allPngs = Directory.GetFiles(sourceRoot, "*.png", SearchOption.AllDirectories);

        foreach (string fullPath in allPngs)
        {
            string assetPath = fullPath.Replace("\\", "/");
            string relativePath = assetPath.Substring(sourceRoot.Length + 1); // bez "Assets/Resources/Sprites/"
            string targetPath = Path.Combine(targetRoot, relativePath).Replace("\\", "/");

            // Upewnij się, że katalog docelowy istnieje
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            // Skopiuj plik
            File.Copy(assetPath, targetPath, true);
        }

        AssetDatabase.Refresh();

        // Ustaw import dla każdego sprite'a UI
        string[] newPngs = Directory.GetFiles(targetRoot, "*.png", SearchOption.AllDirectories);
        foreach (string fullPath in newPngs)
        {
            string assetPath = fullPath.Replace("\\", "/");

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.maxTextureSize = 1024;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        Debug.Log($"Utworzono kopie UI dla {allPngs.Length} sprite'ów w {targetRoot}");
    }
}
