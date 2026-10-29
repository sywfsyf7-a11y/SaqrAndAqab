using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SaqrAndAqab.Core
{
    // The whole game is built from code at runtime. The scene stays empty.
    public static class GameBootstrap
    {
        const string RootName = "GameRoot";
        const float CameraZ = -10f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (GameObject.Find(RootName) != null) return;

            Application.targetFrameRate = 60;
            UnityEngine.Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ApplyLandscapeOrientation();

            ConfigService.Load();
            Loc.Load();
            SaveService.Load();
            AssetLibrary.WarmCatalog();
            ConfigValidator.Validate();

            var root = new GameObject(RootName);
            UnityEngine.Object.DontDestroyOnLoad(root);

            CreateCamera(root.transform);
            var canvas = CreateCanvas(root.transform);
            CreateEventSystem(root.transform);
            CreateBootLabel(canvas);
        }

        static void ApplyLandscapeOrientation()
        {
            UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
            UnityEngine.Screen.autorotateToPortrait = false;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;
            UnityEngine.Screen.autorotateToLandscapeLeft = true;
            UnityEngine.Screen.autorotateToLandscapeRight = true;
        }

        static Camera CreateCamera(Transform parent)
        {
            var existing = Camera.main;
            if (existing != null) return existing;

            var go = new GameObject("Main Camera");
            go.transform.SetParent(parent, false);
            go.tag = "MainCamera";

            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
            go.transform.position = new Vector3(0f, 0f, CameraZ);

            var game = ConfigService.Balance != null ? ConfigService.Balance.game : null;
            if (game != null) camera.orthographicSize = game.cameraOrthoSize;

            go.AddComponent<AudioListener>();
            return camera;
        }

        static Canvas CreateCanvas(Transform parent)
        {
            var go = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var game = ConfigService.Balance != null ? ConfigService.Balance.game : null;
            if (game != null) scaler.referenceResolution = new Vector2(game.referenceWidth, game.referenceHeight);

            return canvas;
        }

        static void CreateEventSystem(Transform parent)
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
            go.transform.SetParent(parent, false);

#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        // Temporary proof that boot worked. Real Arabic UI starts in task 02.
        static void CreateBootLabel(Canvas canvas)
        {
            var go = new GameObject("Boot Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(canvas.transform, false);

            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = "Boot OK";
            label.fontSize = 64;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(900f, 220f);
        }
    }
}
