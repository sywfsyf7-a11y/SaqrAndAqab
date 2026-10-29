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
        static readonly Color ButtonTextColor = new Color(0.12f, 0.09f, 0.05f, 1f);

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

        public static Button CreateButton(Transform parent, string locKey, UnityAction onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = ButtonColor;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(460f, 120f);

            var label = CreateText(go.transform, locKey, 46, ButtonTextColor, TextAlignmentOptions.Center, true);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var button = go.GetComponent<Button>();
            if (onClick != null) button.onClick.AddListener(onClick);
            return button;
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
