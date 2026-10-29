using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using SaqrAndAqab.Data;

// Import settings for real art dropped into Assets/Resources/Art.
// Pivot comes from Assets/Resources/Config/assets.json.
public class ArtImportPostprocessor : AssetPostprocessor
{
    const string ArtFolder = "Assets/Resources/Art/";
    const string CatalogPath = "Assets/Resources/Config/assets.json";
    const float PixelsPerUnit = 100f;

    static AssetsConfig _catalog;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtFolder, StringComparison.OrdinalIgnoreCase)) return;

        var importer = assetImporter as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;

        string id = Path.GetFileNameWithoutExtension(assetPath);
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = PivotFor(id);
        importer.SetTextureSettings(settings);
    }

    static Vector2 PivotFor(string id)
    {
        var entry = FindEntry(id);
        string pivot = entry != null ? entry.pivot : null;

        if (pivot == "bottom") return new Vector2(0.5f, 0f);
        if (pivot == "top") return new Vector2(0.5f, 1f);

        string category = entry != null ? entry.category : null;
        if (category == "character" || category == "enemy" || category == "ally" ||
            category == "boss" || category == "prop")
            return new Vector2(0.5f, 0f);

        return new Vector2(0.5f, 0.5f);
    }

    static AssetEntry FindEntry(string id)
    {
        var catalog = LoadCatalog();
        if (catalog == null || catalog.assets == null) return null;

        for (int i = 0; i < catalog.assets.Count; i++)
        {
            var entry = catalog.assets[i];
            if (entry != null && entry.id == id) return entry;
        }

        return null;
    }

    static AssetsConfig LoadCatalog()
    {
        try
        {
            if (!File.Exists(CatalogPath)) return null;
            return JsonUtility.FromJson<AssetsConfig>(File.ReadAllText(CatalogPath));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ArtImport] Could not read assets.json: " + e.Message);
            return null;
        }
    }
}
