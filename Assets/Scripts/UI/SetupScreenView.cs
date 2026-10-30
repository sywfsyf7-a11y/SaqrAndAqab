using System;
using System.Collections.Generic;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Battle;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.UI
{
    // Setup screen built from code: wave title, three tabs (allies / abilities / charms),
    // unlocked item cards, the chosen slots and the start button.
    public class SetupScreenView
    {
        const float CardWidth = 300f;
        const float CardHeight = 340f;
        const float CardGap = 40f;
        const float SlotSize = 110f;
        const float SlotGap = 24f;

        static readonly Color BackColor = new Color(0.04f, 0.05f, 0.07f, 0.97f);
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color DimTextColor = new Color(0.72f, 0.70f, 0.66f, 1f);
        static readonly Color CardColor = new Color(0.10f, 0.11f, 0.14f, 0.96f);
        static readonly Color CardSelectedColor = new Color(0.36f, 0.28f, 0.12f, 1f);
        static readonly Color SlotColor = new Color(0.08f, 0.09f, 0.11f, 1f);
        static readonly Color SlotSelectedColor = new Color(0.20f, 0.22f, 0.26f, 1f);
        static readonly Color TabColor = new Color(0.16f, 0.17f, 0.20f, 1f);
        static readonly Color TabActiveColor = new Color(0.85f, 0.66f, 0.28f, 1f);
        static readonly Color GoldColor = new Color(0.95f, 0.78f, 0.30f, 1f);

        class Card
        {
            public string Tab;
            public string Id;
            public Image Background;
            public Image Icon;
        }

        class Tab
        {
            public string Key;
            public Button Button;
            public Image Background;
            public RTLTextMeshPro Label;
            public RectTransform Panel;
        }

        class SlotView
        {
            public RectTransform Rect;
            public Image Background;
            public Button Button;
            public Image Icon;
        }

        readonly List<Card> _cards = new List<Card>();
        readonly List<Tab> _tabs = new List<Tab>();
        readonly List<SlotView> _slots = new List<SlotView>();

        readonly List<string> _selectedAllies = new List<string>();
        readonly List<string> _selectedAbilities = new List<string>();
        readonly List<string> _selectedCharms = new List<string>();

        RectTransform _root;
        RectTransform _safeArea;
        RectTransform _slotRow;

        // Exposed for editor checks.
        public RectTransform Root { get { return _root; } }
        int _wave;
        Action _onStart;

        string _activeTabKey = "allies";

        public static SetupScreenView Build(Transform canvas, int wave, Action onStart)
        {
            if (canvas == null) return null;

            var view = new SetupScreenView();
            view._wave = wave;
            view._onStart = onStart;
            view.BuildInternal(canvas);
            return view;
        }

        void BuildInternal(Transform canvas)
        {
            LoadoutService.SyncUnlocks();
            LoadoutService.NormalizeLoadout();
            LoadSelectionFromSave();

            _root = UIFactory.CreateUIRoot(canvas, "Setup UI");

            var back = _root.gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            _safeArea = UIFactory.CreateUIRoot(_root, "Safe Area");
            _safeArea.gameObject.AddComponent<SafeAreaFitter>();

            BuildTitle();
            BuildTabs();
            BuildPanels();
            BuildSlots();
            BuildStartButton();

            SelectTab(0);
            RefreshCards();
            RefreshSlots();
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
        }

        // ---- title ----

        void BuildTitle()
        {
            var title = UIFactory.CreateTextRaw(_safeArea, Loc.T("setup.wave", _wave), 64, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 440f), new Vector2(1200f, 110f));
        }

        // ---- tabs ----

        void BuildTabs()
        {
            string[] keys = { "allies", "abilities", "charms" };
            float[] xs = { -400f, 0f, 400f };

            for (int i = 0; i < keys.Length; i++)
            {
                var tab = new Tab();
                tab.Key = keys[i];

                var button = UIFactory.CreateButton(_safeArea, "setup." + keys[i], delegate { SelectTab(i); },
                    new Vector2(360f, 84f), 34);

                UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(xs[i], 330f), new Vector2(360f, 84f));

                var image = button.GetComponent<Image>();
                image.color = TabColor;
                tab.Background = image;

                var label = button.GetComponentInChildren<RTLTextMeshPro>();
                if (label != null) label.color = TextColor;
                tab.Label = label;
                tab.Button = button;

                _tabs.Add(tab);
            }
        }

        // ---- panels + cards ----

        void BuildPanels()
        {
            string[] keys = { "allies", "abilities", "charms" };

            for (int t = 0; t < keys.Length; t++)
            {
                var tab = _tabs[t];
                var panel = UIFactory.CreateUIRoot(_safeArea, "Tab " + keys[t]);
                UIFactory.Place(panel, new Vector2(0f, 75f), new Vector2(1600f, 380f));
                tab.Panel = panel;
                panel.gameObject.SetActive(false);

                List<string> ids = null;
                if (keys[t] == "allies") ids = LoadoutService.GetUnlockedAllies();
                else if (keys[t] == "abilities") ids = LoadoutService.GetUnlockedAbilities();
                else ids = LoadoutService.GetOwnedCharms();

                if (ids.Count == 0)
                {
                    var empty = UIFactory.CreateText(panel, "setup.empty", 40, DimTextColor, TextAlignmentOptions.Center);
                    UIFactory.Place(empty.rectTransform, Vector2.zero, new Vector2(1200f, 120f));
                    continue;
                }

                float totalWidth = ids.Count * CardWidth + (ids.Count - 1) * CardGap;
                float startX = -totalWidth * 0.5f + CardWidth * 0.5f;

                for (int i = 0; i < ids.Count; i++)
                {
                    CreateCard(panel, keys[t], ids[i], new Vector2(startX + i * (CardWidth + CardGap), 0f));
                }
            }
        }

        void CreateCard(Transform panel, string tabKey, string id, Vector2 position)
        {
            var rect = CreateRect(panel, "Card " + id, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position,
                new Vector2(CardWidth, CardHeight));

            var background = rect.gameObject.AddComponent<Image>();
            background.color = CardColor;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;

            string capturedTab = tabKey;
            string capturedId = id;
            button.onClick.AddListener(delegate { Toggle(capturedTab, capturedId); });

            // Icon.
            var iconRect = CreateRect(rect, "Icon", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -30f), new Vector2(150f, 150f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite(IconFor(tabKey, id));
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            // Name.
            var name = UIFactory.CreateTextRaw(rect, Loc.T(NameKey(tabKey, id)), 30, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(name.rectTransform, new Vector2(0f, -15f), new Vector2(CardWidth - 20f, 50f));

            // Description.
            var desc = UIFactory.CreateTextRaw(rect, Loc.T(NameKey(tabKey, id) + ".desc"), 20, DimTextColor,
                TextAlignmentOptions.Top);
            UIFactory.Place(desc.rectTransform, new Vector2(0f, -115f), new Vector2(CardWidth - 30f, 140f));

            // Charm count.
            if (tabKey == "charms")
            {
                var count = UIFactory.CreateTextRaw(rect, LoadoutService.GetCharmCount(id).ToString(), 26, TextColor,
                    TextAlignmentOptions.Center, true);
                UIFactory.Place(count.rectTransform, new Vector2(0f, 145f), new Vector2(70f, 40f));
            }

            _cards.Add(new Card { Tab = tabKey, Id = id, Background = background, Icon = icon });
        }

        // ---- slots ----

        void BuildSlots()
        {
            int maxSlots = Mathf.Max(LoadoutService.SlotCount("allies"), LoadoutService.SlotCount("abilities"));
            maxSlots = Mathf.Max(maxSlots, LoadoutService.SlotCount("charms"));

            _slotRow = CreateRect(_safeArea, "Slots", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -185f), new Vector2(1600f, SlotSize));

            for (int i = 0; i < maxSlots; i++)
            {
                var rect = CreateRect(_slotRow, "Slot " + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(SlotSize, SlotSize));

                var background = rect.gameObject.AddComponent<Image>();
                background.color = SlotColor;

                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                button.transition = Selectable.Transition.None;

                _slots.Add(new SlotView { Rect = rect, Background = background, Button = button });
            }
        }

        void RefreshSlots()
        {
            var list = SelectedList(_activeTabKey);
            int slots = LoadoutService.SlotCount(_activeTabKey);

            float totalWidth = slots * SlotSize + (slots - 1) * SlotGap;
            float startX = -totalWidth * 0.5f + SlotSize * 0.5f;

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                bool active = i < slots;
                slot.Rect.gameObject.SetActive(active);
                if (!active) continue;

                slot.Rect.anchoredPosition = new Vector2(startX + i * (SlotSize + SlotGap), 0f);

                slot.Button.onClick.RemoveAllListeners();

                if (i < list.Count)
                {
                    string id = list[i];
                    slot.Background.sprite = AssetLibrary.GetSprite(IconFor(_activeTabKey, id));
                    slot.Background.color = Color.white;
                    slot.Background.type = Image.Type.Simple;
                    slot.Background.preserveAspect = true;

                    string capturedId = id;
                    slot.Button.interactable = true;
                    slot.Button.onClick.AddListener(delegate { Toggle(_activeTabKey, capturedId); });
                }
                else
                {
                    slot.Background.sprite = null;
                    slot.Background.color = SlotColor;
                    slot.Background.preserveAspect = false;
                    slot.Button.interactable = false;
                }
            }
        }

        // ---- start ----

        void BuildStartButton()
        {
            var button = UIFactory.CreateButton(_safeArea, "setup.start", OnStartPressed, new Vector2(460f, 100f), 40);
            UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(0f, -370f), new Vector2(460f, 100f));
        }

        void OnStartPressed()
        {
            SaveSelection();
            if (_onStart != null) _onStart();
        }

        // ---- tab selection ----

        void SelectTab(int index)
        {
            if (index < 0 || index >= _tabs.Count) return;

            _activeTabKey = _tabs[index].Key;

            for (int i = 0; i < _tabs.Count; i++)
            {
                bool active = i == index;
                _tabs[i].Panel.gameObject.SetActive(active);
                _tabs[i].Background.color = active ? TabActiveColor : TabColor;
                if (_tabs[i].Label != null)
                    _tabs[i].Label.color = active ? new Color(0.12f, 0.09f, 0.05f, 1f) : TextColor;
            }

            RefreshSlots();
        }

        // ---- selection ----

        void LoadSelectionFromSave()
        {
            _selectedAllies.Clear();
            _selectedAbilities.Clear();
            _selectedCharms.Clear();

            if (SaveService.Data == null) return;

            _selectedAllies.AddRange(SaveService.Data.loadoutAllies);
            _selectedAbilities.AddRange(SaveService.Data.loadoutAbilities);
            _selectedCharms.AddRange(SaveService.Data.loadoutCharms);
        }

        void SaveSelection()
        {
            if (SaveService.Data == null) SaveService.Load();

            SaveService.Data.loadoutAllies.Clear();
            SaveService.Data.loadoutAllies.AddRange(_selectedAllies);
            SaveService.Data.loadoutAbilities.Clear();
            SaveService.Data.loadoutAbilities.AddRange(_selectedAbilities);
            SaveService.Data.loadoutCharms.Clear();
            SaveService.Data.loadoutCharms.AddRange(_selectedCharms);

            SaveService.Save();
        }

        List<string> SelectedList(string tab)
        {
            if (tab == "allies") return _selectedAllies;
            if (tab == "abilities") return _selectedAbilities;
            return _selectedCharms;
        }

        void Toggle(string tab, string id)
        {
            var list = SelectedList(tab);
            int index = list.IndexOf(id);

            if (index >= 0)
            {
                list.RemoveAt(index);
            }
            else
            {
                int slots = LoadoutService.SlotCount(tab);
                if (slots <= 0) return;
                if (list.Count >= slots) list.RemoveAt(0);
                list.Add(id);
            }

            RefreshCards();
            RefreshSlots();
        }

        void RefreshCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                bool selected = SelectedList(card.Tab).Contains(card.Id);
                card.Background.color = selected ? CardSelectedColor : CardColor;
                if (card.Icon != null) card.Icon.color = selected ? Color.white : new Color(1f, 1f, 1f, 0.85f);
            }
        }

        // ---- helpers ----

        static string IconFor(string tab, string id)
        {
            if (tab == "allies") return "ui_ally_" + id;
            if (tab == "abilities") return Ability.IconFor(id);
            return "ui_" + id;
        }

        static string NameKey(string tab, string id)
        {
            if (tab == "allies") return "ally." + id;
            if (tab == "abilities") return "ability." + id;
            return "charm." + id;
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
