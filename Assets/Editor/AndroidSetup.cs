using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using SaqrAndAqab.Core;

// Applies the Android player settings through the PlayerSettings API.
// Menu: Saqr > Apply Android Settings (task 16).
public static class AndroidSetup
{
    const string ApplicationId = "com.saqrandaqab.game";
    const string CompanyName = "SaqrAndAqab";

    // Minimum Android API level (Android 7.0 / Nougat, API 24) required by the game.
    const AndroidSdkVersions MinAndroidApiLevel = AndroidSdkVersions.AndroidApiLevel24;

    [MenuItem("Saqr/Apply Android Settings")]
    public static void ApplyAndroidSettings()
    {
        // Identity: application id + product / company names.
        // The Arabic product name is data-driven (comes from Config/strings_ar.json via Loc).
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
        PlayerSettings.companyName = CompanyName;
        Loc.Load();
        PlayerSettings.productName = Loc.T("app.title");

        // Orientation: landscape left only, auto-rotation limited to landscape.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.useAnimatedAutorotation = true;

        // Scripting backend: IL2CPP (required for Android ARM64).
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        // Architecture: ARM64 only.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Minimum Android API level: 24.
        PlayerSettings.Android.minSdkVersion = MinAndroidApiLevel;

        // Ensure SampleScene is in build settings
        EnsureSampleSceneInBuild();

        Debug.Log(
            "[Android] Settings applied successfully.\n" +
            "- Application ID: " + ApplicationId + "\n" +
            "- Company: " + PlayerSettings.companyName + "\n" +
            "- Product Name: " + PlayerSettings.productName + "\n" +
            "- Orientation: " + PlayerSettings.defaultInterfaceOrientation + "\n" +
            "- Scripting Backend: " + PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) + "\n" +
            "- Architecture: " + PlayerSettings.Android.targetArchitectures + "\n" +
            "- Min SDK: " + PlayerSettings.Android.minSdkVersion);
    }

    private static void EnsureSampleSceneInBuild()
    {
        const string sampleScenePath = "Assets/Scenes/SampleScene.unity";
        var currentScenes = EditorBuildSettings.scenes;

        foreach (var scene in currentScenes)
        {
            if (scene.path == sampleScenePath)
            {
                if (!scene.enabled)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = currentScenes;
                }
                return;
            }
        }

        var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
        currentScenes.CopyTo(newScenes, 0);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(sampleScenePath, true);
        EditorBuildSettings.scenes = newScenes;
        Debug.Log("[Android] Added " + sampleScenePath + " to Build Settings.");
    }
}
