using UnityEngine;

namespace SaqrAndAqab.UI
{
    // Keeps a full-screen rect inside the device safe area. Used by the setup and result screens.
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform _rect;
        Rect _lastSafeArea;
        int _lastWidth;
        int _lastHeight;

        void Awake()
        {
            _rect = transform as RectTransform;
            Apply(true);
        }

        void Update()
        {
            Apply(false);
        }

        void Apply(bool force)
        {
            if (_rect == null) _rect = transform as RectTransform;
            if (_rect == null) return;

            int width = UnityEngine.Screen.width;
            int height = UnityEngine.Screen.height;
            if (width <= 0 || height <= 0) return;

            Rect safe = UnityEngine.Screen.safeArea;
            if (!force && safe == _lastSafeArea && width == _lastWidth && height == _lastHeight) return;

            _lastSafeArea = safe;
            _lastWidth = width;
            _lastHeight = height;

            _rect.anchorMin = new Vector2(safe.xMin / width, safe.yMin / height);
            _rect.anchorMax = new Vector2(safe.xMax / width, safe.yMax / height);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
