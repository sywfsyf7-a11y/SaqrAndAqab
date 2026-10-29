using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        static Transform _root;

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
            _root = root.transform;

            var canvas = CreateCanvas(root.transform);
            CreateEventSystem(root.transform);
            CreateBootLabel(canvas);
            EnsureCamera(root.transform);

            Debug.Log("Boot OK");
        }

        static void ApplyLandscapeOrientation()
        {
            UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
            UnityEngine.Screen.autorotateToPortrait = false;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;
            UnityEngine.Screen.autorotateToLandscapeLeft = true;
            UnityEngine.Screen.autorotateToLandscapeRight = true;
        }

        // Uses the scene camera when there is one. Never creates a second camera or a second AudioListener.
        static void EnsureCamera(Transform parent)
        {
            var existing = Camera.main;
            if (existing != null)
            {
                ApplyCameraSettings(existing);
                EnsureSingleAudioListener(existing);
                return;
            }

            // We boot before the scene loads, so the scene camera does not exist yet.
            // Check again right after the scene is loaded instead of creating a second camera.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var camera = Camera.main;
            if (camera == null) camera = CreateCamera(_root);
            if (camera == null) return;

            ApplyCameraSettings(camera);
            EnsureSingleAudioListener(camera);
        }

        static Camera CreateCamera(Transform parent)
        {
            var go = new GameObject("Main Camera");
            if (parent != null) go.transform.SetParent(parent, false);
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, CameraZ);

            return go.AddComponent<Camera>();
        }

        static void ApplyCameraSettings(Camera camera)
        {
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);

            var game = ConfigService.Balance != null ? ConfigService.Balance.game : null;
            if (game != null) camera.orthographicSize = game.cameraOrthoSize;
        }

        static void EnsureSingleAudioListener(Camera camera)
        {
            var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length == 0)
            {
                camera.gameObject.AddComponent<AudioListener>();
                return;
            }

            var keep = camera.GetComponent<AudioListener>();
            if (keep == null) keep = listeners[0];

            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] != keep) UnityEngine.Object.Destroy(listeners[i]);
            }
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
