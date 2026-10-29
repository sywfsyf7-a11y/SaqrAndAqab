using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SaqrAndAqab.UI;
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
        static readonly Color TitleColor = new Color(0.95f, 0.85f, 0.60f, 1f);
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
            CreateMenuUI(canvas);
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

        // Temporary menu screen: Arabic title + play button. The real menu comes in task 10.
        static void CreateMenuUI(Canvas canvas)
        {
            var title = UIFactory.CreateText(canvas.transform, "app.title", 140, TitleColor,
                TextAlignmentOptions.Center, true);
            Place(title.rectTransform, new Vector2(0f, 230f), new Vector2(1500f, 320f));

            var button = UIFactory.CreateButton(canvas.transform, "menu.play", OnPlayPressed);
            Place(button.GetComponent<RectTransform>(), new Vector2(0f, -140f), new Vector2(460f, 120f));
        }

        static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static void OnPlayPressed()
        {
            Debug.Log("Play button pressed");
        }
    }
}
