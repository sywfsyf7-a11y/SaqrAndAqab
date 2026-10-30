using System;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Battle;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.UI
{
    // One-shot tutorial hints for waves 1-2. Saved so they never repeat.
    public class TutorialHints : MonoBehaviour
    {
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);

        BattleWorld _world;
        WaveManager _waves;
        HaybahMeter _haybah;
        Ability _slash;

        GameObject _panel;
        RTLTextMeshPro _label;
        float _dismissTimer;
        bool _showing;

        string _pendingKey;
        Vector2 _pendingPosition;

        readonly System.Collections.Generic.Queue<string> _queue = new System.Collections.Generic.Queue<string>();

        public static TutorialHints Create(Transform canvasParent, BattleWorld world)
        {
            if (canvasParent == null || world == null) return null;

            var go = new GameObject("Tutorial Hints", typeof(RectTransform), typeof(TutorialHints));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvasParent, false);
            UIFactory.Fill(rect);

            var hints = go.GetComponent<TutorialHints>();
            hints.Init(world);
            return hints;
        }

        void Init(BattleWorld world)
        {
            _world = world;
            _waves = world.Waves;
            _haybah = world.Haybah;

            if (_waves != null)
            {
                _waves.OnWaveProgress += delegate { };
            }

            // Find slash ability.
            if (_world.Abilities != null)
            {
                for (int i = 0; i < _world.Abilities.Count; i++)
                {
                    if (_world.Abilities[i] != null && _world.Abilities[i].Id == "crescent_slash")
                    {
                        _slash = _world.Abilities[i];
                        break;
                    }
                }
            }

            if (_haybah != null) _haybah.OnChanged += OnHaybahChanged;

            BuildPanel();

            // Wave 1/2 hints at start.
            int wave = _waves != null ? _waves.CurrentWave : 1;
            if (wave <= 2)
            {
                EnqueueHint("tutorial.protect", new Vector2(-4f, 0f)); // arrow to gate approximated
                EnqueueHint("tutorial.move", Vector2.zero);
            }
        }

        void BuildPanel()
        {
            _panel = new GameObject("Hint Panel", typeof(RectTransform), typeof(Image));
            var rect = _panel.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            UIFactory.Place(rect, new Vector2(0f, -260f), new Vector2(700f, 160f));

            var image = _panel.GetComponent<Image>();
            image.sprite = AssetLibrary.GetSprite("ui_panel_scroll");
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;

            _label = UIFactory.CreateTextRaw(rect, "", 30, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Fill(_label.rectTransform);

            var button = _panel.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Dismiss);

            _panel.SetActive(false);
        }

        void OnHaybahChanged(int value)
        {
            if (SaveService.Data == null) return;
            if (SaveService.Data.tutorialHaybahShown) return;

            // First time haybah reaches a summon cost (farmer costs 5).
            if (value >= 5)
            {
                SaveService.Data.tutorialHaybahShown = true;
                SaveService.Save();
                EnqueueHint("tutorial.haybah", Vector2.zero);
            }
        }

        void Update()
        {
            // Slash ready hint.
            if (_slash != null && !_showing && SaveService.Data != null && !SaveService.Data.tutorialSlashShown)
            {
                if (_slash.IsReady)
                {
                    SaveService.Data.tutorialSlashShown = true;
                    SaveService.Save();
                    EnqueueHint("tutorial.slash", Vector2.zero);
                }
            }

            if (_showing)
            {
                _dismissTimer -= Time.deltaTime;
                if (_dismissTimer <= 0f) Dismiss();
            }
            else if (_queue.Count > 0)
            {
                ShowNext();
            }
        }

        void EnqueueHint(string key, Vector2 position)
        {
            // Respect saved flags for protect/move.
            if (key == "tutorial.protect" && SaveService.Data != null && SaveService.Data.tutorialProtectShown) return;
            if (key == "tutorial.move" && SaveService.Data != null && SaveService.Data.tutorialMoveShown) return;

            _queue.Enqueue(key);

            if (key == "tutorial.protect" && SaveService.Data != null) { SaveService.Data.tutorialProtectShown = true; SaveService.Save(); }
            if (key == "tutorial.move" && SaveService.Data != null) { SaveService.Data.tutorialMoveShown = true; SaveService.Save(); }
        }

        void ShowNext()
        {
            if (_queue.Count == 0) return;
            string key = _queue.Dequeue();

            if (_label != null) _label.text = Loc.T(key);
            if (_panel != null) _panel.SetActive(true);
            _showing = true;
            _dismissTimer = 5f;
        }

        void Dismiss()
        {
            if (_panel != null) _panel.SetActive(false);
            _showing = false;
            _dismissTimer = 0f;
        }

        void OnDestroy()
        {
            if (_haybah != null) _haybah.OnChanged -= OnHaybahChanged;
        }
    }
}
