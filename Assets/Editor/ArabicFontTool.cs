using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Builds the Arabic SDF font assets used by RTLTextMeshPro.
// Menu: Saqr > Create Arabic Font Assets
public static class ArabicFontTool
{
    const string FontsFolder = "Assets/Fonts";
    const string OutputFolder = "Assets/Resources/Fonts";

    const int SamplingPointSize = 90;
    const int AtlasPadding = 9;
    const int AtlasWidth = 1024;
    const int AtlasHeight = 1024;

    [MenuItem("Saqr/Create Arabic Font Assets")]
    public static void CreateArabicFontAssets()
    {
        EnsureFolder(OutputFolder);

        bool ok = CreateFontAsset("Tajawal-Regular", "Tajawal-Regular SDF");
        ok &= CreateFontAsset("Tajawal-Bold", "Tajawal-Bold SDF");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (ok) Debug.Log("[Fonts] Arabic font assets are ready in " + OutputFolder);
    }

    static bool CreateFontAsset(string fontFileName, string assetName)
    {
        string fontPath = FontsFolder + "/" + fontFileName + ".ttf";
        var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null)
        {
            Debug.LogError("[Fonts] Missing font file: " + fontPath);
            return false;
        }

        TMP_FontAsset fontAsset = null;
        try
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                SamplingPointSize,
                AtlasPadding,
                GlyphRenderMode.SDFAA,
                AtlasWidth,
                AtlasHeight,
                AtlasPopulationMode.Dynamic,
                true);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Fonts] TMP_FontAsset.CreateFontAsset failed: " + e.Message);
        }

        if (fontAsset == null)
        {
            LogManualSteps();
            return false;
        }

        string assetPath = OutputFolder + "/" + assetName + ".asset";
        fontAsset.name = assetName;
        AssetDatabase.CreateAsset(fontAsset, assetPath);

        // The atlas texture and the material must live inside the font asset file.
        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
        {
            var atlas = fontAsset.atlasTextures[0];
            atlas.name = assetName + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        }

        if (fontAsset.material != null)
        {
            fontAsset.material.name = assetName + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        return true;
    }

    static void LogManualSteps()
    {
        Debug.LogError(
            "[Fonts] Could not create the font assets from code in this Unity version.\n" +
            "Do it manually: Window > TextMeshPro > Font Asset Creator\n" +
            "1) Font Source = Assets/Fonts/Tajawal-Regular.ttf (then repeat for Tajawal-Bold.ttf)\n" +
            "2) Atlas Population Mode = Dynamic, Atlas Resolution = 1024 x 1024\n" +
            "3) Generate Font Atlas, then Save as Assets/Resources/Fonts/Tajawal-Regular SDF and Tajawal-Bold SDF");
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        string parent = Path.GetDirectoryName(folder);
        if (string.IsNullOrEmpty(parent)) return;
        parent = parent.Replace('\\', '/');

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }
}
