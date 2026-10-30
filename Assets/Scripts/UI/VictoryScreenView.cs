using System;
using System.Collections.Generic;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.UI
{
    // Victory screen built from code: wave title, award / collected / total rows,
    // the charm that was found (if any), the newly unlocked items and the next button.
    public class VictoryScreenView
    {
        static readonly Color BackColor = new Color(0.03f, 0.07f, 0.06f, 0.96f);
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color DimTextColor = new Color(0.72f, 0.70f, 0.66f, 1f);
        static readonly Color GoldColor = new Color(0.95f, 0.78f, 0.30f, 1f);

        RectTransform _root;

        public RectTransform Root { get { return _root; } }

        public static VictoryScreenView Build(Transform canvas, int wave, int award, int collected, int total,
            CharmConfig charm, List<UnlockConfig> unlocked, Action onNext)
        {
            if (canvas == null) return null;

            var view = new VictoryScreenView();
            view.BuildInternal(canvas, wave, award, collected, total, charm, unlocked, onNext);
            return view;
        }

        void BuildInternal(Transform canvas, int wave, int award, int collected, int total,
            CharmConfig charm, List<UnlockConfig> unlocked, Action onNext)
        {
            _root = UIFactory.CreateUIRoot(canvas, "Victory UI");

            var back = _root.gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            var safe = UIFactory.CreateUIRoot(_root, "Safe Area");
            safe.gameObject.AddComponent<SafeAreaFitter>();

            var title = UIFactory.CreateTextRaw(safe, Loc.T("victory.title", wave), 72, GoldColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 400f), new Vector2(1200f, 130f));

            AddRow(safe, "victory.award", award, 250f, false);
            AddRow(safe, "victory.collected", collected, 165f, false);
            AddRow(safe, "victory.total", total, 80f, true);

            if (charm != null)
            {
                var charmText = UIFactory.CreateTextRaw(safe, Loc.T("victory.charm_found") + " " + Loc.T("charm." + charm.id),
                    34, TextColor, TextAlignmentOptions.Center, true);
                UIFactory.Place(charmText.rectTransform, new Vector2(60f, -30f), new Vector2(900f, 60f));

                var iconRect = CreateRect(safe, "Charm Icon", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-320f, -30f), new Vector2(90f, 90f));
                var icon = iconRect.gameObject.AddComponent<Image>();
                icon.sprite = AssetLibrary.GetSprite("ui_" + charm.id);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }

            if (unlocked != null && unlocked.Count > 0)
            {
                var label = UIFactory.CreateText(safe, "victory.unlocked", 34, TextColor, TextAlignmentOptions.Center, true);
                UIFactory.Place(label.rectTransform, new Vector2(0f, -160f), new Vector2(900f, 60f));

                for (int i = 0; i < unlocked.Count; i++)
                {
                    var entry = unlocked[i];
                    if (entry == null) continue;

                    string nameKey = entry.type + "." + entry.id;
                    var row = UIFactory.CreateTextRaw(safe, Loc.T(nameKey), 30, DimTextColor, TextAlignmentOptions.Center, true);
                    UIFactory.Place(row.rectTransform, new Vector2(0f, -230f - i * 70f), new Vector2(900f, 50f));
                }
            }

            var next = UIFactory.CreateButton(safe, "common.next", delegate { if (onNext != null) onNext(); },
                new Vector2(340f, 100f), 40);
            UIFactory.Place(next.GetComponent<RectTransform>(), new Vector2(0f, -420f), new Vector2(340f, 100f));
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
        }

        void AddRow(Transform parent, string labelKey, int value, float y, bool highlight)
        {
            var row = CreateRect(parent, "Row " + labelKey, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, y), new Vector2(1000f, 70f));

            var label = UIFactory.CreateText(row, labelKey, 34, highlight ? GoldColor : TextColor,
                TextAlignmentOptions.Right, true);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(1f, 0.5f);
            labelRect.anchoredPosition = new Vector2(30f, 0f);
            labelRect.sizeDelta = new Vector2(600f, 60f);

            var valueText = UIFactory.CreateTextRaw(row, value.ToString(), 38, highlight ? GoldColor : TextColor,
                TextAlignmentOptions.Left, true);
            var valueRect = valueText.rectTransform;
            valueRect.anchorMin = new Vector2(0.5f, 0.5f);
            valueRect.anchorMax = new Vector2(0.5f, 0.5f);
            valueRect.pivot = new Vector2(0f, 0.5f);
            valueRect.anchoredPosition = new Vector2(-30f, 0f);
            valueRect.sizeDelta = new Vector2(300f, 60f);
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
