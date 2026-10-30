using System;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.UI
{
    // Defeat screen built from code: the reason the wave was lost and the retry / menu buttons.
    public class DefeatScreenView
    {
        static readonly Color BackColor = new Color(0.07f, 0.03f, 0.03f, 0.96f);
        static readonly Color TextColor = new Color(1f, 0.90f, 0.85f, 1f);
        static readonly Color RedColor = new Color(0.95f, 0.35f, 0.30f, 1f);

        RectTransform _root;

        public RectTransform Root { get { return _root; } }

        public static DefeatScreenView Build(Transform canvas, int wave, string reason, Action onRetry, Action onMenu)
        {
            if (canvas == null) return null;

            var view = new DefeatScreenView();
            view.BuildInternal(canvas, wave, reason, onRetry, onMenu);
            return view;
        }

        void BuildInternal(Transform canvas, int wave, string reason, Action onRetry, Action onMenu)
        {
            _root = UIFactory.CreateUIRoot(canvas, "Defeat UI");

            var back = _root.gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            var safe = UIFactory.CreateUIRoot(_root, "Safe Area");
            safe.gameObject.AddComponent<SafeAreaFitter>();

            string titleKey = reason == "hero" ? "defeat.hero" : "defeat.gate";
            var title = UIFactory.CreateText(safe, titleKey, 72, RedColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 220f), new Vector2(1400f, 150f));

            var waveLabel = UIFactory.CreateTextRaw(safe, Loc.T("hud.wave", Mathf.Max(1, wave)), 36, TextColor,
                TextAlignmentOptions.Center, true);
            UIFactory.Place(waveLabel.rectTransform, new Vector2(0f, 100f), new Vector2(600f, 60f));

            var retry = UIFactory.CreateButton(safe, "defeat.retry", delegate { if (onRetry != null) onRetry(); },
                new Vector2(460f, 110f), 40);
            UIFactory.Place(retry.GetComponent<RectTransform>(), new Vector2(0f, -40f), new Vector2(460f, 110f));

            var menu = UIFactory.CreateButton(safe, "common.menu", delegate { if (onMenu != null) onMenu(); },
                new Vector2(460f, 110f), 40);
            UIFactory.Place(menu.GetComponent<RectTransform>(), new Vector2(0f, -190f), new Vector2(460f, 110f));
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
        }
    }
}
