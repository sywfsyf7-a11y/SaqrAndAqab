using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SaqrAndAqab.Battle
{
    public enum WorldThemeId
    {
        Day = 0,
        Night = 1,
        DarkRealm = 2
    }

    // Look of each world theme: camera background colour + one global 2D light.
    // These are art colours, not gameplay numbers, so they live here and not in balance.json.
    public static class WorldTheme
    {
        public static Color CameraColor(WorldThemeId theme)
        {
            switch (theme)
            {
                case WorldThemeId.Night: return new Color(0.05f, 0.07f, 0.14f, 1f);
                case WorldThemeId.DarkRealm: return new Color(0.09f, 0.05f, 0.13f, 1f);
                default: return new Color(0.85f, 0.70f, 0.48f, 1f);
            }
        }

        public static Color LightColor(WorldThemeId theme)
        {
            switch (theme)
            {
                case WorldThemeId.Night: return new Color(0.55f, 0.66f, 0.95f, 1f);
                case WorldThemeId.DarkRealm: return new Color(0.52f, 0.38f, 0.92f, 1f);
                default: return new Color(1f, 0.95f, 0.86f, 1f);
            }
        }

        public static float LightIntensity(WorldThemeId theme)
        {
            switch (theme)
            {
                case WorldThemeId.Night: return 0.70f;
                case WorldThemeId.DarkRealm: return 0.55f;
                default: return 1.10f;
            }
        }

        public static string SkySprite(WorldThemeId theme) { return "bg_" + Key(theme) + "_sky"; }

        public static string FarSprite(WorldThemeId theme) { return "bg_" + Key(theme) + "_far"; }

        public static string GroundSprite(WorldThemeId theme) { return "ground_" + Key(theme); }

        static string Key(WorldThemeId theme)
        {
            switch (theme)
            {
                case WorldThemeId.Night: return "night";
                case WorldThemeId.DarkRealm: return "dark";
                default: return "day";
            }
        }

        // Applies the theme to the camera and adds one Global Light 2D under the world root.
        // If Light2D is not available in this render pipeline we keep the camera colour only.
        public static void Apply(WorldThemeId theme, Camera camera, Transform parent)
        {
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = CameraColor(theme);
            }

            CreateGlobalLight(theme, parent);
        }

        static void CreateGlobalLight(WorldThemeId theme, Transform parent)
        {
            try
            {
                var go = new GameObject("Global Light 2D", typeof(Light2D));
                if (parent != null) go.transform.SetParent(parent, false);

                var light = go.GetComponent<Light2D>();
                if (light == null)
                {
                    Object.Destroy(go);
                    return;
                }

                light.lightType = Light2D.LightType.Global;
                light.color = LightColor(theme);
                light.intensity = LightIntensity(theme);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Battle] Global Light 2D is not available, using the camera colour only: " + e.Message);
            }
        }
    }
}
