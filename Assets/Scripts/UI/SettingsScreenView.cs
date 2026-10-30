using System;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.UI
{
    // Settings screen: music/sfx sliders (saved) and a reset-progress action with confirmation.
    public class SettingsScreenView
    {
        static readonly Color BackColor = new Color(0.04f, 0.05f, 0.07f, 0.97f);
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color GoldColor = new Color(0.95f, 0.78f, 0.30f, 1f);

        RectTransform _root;
        Slider _musicSlider;
        Slider _sfxSlider;
        GameObject _confirmRoot;

        public RectTransform Root { get { return _root; } }

        public static SettingsScreenView Build(Transform canvas, Action onBack)
        {
            if (canvas == null) return null;

            var view = new SettingsScreenView();
            view.BuildInternal(canvas, onBack);
            return view;
        }

        void BuildInternal(Transform canvas, Action onBack)
        {
            if (SaveService.Data == null) SaveService.Load();

            _root = UIFactory.CreateUIRoot(canvas, "Settings UI");

            var back = _root.gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            var safe = UIFactory.CreateUIRoot(_root, "Safe Area");
            safe.gameObject.AddComponent<SafeAreaFitter>();

            var title = UIFactory.CreateText(safe, "settings.title", 64, GoldColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 400f), new Vector2(900f, 110f));

            // Back button.
            var backButton = UIFactory.CreateButton(safe, "common.back", delegate { if (onBack != null) onBack(); },
                new Vector2(220f, 84f), 34);
            UIFactory.Place(backButton.GetComponent<RectTransform>(), new Vector2(-760f, 400f), new Vector2(220f, 84f));

            // Music slider.
            BuildSlider(safe, "settings.music", 180f, SaveService.Data.musicVolume, delegate (float v)
            {
                if (SaveService.Data == null) return;
                SaveService.Data.musicVolume = v;
                SaveService.Save();
                AudioService.ApplyVolumes();
            }, out _musicSlider);

            // SFX slider.
            BuildSlider(safe, "settings.sfx", 0f, SaveService.Data.sfxVolume, delegate (float v)
            {
                if (SaveService.Data == null) return;
                SaveService.Data.sfxVolume = v;
                SaveService.Save();
                AudioService.ApplyVolumes();
            }, out _sfxSlider);

            // Reset button.
            var reset = UIFactory.CreateButton(safe, "settings.reset", OnResetPressed, new Vector2(460f, 100f), 34);
            UIFactory.Place(reset.GetComponent<RectTransform>(), new Vector2(0f, -260f), new Vector2(460f, 100f));

            BuildConfirmOverlay(safe);
        }

        void BuildSlider(Transform parent, string labelKey, float y, float value, Action<float> onChanged, out Slider slider)
        {
            var label = UIFactory.CreateText(parent, labelKey, 34, TextColor, TextAlignmentOptions.Right, true);
            UIFactory.Place(label.rectTransform, new Vector2(0f, y), new Vector2(400f, 50f));
            // shift label to the right side
            label.rectTransform.anchoredPosition = new Vector2(260f, y);

            var sliderRect = CreateRect(parent, labelKey + " Slider", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-120f, y), new Vector2(520f, 40f));

            var bg = sliderRect.gameObject.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.17f, 0.20f, 1f);

            slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = Mathf.Clamp01(value);
            slider.direction = Slider.Direction.LeftToRight;

            // Fill area.
            var fillArea = CreateRect(sliderRect, "Fill Area", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(500f, 20f));
            fillArea.anchorMin = new Vector2(0f, 0.25f);
            fillArea.anchorMax = new Vector2(1f, 0.75f);
            fillArea.offsetMin = new Vector2(10f, 0f);
            fillArea.offsetMax = new Vector2(-10f, 0f);

            var fill = CreateRect(fillArea, "Fill", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, Vector2.zero);
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = GoldColor;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            slider.fillRect = fill;

            // Handle.
            var handleArea = CreateRect(sliderRect, "Handle Slide Area", new Vector2(0f, 0f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(10f, 0f);
            handleArea.offsetMax = new Vector2(-10f, 0f);

            var handle = CreateRect(handleArea, "Handle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(30f, 40f));
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = TextColor;
            slider.handleRect = handle;

            if (onChanged != null) slider.onValueChanged.AddListener(delegate (float v) { onChanged(v); });
        }

        void OnResetPressed()
        {
            if (_confirmRoot != null) _confirmRoot.SetActive(true);
        }

        void BuildConfirmOverlay(Transform parent)
        {
            _confirmRoot = CreateRect(parent, "Confirm", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero).gameObject;
            var rect = _confirmRoot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = _confirmRoot.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.03f, 0.04f, 0.85f);
            bg.raycastTarget = true;

            var text = UIFactory.CreateText(_confirmRoot.transform, "settings.reset_confirm", 34, TextColor,
                TextAlignmentOptions.Center, true);
            UIFactory.Place(text.rectTransform, new Vector2(0f, 80f), new Vector2(900f, 120f));

            var confirm = UIFactory.CreateButton(_confirmRoot.transform, "common.confirm", delegate
            {
                SaveService.Reset();
                AudioService.ApplyVolumes();
                if (_confirmRoot != null) _confirmRoot.SetActive(false);
            }, new Vector2(300f, 90f), 34);
            UIFactory.Place(confirm.GetComponent<RectTransform>(), new Vector2(-180f, -120f), new Vector2(300f, 90f));

            var cancel = UIFactory.CreateButton(_confirmRoot.transform, "common.cancel", delegate
            {
                if (_confirmRoot != null) _confirmRoot.SetActive(false);
            }, new Vector2(300f, 90f), 34);
            UIFactory.Place(cancel.GetComponent<RectTransform>(), new Vector2(180f, -120f), new Vector2(300f, 90f));

            _confirmRoot.SetActive(false);
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
