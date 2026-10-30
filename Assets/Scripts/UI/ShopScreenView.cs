using System;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.UI
{
    // Shop screen built from code: dirham balance, three tabs (hero / allies / items),
    // upgrade rows and the buy buttons for dates and charms.
    public class ShopScreenView
    {
        static readonly Color BackColor = new Color(0.04f, 0.05f, 0.07f, 0.97f);
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color DimTextColor = new Color(0.72f, 0.70f, 0.66f, 1f);
        static readonly Color GoldColor = new Color(0.95f, 0.78f, 0.30f, 1f);
        static readonly Color RowColor = new Color(0.10f, 0.11f, 0.14f, 0.96f);
        static readonly Color TabColor = new Color(0.16f, 0.17f, 0.20f, 1f);
        static readonly Color TabActiveColor = new Color(0.85f, 0.66f, 0.28f, 1f);

        RectTransform _root;
        RectTransform _safeArea;
        RectTransform _content;
        RTLTextMeshPro _balanceLabel;
        int _activeTab;
        readonly System.Collections.Generic.List<Image> _tabImages = new System.Collections.Generic.List<Image>();

        public RectTransform Root { get { return _root; } }

        public static ShopScreenView Build(Transform canvas, Action onBack)
        {
            if (canvas == null) return null;

            var view = new ShopScreenView();
            view.BuildInternal(canvas, onBack);
            return view;
        }

        void BuildInternal(Transform canvas, Action onBack)
        {
            if (SaveService.Data == null) SaveService.Load();

            _root = UIFactory.CreateUIRoot(canvas, "Shop UI");

            var back = _root.gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            _safeArea = UIFactory.CreateUIRoot(_root, "Safe Area");
            _safeArea.gameObject.AddComponent<SafeAreaFitter>();

            var title = UIFactory.CreateText(_safeArea, "shop.title", 64, GoldColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 440f), new Vector2(900f, 110f));

            // Back button (top-left).
            var backButton = UIFactory.CreateButton(_safeArea, "common.back", delegate { if (onBack != null) onBack(); },
                new Vector2(220f, 84f), 34);
            UIFactory.Place(backButton.GetComponent<RectTransform>(), new Vector2(-760f, 440f), new Vector2(220f, 84f));

            // Dirham balance (top-right).
            var balanceRect = CreateRect(_safeArea, "Balance", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-28f, -24f), new Vector2(260f, 84f));
            var iconRect = CreateRect(balanceRect, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(62f, 62f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite("ui_dirham");
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            _balanceLabel = UIFactory.CreateTextRaw(balanceRect, "0", 34, TextColor, TextAlignmentOptions.Left, true);
            var labelRect = _balanceLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(74f, 0f);
            labelRect.sizeDelta = new Vector2(180f, 60f);

            RefreshBalance();

            // Tabs.
            string[] tabKeys = { "shop.tab_hero", "shop.tab_allies", "shop.tab_items" };
            string[] tabIds = { "hero", "allies", "items" };
            float[] xs = { -400f, 0f, 400f };

            for (int i = 0; i < tabKeys.Length; i++)
            {
                int captured = i;
                var button = UIFactory.CreateButton(_safeArea, tabKeys[i], delegate { SelectTab(captured); },
                    new Vector2(360f, 84f), 30);
                UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(xs[i], 330f), new Vector2(360f, 84f));

                var image = button.GetComponent<Image>();
                image.color = i == 0 ? TabActiveColor : TabColor;
                _tabImages.Add(image);
            }

            // Content panel.
            _content = CreateRect(_safeArea, "Content", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f), new Vector2(1700f, 620f));

            SelectTab(0);
        }

        public void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
        }

        void SelectTab(int index)
        {
            _activeTab = index;

            for (int i = 0; i < _tabImages.Count; i++)
            {
                if (_tabImages[i] != null)
                    _tabImages[i].color = (i == index) ? TabActiveColor : TabColor;
            }

            RefreshContent();
        }

        void RefreshBalance()
        {
            if (_balanceLabel == null) return;
            int dirhams = SaveService.Data != null ? SaveService.Data.dirhams : 0;
            _balanceLabel.text = dirhams + " " + Loc.T("hud.dirham");
        }

        void RefreshContent()
        {
            if (_content == null) return;

            for (int i = _content.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_content.GetChild(i).gameObject);

            if (_activeTab == 0) BuildHeroUpgrades();
            else if (_activeTab == 1) BuildAllyUpgrades();
            else BuildItems();
        }

        // ---- hero tab: hero, sword, bow, haybah, gate, slash ----

        void BuildHeroUpgrades()
        {
            string[] ids = { "hero", "sword", "bow", "haybah", "gate", "slash" };
            float y = 250f;
            for (int i = 0; i < ids.Length; i++)
            {
                AddUpgradeRow(ids[i], new Vector2(0f, y - i * 105f));
            }
        }

        // ---- allies tab: farmer, archer ----

        void BuildAllyUpgrades()
        {
            string[] ids = { "farmer", "archer" };
            float y = 150f;
            for (int i = 0; i < ids.Length; i++)
            {
                AddUpgradeRow(ids[i], new Vector2(0f, y - i * 105f));
            }
        }

        void AddUpgradeRow(string id, Vector2 position)
        {
            var cfg = FindUpgrade(id);
            if (cfg == null) return;

            var row = CreateRect(_content, "Row " + id, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(1600f, 95f));

            var bg = row.gameObject.AddComponent<Image>();
            bg.color = RowColor;
            bg.raycastTarget = false;

            // Icon.
            var iconRect = CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(-720f, 0f), new Vector2(70f, 70f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite(UpgradeIcon(id));
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            // Name.
            var name = UIFactory.CreateTextRaw(row, Loc.T("upgrade." + id), 30, TextColor, TextAlignmentOptions.Right, true);
            UIFactory.Place(name.rectTransform, new Vector2(-360f, 15f), new Vector2(600f, 40f));

            int level = UpgradeService.Level(id);
            int maxLevel = UpgradeService.MaxLevel(id);
            bool isMax = level >= maxLevel;

            // Status text (cost or max or not enough).
            string status;
            if (isMax)
            {
                status = Loc.T("shop.max");
            }
            else
            {
                int cost = UpgradeService.Cost(id);
                bool affordable = SaveService.Data != null && SaveService.Data.dirhams >= cost;
                if (!affordable) status = Loc.T("shop.not_enough");
                else status = cost + " " + Loc.T("hud.dirham");
            }

            var statusText = UIFactory.CreateTextRaw(row, status, 26,
                isMax ? DimTextColor : (status == Loc.T("shop.not_enough") ? new Color(0.95f, 0.35f, 0.30f, 1f) : GoldColor),
                TextAlignmentOptions.Left, true);
            UIFactory.Place(statusText.rectTransform, new Vector2(260f, 0f), new Vector2(320f, 40f));

            // Buy button.
            int nextLevel = Mathf.Min(level + 1, maxLevel);
            string label = isMax ? Loc.T("shop.max") : Loc.T("shop.level_up", Loc.T("upgrade." + id), nextLevel);

            var button = UIFactory.CreateButton(row, null, null, new Vector2(260f, 70f), 26);
            // Override the label text (CreateButton with locKey null would log missing key, so set manually).
            var btnLabel = button.GetComponentInChildren<RTLTextMeshPro>();
            if (btnLabel != null) btnLabel.text = label;

            UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(620f, 0f), new Vector2(260f, 70f));

            bool canBuy = !isMax && SaveService.Data != null && SaveService.Data.dirhams >= UpgradeService.Cost(id);
            button.interactable = canBuy;
            var btnImage = button.GetComponent<Image>();
            if (btnImage != null) btnImage.color = canBuy ? new Color(0.85f, 0.66f, 0.28f, 1f) : new Color(0.40f, 0.38f, 0.34f, 1f);

            if (canBuy)
            {
                string capturedId = id;
                button.onClick.AddListener(delegate
                {
                    if (UpgradeService.TryBuy(capturedId))
                    {
                        RefreshBalance();
                        RefreshContent();
                    }
                });
            }
        }

        // ---- items tab: dates + charms ----

        void BuildItems()
        {
            float y = 250f;
            AddDateRow(new Vector2(0f, y));
            y -= 105f;

            var balance = ConfigService.Balance;
            if (balance != null && balance.charms != null)
            {
                for (int i = 0; i < balance.charms.Count; i++)
                {
                    var charm = balance.charms[i];
                    if (charm == null) continue;
                    AddCharmRow(charm, new Vector2(0f, y - i * 105f));
                }
            }
        }

        void AddDateRow(Vector2 position)
        {
            var item = FindItem("date");
            if (item == null) return;

            var row = CreateRect(_content, "Row date", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(1600f, 95f));
            var bg = row.gameObject.AddComponent<Image>();
            bg.color = RowColor;
            bg.raycastTarget = false;

            var iconRect = CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(-720f, 0f), new Vector2(70f, 70f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite("ui_date");
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var name = UIFactory.CreateTextRaw(row, Loc.T("item.date"), 30, TextColor, TextAlignmentOptions.Right, true);
            UIFactory.Place(name.rectTransform, new Vector2(-360f, 15f), new Vector2(600f, 40f));

            int owned = SaveService.Data != null ? SaveService.Data.dateCount : 0;
            var ownedText = UIFactory.CreateTextRaw(row, Loc.T("shop.owned", owned), 26, DimTextColor, TextAlignmentOptions.Left, true);
            UIFactory.Place(ownedText.rectTransform, new Vector2(260f, 15f), new Vector2(320f, 40f));

            int price = item.priceDirhams;
            bool affordable = SaveService.Data != null && SaveService.Data.dirhams >= price;

            var button = UIFactory.CreateButton(row, null, null, new Vector2(260f, 70f), 26);
            var btnLabel = button.GetComponentInChildren<RTLTextMeshPro>();
            if (btnLabel != null) btnLabel.text = price + " " + Loc.T("hud.dirham");

            UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(620f, 0f), new Vector2(260f, 70f));

            button.interactable = affordable;
            var btnImage = button.GetComponent<Image>();
            if (btnImage != null) btnImage.color = affordable ? new Color(0.85f, 0.66f, 0.28f, 1f) : new Color(0.40f, 0.38f, 0.34f, 1f);

            if (affordable)
            {
                button.onClick.AddListener(delegate
                {
                    if (SaveService.Data == null) return;
                    if (SaveService.Data.dirhams < price) return;

                    SaveService.Data.dirhams -= price;
                    if (!SaveService.Data.datesInitialized)
                    {
                        SaveService.Data.dateCount = item.startCount;
                        SaveService.Data.datesInitialized = true;
                    }
                    SaveService.Data.dateCount++;
                    SaveService.Save();

                    RefreshBalance();
                    RefreshContent();
                });
            }
        }

        void AddCharmRow(CharmConfig charm, Vector2 position)
        {
            if (charm == null) return;

            var row = CreateRect(_content, "Row " + charm.id, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(1600f, 95f));
            var bg = row.gameObject.AddComponent<Image>();
            bg.color = RowColor;
            bg.raycastTarget = false;

            var iconRect = CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(-720f, 0f), new Vector2(70f, 70f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite("ui_" + charm.id);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var name = UIFactory.CreateTextRaw(row, Loc.T("charm." + charm.id), 30, TextColor, TextAlignmentOptions.Right, true);
            UIFactory.Place(name.rectTransform, new Vector2(-360f, 15f), new Vector2(600f, 40f));

            int owned = LoadoutService.GetCharmCount(charm.id);
            var ownedText = UIFactory.CreateTextRaw(row, Loc.T("shop.owned", owned), 26, DimTextColor, TextAlignmentOptions.Left, true);
            UIFactory.Place(ownedText.rectTransform, new Vector2(260f, 15f), new Vector2(320f, 40f));

            int price = charm.priceDirhams;
            bool affordable = SaveService.Data != null && SaveService.Data.dirhams >= price;

            var button = UIFactory.CreateButton(row, null, null, new Vector2(260f, 70f), 26);
            var btnLabel = button.GetComponentInChildren<RTLTextMeshPro>();
            if (btnLabel != null) btnLabel.text = price + " " + Loc.T("hud.dirham");

            UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(620f, 0f), new Vector2(260f, 70f));

            button.interactable = affordable;
            var btnImage = button.GetComponent<Image>();
            if (btnImage != null) btnImage.color = affordable ? new Color(0.85f, 0.66f, 0.28f, 1f) : new Color(0.40f, 0.38f, 0.34f, 1f);

            if (affordable)
            {
                string capturedId = charm.id;
                int capturedPrice = price;
                button.onClick.AddListener(delegate
                {
                    if (SaveService.Data == null) return;
                    if (SaveService.Data.dirhams < capturedPrice) return;

                    SaveService.Data.dirhams -= capturedPrice;
                    LoadoutService.AddCharm(capturedId, LoadoutService.CharmDropAmount());
                    SaveService.Save();

                    RefreshBalance();
                    RefreshContent();
                });
            }
        }

        // ---- helpers ----

        static UpgradeConfig FindUpgrade(string id)
        {
            var balance = ConfigService.Balance;
            if (balance == null || balance.upgrades == null) return null;

            for (int i = 0; i < balance.upgrades.Count; i++)
            {
                var upgrade = balance.upgrades[i];
                if (upgrade != null && upgrade.id == id) return upgrade;
            }

            return null;
        }

        static ItemConfig FindItem(string id)
        {
            var balance = ConfigService.Balance;
            if (balance == null || balance.items == null) return null;

            for (int i = 0; i < balance.items.Count; i++)
            {
                var item = balance.items[i];
                if (item != null && item.id == id) return item;
            }

            return null;
        }

        static string UpgradeIcon(string id)
        {
            switch (id)
            {
                case "hero": return "ui_portrait_saqr";
                case "sword": return "ui_icon_slash";
                case "bow": return "ui_icon_bow";
                case "haybah": return "ui_haybah";
                case "gate": return "prop_gate";
                case "slash": return "ui_icon_slash";
                case "farmer": return "ui_ally_farmer";
                case "archer": return "ui_ally_archer";
                default: return "ui_button";
            }
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
