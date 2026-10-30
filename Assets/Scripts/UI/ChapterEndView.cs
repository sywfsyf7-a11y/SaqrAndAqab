using System;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.UI
{
    // Chapter-end screen: title, then story.end.1..3 one by one (tap to continue).
    public class ChapterEndView
    {
        static readonly Color BackColor = new Color(0.02f, 0.03f, 0.06f, 0.98f);
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color GoldColor = new Color(0.95f, 0.78f, 0.30f, 1f);

        RectTransform _root;
        RTLTextMeshPro _line;
        int _index;
        Action _onFinished;

        public RectTransform Root { get { return _root; } }

        public static ChapterEndView Build(Transform canvas, Action onFinished)
        {
            if (canvas == null) return null;

            var view = new ChapterEndView();
            view._onFinished = onFinished;
            view.BuildInternal(canvas);
            return view;
        }

        void BuildInternal(Transform canvas)
        {
            _root = UIFactory.CreateUIRoot(canvas, "Chapter End UI");

            var back = _root.gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            var safe = UIFactory.CreateUIRoot(_root, "Safe Area");
            safe.gameObject.AddComponent<SafeAreaFitter>();

            var title = UIFactory.CreateText(safe, "story.end.title", 72, GoldColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 320f), new Vector2(1200f, 130f));

            _line = UIFactory.CreateTextRaw(safe, Loc.T("story.end.1"), 40, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(_line.rectTransform, new Vector2(0f, 40f), new Vector2(1400f, 260f));

            var hint = UIFactory.CreateText(safe, "common.next", 28, new Color(0.72f, 0.70f, 0.66f, 1f),
                TextAlignmentOptions.Center, true);
            UIFactory.Place(hint.rectTransform, new Vector2(0f, -260f), new Vector2(600f, 50f));

            // Full-screen tap target.
            var tap = CreateRect(safe, "Tap", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            tap.anchorMin = Vector2.zero;
            tap.anchorMax = Vector2.one;
            tap.offsetMin = Vector2.zero;
            tap.offsetMax = Vector2.zero;
            var tapImage = tap.gameObject.AddComponent<Image>();
            tapImage.color = new Color(0f, 0f, 0f, 0f);
            tapImage.raycastTarget = true;
            var button = tap.gameObject.AddComponent<Button>();
            button.targetGraphic = tapImage;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Advance);
        }

        void Advance()
        {
            _index++;
            if (_index >= 3)
            {
                if (_onFinished != null) _onFinished();
                return;
            }

            if (_line != null) _line.text = Loc.T("story.end." + (_index + 1));
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
