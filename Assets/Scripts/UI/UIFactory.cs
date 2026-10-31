using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using RTLTMPro;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.UI
{
    // Creates RTL Arabic UI from code. Text always goes through RTLTextMeshPro.
    public static class UIFactory
    {
        const string RegularFontResource = "Fonts/IBMPlexSansArabic-Regular SDF";
        const string BoldFontResource = "Fonts/IBMPlexSansArabic-Bold SDF";

        static readonly Color ButtonColor = new Color(0.85f, 0.66f, 0.28f, 1f);
        static readonly Color ButtonTextColor = new Color(0.98f, 0.94f, 0.85f, 1f);

        static TMP_FontAsset _regularFont;
        static TMP_FontAsset _boldFont;
        static bool _missingFontLogged;

        public static TMP_FontAsset RegularFont
        {
            get
            {
                if (_regularFont == null) _regularFont = LoadFont(RegularFontResource);
                return _regularFont;
            }
        }

        public static TMP_FontAsset BoldFont
        {
            get
            {
                if (_boldFont == null) _boldFont = LoadFont(BoldFontResource);
                return _boldFont;
            }
        }

        public static RTLTextMeshPro CreateText(Transform parent, string locKey, int size, Color color,
            TextAlignmentOptions align, bool bold = false)
        {
            return CreateTextRaw(parent, Loc.T(locKey), size, color, align, bold);
        }

        // For numbers and dynamic strings. Still RTL so digits and mixed text stay correct.
        public static RTLTextMeshPro CreateTextRaw(Transform parent, string text, int size, Color color,
            TextAlignmentOptions align, bool bold = false)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(RTLTextMeshPro));
            go.transform.SetParent(parent, false);

            var label = go.GetComponent<RTLTextMeshPro>();
            label.font = bold ? BoldFont : RegularFont;
            label.Farsi = false;
            label.PreserveNumbers = true;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.raycastTarget = false;
            return label;
        }

        // Stretches a rect to fill its parent (used for text inside a button).
        public static void Fill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // A full-canvas container for one screen's widgets.
        public static RectTransform CreateUIRoot(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Fill(rect);
            return rect;
        }

        // Centers a rect on its parent with an explicit size.
        public static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Button CreateButton(Transform parent, string locKey, UnityAction onClick)
        {
            return CreateButton(parent, locKey, onClick, new Vector2(460f, 120f), 46);
        }

        public static Button CreateButton(Transform parent, string locKey, UnityAction onClick,
            Vector2 size, int labelSize)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            var button = go.GetComponent<Button>();
            UIKit.ApplyButton(button, image);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            var label = CreateText(go.transform, locKey, labelSize, ButtonTextColor, TextAlignmentOptions.Center, true);
            var labelRect = label.rectTransform;
            Fill(labelRect);

            if (onClick != null) button.onClick.AddListener(onClick);
            button.onClick.AddListener(delegate { AudioService.Play("ui_click"); });
            return button;
        }

        public static Image CreatePanel(Transform parent, Vector2 size)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            UIKit.ApplyPanel(image);
            return image;
        }

        static TMP_FontAsset LoadFont(string resourcePath)
        {
            var font = Resources.Load<TMP_FontAsset>(resourcePath);
            if (font == null && !_missingFontLogged)
            {
                _missingFontLogged = true;
                Debug.LogError("[UI] Missing font asset Resources/" + resourcePath +
                               ".asset - run the menu Saqr > Create Arabic Font Assets once.");
            }
            return font;
        }
    }
}
