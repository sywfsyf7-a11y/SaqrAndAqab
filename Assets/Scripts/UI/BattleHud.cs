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
    // Battle HUD built from code: hero portrait + HP bar (top-left), wave progress (top-center),
    // dirhams + pause (top-right), date (healing) button (bottom-center), haybah + allies (bottom-left),
    // ability buttons with a circular cooldown (bottom-right) and the pause overlay.
    // Anchored for the 1920x1080 reference and kept inside the device safe area.
    public class BattleHud : MonoBehaviour
    {
        const string PortraitSprite = "ui_portrait_saqr";
        const string DirhamSprite = "ui_dirham";
        const string DateSprite = "ui_date";
        const string HaybahSprite = "ui_haybah";

        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color BarBackColor = new Color(0.07f, 0.06f, 0.05f, 0.78f);
        static readonly Color HpColor = new Color(0.82f, 0.22f, 0.20f, 1f);
        static readonly Color WaveColor = new Color(0.95f, 0.78f, 0.30f, 1f);
        static readonly Color HaybahColor = new Color(0.30f, 0.72f, 0.85f, 1f);
        static readonly Color CooldownColor = new Color(0f, 0f, 0f, 0.62f);
        static readonly Color OverlayColor = new Color(0.03f, 0.03f, 0.04f, 0.82f);
        static readonly Color DisabledColor = new Color(1f, 1f, 1f, 0.40f);

        class AllyButton
        {
            public AllyConfig Config;
            public Button Button;
            public Image Icon;
        }

        class AbilityButton
        {
            public Ability Ability;
            public Button Button;
            public Image Icon;
            public Image Cooldown;
            public bool WasReady;
            public float PulseTimer;
        }

        BattleWorld _world;
        Hero _hero;
        WaveManager _waves;
        HaybahMeter _haybah;
        ItemConfig _dateItem;

        public static BattleHud Current { get; private set; }

        public bool TryUseDate()
        {
            if (_dateItem == null || _hero == null || _hero.Health == null || !_hero.Health.IsAlive) return false;
            if (_dateCooldown > 0f) return false;
            if (SaveService.Data == null || SaveService.Data.dateCount <= 0) return false;
            OnDatePressed();
            return true;
        }

        readonly List<AllyButton> _allyButtons = new List<AllyButton>();
        readonly List<AbilityButton> _abilityButtons = new List<AbilityButton>();

        RectTransform _safeArea;
        Rect _lastSafeArea;
        int _lastScreenWidth;
        int _lastScreenHeight;

        Image _hpFill;
        RTLTextMeshPro _hpLabel;
        int _hpShown = -1;

        Image _waveFill;
        RTLTextMeshPro _waveLabel;
        int _waveShown = -1;

        GameObject _waveBarRoot;
        GameObject _bossBarRoot;
        Image _bossFill;
        RTLTextMeshPro _bossName;
        RTLTextMeshPro _bossHpLabel;
        int _bossHpShown = -1;

        Image _haybahFill;
        RTLTextMeshPro _haybahLabel;

        RTLTextMeshPro _dirhamLabel;

        Button _dateButton;
        RectTransform _dateCooldownFill;
        RTLTextMeshPro _dateCount;
        float _dateCooldown;

        GameObject _pauseRoot;
        bool _paused;

        public static BattleHud Create(Transform canvasParent, BattleWorld world)
        {
            if (canvasParent == null) return null;

            var go = new GameObject("Battle HUD", typeof(RectTransform), typeof(BattleHud));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvasParent, false);
            UIFactory.Fill(rect);

            var hud = go.GetComponent<BattleHud>();
            hud.Setup(world);
            return hud;
        }

        void Setup(BattleWorld world)
        {
            Current = this;
            _world = world;
            _hero = world != null ? world.Hero : null;
            _waves = world != null ? world.Waves : null;
            _haybah = world != null ? world.Haybah : null;

            _safeArea = UIFactory.CreateUIRoot(transform, "Safe Area");
            ApplySafeArea(true);

            BuildHeroPanel();
            BuildWavePanel();
            BuildDirhamPanel();
            BuildDateButton();
            BuildHaybahPanel();
            BuildAllyButtons();
            BuildAbilityButtons();
            BuildPauseOverlay();

            InitDates();

            if (_waves != null)
            {
                _waves.OnWaveProgress += HandleWaveProgress;
                _waves.OnDirhamsChanged += HandleDirhamsChanged;
            }

            if (_haybah != null) _haybah.OnChanged += HandleHaybahChanged;

            RefreshHealth();
            RefreshWaveLabel();
            RefreshDirhams(_waves != null ? _waves.RunDirhams : 0);
            if (_haybah != null) HandleHaybahChanged(_haybah.Value);
        }

        void Update()
        {
            ApplySafeArea(false);
            RefreshHealth();
            RefreshWaveLabel();
            TickDate();
            RefreshAbilityButtons();
            RefreshBossBar();
            TickAbilityPulse();
        }

        void RefreshBossBar()
        {
            var boss = _waves != null ? _waves.Boss : null;
            bool hasBoss = boss != null && boss.Health != null && boss.Health.IsAlive;

            if (_waveBarRoot != null) _waveBarRoot.SetActive(!hasBoss);
            if (_waveLabel != null) _waveLabel.gameObject.SetActive(!hasBoss);
            if (_bossBarRoot != null) _bossBarRoot.SetActive(hasBoss);
            if (_bossName != null) _bossName.gameObject.SetActive(hasBoss);

            if (!hasBoss) return;

            int hp = boss.Health.hp;
            if (hp == _bossHpShown) return;
            _bossHpShown = hp;

            if (_bossFill != null)
                SetBar(_bossFill, boss.Health.max > 0 ? (float)hp / boss.Health.max : 0f);

            if (_bossHpLabel != null) _bossHpLabel.text = hp + " / " + boss.Health.max;
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
            if (_waves != null)
            {
                _waves.OnWaveProgress -= HandleWaveProgress;
                _waves.OnDirhamsChanged -= HandleDirhamsChanged;
            }

            if (_haybah != null) _haybah.OnChanged -= HandleHaybahChanged;

            // Never leave the game frozen behind a destroyed HUD.
            HitStop.Cancel();
            Time.timeScale = 1f;
        }

        // ---- top-left: portrait + HP ----

        void BuildHeroPanel()
        {
            var portraitRect = CreateRect(_safeArea, "Portrait", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -24f), new Vector2(120f, 120f));

            var portrait = portraitRect.gameObject.AddComponent<Image>();
            portrait.sprite = AssetLibrary.GetSprite(PortraitSprite);
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;

            Image hpFill;
            var hpBar = CreateBar(_safeArea, "HP Bar", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(164f, -54f), new Vector2(320f, 34f), HpColor, out hpFill);
            _hpFill = hpFill;

            _hpLabel = UIFactory.CreateTextRaw(hpBar, "100 / 100", 22, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Fill(_hpLabel.rectTransform);
        }

        // ---- top-center: wave label + progress ----

        void BuildWavePanel()
        {
            _waveLabel = UIFactory.CreateTextRaw(_safeArea, "", 34, TextColor, TextAlignmentOptions.Center, true);
            var labelRect = _waveLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -18f);
            labelRect.sizeDelta = new Vector2(600f, 50f);

            var waveBarRect = CreateBar(_safeArea, "Wave Bar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -74f), new Vector2(440f, 22f), WaveColor, out var waveFill);
            _waveFill = waveFill;
            _waveBarRoot = waveBarRect.gameObject;
            SetBar(_waveFill, 0f);

            // Boss HP bar (replaces the wave bar while the boss lives).
            var bossBarRect = CreateBar(_safeArea, "Boss Bar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -74f), new Vector2(700f, 30f), new Color(0.85f, 0.20f, 0.20f, 1f), out var bossFill);
            _bossFill = bossFill;
            _bossBarRoot = bossBarRect.gameObject;
            _bossBarRoot.SetActive(false);
            SetBar(_bossFill, 0f);

            _bossName = UIFactory.CreateTextRaw(_safeArea, Loc.T("enemy.marid"), 28, TextColor, TextAlignmentOptions.Center, true);
            var bossNameRect = _bossName.rectTransform;
            bossNameRect.anchorMin = new Vector2(0.5f, 1f);
            bossNameRect.anchorMax = new Vector2(0.5f, 1f);
            bossNameRect.pivot = new Vector2(0.5f, 1f);
            bossNameRect.anchoredPosition = new Vector2(0f, -10f);
            bossNameRect.sizeDelta = new Vector2(700f, 40f);
            _bossName.gameObject.SetActive(false);

            _bossHpLabel = UIFactory.CreateTextRaw(bossBarRect, "", 20, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Fill(_bossHpLabel.rectTransform);
        }

        // ---- top-right: dirhams + pause ----

        void BuildDirhamPanel()
        {
            var pause = UIFactory.CreateButton(_safeArea, "pause.title", OnPausePressed,
                new Vector2(150f, 84f), 34);
            var pauseRect = pause.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(1f, 1f);
            pauseRect.anchorMax = new Vector2(1f, 1f);
            pauseRect.pivot = new Vector2(1f, 1f);
            pauseRect.anchoredPosition = new Vector2(-28f, -24f);

            var counter = CreateRect(_safeArea, "Dirhams", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-198f, -24f), new Vector2(260f, 84f));

            var iconRect = CreateRect(counter, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(62f, 62f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite(DirhamSprite);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            _dirhamLabel = UIFactory.CreateTextRaw(counter, "0", 34, TextColor, TextAlignmentOptions.Left, true);
            var labelRect = _dirhamLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(74f, 0f);
            labelRect.sizeDelta = new Vector2(180f, 60f);
        }

        // ---- bottom-center: date (healing) button ----

        void BuildDateButton()
        {
            var buttonRect = CreateRect(_safeArea, "Date Button", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 54f), new Vector2(100f, 100f));

            var image = buttonRect.gameObject.AddComponent<Image>();
            image.sprite = AssetLibrary.GetSprite(DateSprite);
            image.preserveAspect = true;
            image.raycastTarget = true;

            _dateButton = buttonRect.gameObject.AddComponent<Button>();
            _dateButton.targetGraphic = image;
            _dateButton.onClick.AddListener(OnDatePressed);

            var caption = UIFactory.CreateText(_safeArea, "hud.date", 22, TextColor, TextAlignmentOptions.Center, true);
            var captionRect = caption.rectTransform;
            captionRect.anchorMin = new Vector2(0.5f, 0f);
            captionRect.anchorMax = new Vector2(0.5f, 0f);
            captionRect.pivot = new Vector2(0.5f, 0f);
            captionRect.anchoredPosition = new Vector2(0f, 16f);
            captionRect.sizeDelta = new Vector2(140f, 30f);

            _dateCooldownFill = CreateRect(buttonRect, "Cooldown", Vector2.zero, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var cooldown = _dateCooldownFill.gameObject.AddComponent<Image>();
            cooldown.color = CooldownColor;
            cooldown.raycastTarget = false;
            SetCooldown(0f);

            _dateCount = UIFactory.CreateTextRaw(buttonRect, "0", 26, TextColor, TextAlignmentOptions.Center, true);
            var countRect = _dateCount.rectTransform;
            countRect.anchorMin = new Vector2(1f, 0f);
            countRect.anchorMax = new Vector2(1f, 0f);
            countRect.pivot = new Vector2(1f, 0f);
            countRect.anchoredPosition = new Vector2(-4f, 4f);
            countRect.sizeDelta = new Vector2(40f, 30f);
        }

        // ---- bottom-left: haybah meter + ally buttons ----

        void BuildHaybahPanel()
        {
            var iconRect = CreateRect(_safeArea, "Haybah Icon", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(28f, 36f), new Vector2(56f, 56f));
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.sprite = AssetLibrary.GetSprite(HaybahSprite);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            Image fill;
            var bar = CreateBar(_safeArea, "Haybah Bar", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(96f, 47f), new Vector2(300f, 34f), HaybahColor, out fill);
            _haybahFill = fill;

            _haybahLabel = UIFactory.CreateTextRaw(bar, "0", 24, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Fill(_haybahLabel.rectTransform);
        }

        void BuildAllyButtons()
        {
            if (_world == null) return;

            var allies = _world.SelectedAllies;
            for (int i = 0; i < allies.Count; i++)
            {
                var config = allies[i];
                if (config == null) continue;

                var rect = CreateRect(_safeArea, "Ally " + config.id, new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(28f + i * 132f, 100f), new Vector2(120f, 120f));

                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = AssetLibrary.GetSprite("ui_ally_" + config.id);
                image.preserveAspect = true;
                image.raycastTarget = true;

                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;

                var captured = config;
                button.onClick.AddListener(delegate { OnAllyPressed(captured); });

                // Haybah cost strip at the bottom of the button.
                var stripRect = CreateRect(rect, "Cost", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    Vector2.zero, new Vector2(120f, 30f));
                var strip = stripRect.gameObject.AddComponent<Image>();
                strip.color = new Color(0f, 0f, 0f, 0.55f);
                strip.raycastTarget = false;

                var cost = UIFactory.CreateTextRaw(stripRect, config.haybahCost.ToString(), 24, TextColor,
                    TextAlignmentOptions.Center, true);
                UIFactory.Fill(cost.rectTransform);

                _allyButtons.Add(new AllyButton { Config = config, Button = button, Icon = image });
            }

            RefreshAllyButtons();
        }

        void OnAllyPressed(AllyConfig config)
        {
            if (_world == null || config == null) return;
            if (_world.TrySpawnAlly(config)) RefreshAllyButtons();
        }

        void RefreshAllyButtons()
        {
            int value = _haybah != null ? _haybah.Value : 0;

            for (int i = 0; i < _allyButtons.Count; i++)
            {
                var entry = _allyButtons[i];
                if (entry.Config == null || entry.Button == null) continue;

                bool affordable = value >= entry.Config.haybahCost;
                entry.Button.interactable = affordable;
                if (entry.Icon != null) entry.Icon.color = affordable ? Color.white : DisabledColor;
            }
        }

        // ---- bottom-right: ability buttons ----

        void BuildAbilityButtons()
        {
            if (_world == null) return;

            var abilities = _world.Abilities;
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];
                if (ability == null) continue;

                var rect = CreateRect(_safeArea, "Ability " + ability.Id, new Vector2(1f, 0f), new Vector2(1f, 0f),
                    new Vector2(-28f - i * 132f, 36f), new Vector2(150f, 150f));

                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = AssetLibrary.GetSprite(ability.IconId);
                image.preserveAspect = true;
                image.raycastTarget = true;

                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;

                var captured = ability;
                button.onClick.AddListener(delegate { OnAbilityPressed(captured); });

                // Circular cooldown fill drawn on top of the icon.
                var cooldownRect = CreateRect(rect, "Cooldown", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero);
                UIFactory.Fill(cooldownRect);

                var cooldown = cooldownRect.gameObject.AddComponent<Image>();
                cooldown.color = CooldownColor;
                cooldown.raycastTarget = false;
                cooldown.type = Image.Type.Filled;
                cooldown.fillMethod = Image.FillMethod.Radial360;
                cooldown.fillOrigin = (int)Image.Origin360.Top;
                cooldown.fillClockwise = true;
                cooldown.fillAmount = 0f;
                cooldown.gameObject.SetActive(false);

                _abilityButtons.Add(new AbilityButton
                {
                    Ability = ability,
                    Button = button,
                    Icon = image,
                    Cooldown = cooldown
                });
            }

            RefreshAbilityButtons();
        }

        void OnAbilityPressed(Ability ability)
        {
            if (_world == null || ability == null) return;

            _world.TryUseAbility(ability);
            RefreshAbilityButtons();
        }

        void RefreshAbilityButtons()
        {
            bool alive = _hero != null && _hero.Health != null && _hero.Health.IsAlive;

            for (int i = 0; i < _abilityButtons.Count; i++)
            {
                var entry = _abilityButtons[i];
                if (entry.Ability == null || entry.Button == null) continue;

                bool ready = alive && entry.Ability.IsReady;
                if (ready && !entry.WasReady) entry.PulseTimer = 0.4f;
                entry.WasReady = ready;
                entry.Button.interactable = ready;
                if (entry.Icon != null) entry.Icon.color = ready ? Color.white : DisabledColor;

                if (entry.Cooldown != null)
                {
                    float fraction = entry.Ability.CooldownFraction;
                    entry.Cooldown.fillAmount = fraction;

                    bool show = fraction > 0f;
                    if (entry.Cooldown.gameObject.activeSelf != show) entry.Cooldown.gameObject.SetActive(show);
                }
            }
        }

        void TickAbilityPulse()
        {
            for (int i = 0; i < _abilityButtons.Count; i++)
            {
                var entry = _abilityButtons[i];
                if (entry.Button == null) continue;

                if (entry.PulseTimer > 0f)
                {
                    entry.PulseTimer -= Time.deltaTime;
                    float t = 1f - Mathf.Clamp01(entry.PulseTimer / 0.4f);
                    float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.18f;
                    entry.Button.transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    entry.Button.transform.localScale = Vector3.one;
                }
            }
        }

        // ---- pause overlay ----

        void BuildPauseOverlay()
        {
            var root = CreateRect(_safeArea, "Pause Overlay", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var back = root.gameObject.AddComponent<Image>();
            back.color = OverlayColor;
            back.raycastTarget = true;

            var title = UIFactory.CreateText(root, "pause.title", 100, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 260f), new Vector2(900f, 160f));

            var resume = UIFactory.CreateButton(root, "common.resume", OnResumePressed, new Vector2(440f, 110f), 40);
            UIFactory.Place(resume.GetComponent<RectTransform>(), new Vector2(0f, 50f), new Vector2(440f, 110f));

            var restart = UIFactory.CreateButton(root, "common.restart", OnRestartPressed, new Vector2(440f, 110f), 40);
            UIFactory.Place(restart.GetComponent<RectTransform>(), new Vector2(0f, -90f), new Vector2(440f, 110f));

            var menu = UIFactory.CreateButton(root, "common.menu", OnMenuPressed, new Vector2(440f, 110f), 40);
            UIFactory.Place(menu.GetComponent<RectTransform>(), new Vector2(0f, -230f), new Vector2(440f, 110f));

            _pauseRoot = root.gameObject;
            _pauseRoot.SetActive(false);
        }

        // ---- dates ----

        void InitDates()
        {
            _dateItem = FindItem("date");
            if (_dateItem == null)
            {
                Debug.LogWarning("[HUD] No 'date' item in balance.json.");
                return;
            }

            if (SaveService.Data == null) SaveService.Load();

            // The saved count starts at startCount the first time only.
            if (!SaveService.Data.datesInitialized)
            {
                SaveService.Data.dateCount = _dateItem.startCount;
                SaveService.Data.datesInitialized = true;
                SaveService.Save();
            }

            RefreshDateLabel();
        }

        void TickDate()
        {
            if (_dateItem == null) return;

            if (_dateCooldown > 0f)
            {
                _dateCooldown -= Time.deltaTime;
                if (_dateCooldown < 0f) _dateCooldown = 0f;
            }

            float fraction = _dateItem.cooldown > 0f ? Mathf.Clamp01(_dateCooldown / _dateItem.cooldown) : 0f;
            SetCooldown(fraction);

            int count = SaveService.Data != null ? SaveService.Data.dateCount : 0;
            bool alive = _hero != null && _hero.Health != null && _hero.Health.IsAlive;
            if (_dateButton != null) _dateButton.interactable = count > 0 && _dateCooldown <= 0f && alive;
        }

        void OnDatePressed()
        {
            if (_dateItem == null || _hero == null || _hero.Health == null) return;
            if (!_hero.Health.IsAlive) return;
            if (_dateCooldown > 0f) return;
            if (SaveService.Data == null || SaveService.Data.dateCount <= 0) return;

            _hero.Health.Heal(_dateItem.healAmount);
            AudioService.Play("heal");

            SaveService.Data.dateCount--;
            _dateCooldown = _dateItem.cooldown;
            SaveService.Save();

            RefreshDateLabel();
        }

        void RefreshDateLabel()
        {
            if (_dateCount == null) return;

            int count = SaveService.Data != null ? SaveService.Data.dateCount : 0;
            _dateCount.text = count.ToString();
        }

        void SetCooldown(float fraction)
        {
            if (_dateCooldownFill == null) return;

            fraction = Mathf.Clamp01(fraction);
            _dateCooldownFill.anchorMin = Vector2.zero;
            _dateCooldownFill.anchorMax = new Vector2(1f, fraction);
            _dateCooldownFill.offsetMin = Vector2.zero;
            _dateCooldownFill.offsetMax = Vector2.zero;
            _dateCooldownFill.gameObject.SetActive(fraction > 0f);
        }

        // ---- pause ----

        void OnPausePressed()
        {
            // Ignore the pause tap during the 40 ms ability hit-stop (it would freeze the game for good).
            if (_paused || HitStop.IsActive) return;

            _paused = true;
            if (_pauseRoot != null) _pauseRoot.SetActive(true);
            Time.timeScale = 0f;
        }

        void OnResumePressed()
        {
            ClosePause();
        }

        void OnRestartPressed()
        {
            ClosePause();

            if (_hero != null && _hero.Health != null && _hero.Health.IsAlive)
                _hero.Health.Heal(_hero.Health.max);

            if (_waves != null) _waves.StartWave(Mathf.Max(1, _waves.CurrentWave));
        }

        void OnMenuPressed()
        {
            Time.timeScale = 1f;
            ScreenManager.Show(SaqrAndAqab.Core.Screen.Menu);
        }

        void ClosePause()
        {
            _paused = false;
            if (_pauseRoot != null) _pauseRoot.SetActive(false);
            HitStop.Cancel();
            Time.timeScale = 1f;
        }

        // ---- refresh ----

        void HandleWaveProgress(float progress)
        {
            SetBar(_waveFill, progress);
            RefreshWaveLabel();
        }

        void HandleDirhamsChanged(int total)
        {
            RefreshDirhams(total);
        }

        void HandleHaybahChanged(int value)
        {
            if (_haybahFill != null && _haybah != null && _haybah.Max > 0)
                SetBar(_haybahFill, (float)value / _haybah.Max);

            if (_haybahLabel != null) _haybahLabel.text = value.ToString();

            RefreshAllyButtons();
        }

        void RefreshHealth()
        {
            if (_hpFill == null || _hero == null || _hero.Health == null) return;

            int hp = _hero.Health.hp;
            if (hp == _hpShown) return;
            _hpShown = hp;

            SetBar(_hpFill, _hero.Health.max > 0 ? (float)hp / _hero.Health.max : 0f);
            if (_hpLabel != null) _hpLabel.text = hp + " / " + _hero.Health.max;
        }

        void RefreshWaveLabel()
        {
            if (_waveLabel == null) return;

            int wave = _waves != null ? Mathf.Max(1, _waves.CurrentWave) : 1;
            if (wave == _waveShown) return;
            _waveShown = wave;

            _waveLabel.text = Loc.T("hud.wave", wave);
        }

        void RefreshDirhams(int total)
        {
            if (_dirhamLabel == null) return;

            _dirhamLabel.text = total + " " + Loc.T("hud.dirham");
        }

        // ---- helpers ----

        void ApplySafeArea(bool force)
        {
            if (_safeArea == null) return;

            int width = UnityEngine.Screen.width;
            int height = UnityEngine.Screen.height;
            if (width <= 0 || height <= 0) return;

            Rect safe = UnityEngine.Screen.safeArea;
            if (!force && safe == _lastSafeArea && width == _lastScreenWidth && height == _lastScreenHeight) return;

            _lastSafeArea = safe;
            _lastScreenWidth = width;
            _lastScreenHeight = height;

            _safeArea.anchorMin = new Vector2(safe.xMin / width, safe.yMin / height);
            _safeArea.anchorMax = new Vector2(safe.xMax / width, safe.yMax / height);
            _safeArea.offsetMin = Vector2.zero;
            _safeArea.offsetMax = Vector2.zero;
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size)
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

        static RectTransform CreateBar(Transform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, Color fillColor, out Image fillImage)
        {
            var rect = CreateRect(parent, name, anchor, pivot, position, size);

            var back = rect.gameObject.AddComponent<Image>();
            back.color = BarBackColor;
            back.raycastTarget = false;

            var fillRect = CreateRect(rect, "Fill", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            fillImage = fillRect.gameObject.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            return rect;
        }

        static void SetBar(Image fill, float amount)
        {
            if (fill == null) return;

            var rect = fill.rectTransform;
            rect.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
    }
}
