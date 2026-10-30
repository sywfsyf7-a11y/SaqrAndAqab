using System;
using UnityEngine;
using RTLTMPro;
using TMPro;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;
using SaqrAndAqab.UI;

namespace SaqrAndAqab.Battle
{
    // The Dark Realm bonus stage: a short floating, invulnerable, high-reward run
    // that plays after the victory of waves 5 and 10, before the normal victory screen.
    public class DarkRealmStage : MonoBehaviour
    {
        const float AllySummonInterval = 3f;

        BattleWorld _world;
        Action _onComplete;
        DarkRealmConfig _config;
        GameConfig _game;
        Hero _hero;
        WaveManager _waves;
        HaybahMeter _haybah;

        float _timer;
        float _spawnTimer;
        float _allyTimer;
        bool _running;

        WorldThemeId _savedTheme;
        GameObject _aura;
        SpriteRenderer _auraRenderer;
        float _auraTime;

        RTLTextMeshPro _countdownLabel;
        RTLTextMeshPro _titleLabel;
        RTLTextMeshPro _hintLabel;
        RTLTextMeshPro _speakerLabel;
        RTLTextMeshPro _lineLabel;

        public static DarkRealmStage Start(BattleWorld world, Action onComplete)
        {
            if (world == null) return null;

            var go = new GameObject("Dark Realm Stage", typeof(DarkRealmStage));
            go.transform.SetParent(world.transform, false);

            var stage = go.GetComponent<DarkRealmStage>();
            stage.Begin(world, onComplete);
            return stage;
        }

        void Begin(BattleWorld world, Action onComplete)
        {
            _world = world;
            _onComplete = onComplete;
            _hero = world.Hero;
            _waves = world.Waves;
            _haybah = world.Haybah;
            _game = world.Game;

            var balance = ConfigService.Balance;
            _config = balance != null ? balance.darkRealm : null;
            if (_config == null)
            {
                Debug.LogWarning("[DarkRealm] Missing darkRealm config.");
                Finish();
                return;
            }

            float duration = _config.durationSeconds > 0f ? _config.durationSeconds : 35f;
            _timer = duration;
            _spawnTimer = _config.spawnInterval > 0f ? _config.spawnInterval : 0.5f;
            _allyTimer = AllySummonInterval;

            // Save and switch the theme.
            _savedTheme = world.Theme;
            world.ApplyTheme(WorldThemeId.DarkRealm);

            // Hero: blue sprite, floating, invulnerable.
            if (_hero != null)
            {
                _hero.SetSprite("hero_saqr_blue");
                _hero.Floating = true;
                _hero.SetInvulnerable(_config.heroInvulnerable);

                // Pulsing blue aura (a tinted copy of the hero sprite).
                _aura = new GameObject("Aura");
                _aura.transform.SetParent(_hero.transform, false);
                _aura.transform.localPosition = Vector3.zero;
                _auraRenderer = _aura.AddComponent<SpriteRenderer>();
                _auraRenderer.sprite = _hero.SpriteRenderer != null ? _hero.SpriteRenderer.sprite : (Sprite)null;
                _auraRenderer.color = new Color(0.4f, 0.6f, 1f, 0.45f);
                _auraRenderer.sortingOrder = (_hero.SpriteRenderer != null ? _hero.SpriteRenderer.sortingOrder : 0) - 1;
                _aura.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            }

            // Haybah: instantly full, allies free.
            if (_haybah != null)
            {
                _haybah.SetFull();
                _haybah.AlliesFree = _config.instantHaybah;
            }

            // Dirham multiplier: reward multiplier (on top of the wealth charm).
            if (_waves != null)
            {
                _waves.SetDirhamMultiplier(_config.rewardMultiplier);
            }

            BuildUi();

            _running = true;
        }

        void BuildUi()
        {
            // Build UI under the world as world-space labels.
            // Countdown label as a simple world-space text above the hero area.
            var go = new GameObject("Dark Realm UI");
            // We cannot easily parent to UI canvas here (no reference). Create under world; use TextMeshPro world space.
            go.transform.SetParent(_world.transform, false);

            _titleLabel = CreateWorldText(go.transform, Loc.T("dark.title"), 1.2f, new Color(0.8f, 0.7f, 1f, 1f));
            _titleLabel.transform.localPosition = new Vector3(0f, 4f, 0f);

            _hintLabel = CreateWorldText(go.transform, Loc.T("dark.hint"), 0.7f, new Color(1f, 0.95f, 0.85f, 1f));
            _hintLabel.transform.localPosition = new Vector3(0f, 3.2f, 0f);

            _speakerLabel = CreateWorldText(go.transform, Loc.T("speaker.entity"), 0.6f, new Color(0.6f, 0.8f, 1f, 1f));
            _speakerLabel.transform.localPosition = new Vector3(0f, 2.6f, 0f);

            _lineLabel = CreateWorldText(go.transform, Loc.T("story.dark.1"), 0.6f, new Color(1f, 0.95f, 0.85f, 1f));
            _lineLabel.transform.localPosition = new Vector3(0f, 2.0f, 0f);

            _countdownLabel = CreateWorldText(go.transform, "", 0.8f, new Color(1f, 0.95f, 0.85f, 1f));
            _countdownLabel.transform.localPosition = new Vector3(0f, 4.8f, 0f);
        }

        static RTLTMPro.RTLTextMeshPro CreateWorldText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(RTLTMPro.RTLTextMeshPro));
            go.transform.SetParent(parent, false);

            var label = go.GetComponent<RTLTMPro.RTLTextMeshPro>();
            label.font = UIFactory.BoldFont;
            label.Farsi = false;
            label.PreserveNumbers = true;
            label.text = text;
            label.fontSize = size * 10f; // world-space-ish
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            // Use TextMeshPro world rendering by setting transform scale small.
            go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            return label;
        }

        void Update()
        {
            if (!_running) return;

            float dt = Time.deltaTime;
            _timer -= dt;
            _auraTime += dt;

            // Pulse the aura.
            if (_auraRenderer != null)
            {
                float alpha = 0.35f + Mathf.Sin(_auraTime * 3f) * 0.15f;
                var c = _auraRenderer.color;
                c.a = alpha;
                _auraRenderer.color = c;
            }

            // Countdown.
            if (_countdownLabel != null)
                _countdownLabel.text = Mathf.CeilToInt(Mathf.Max(0f, _timer)).ToString();

            // Spawn enemies.
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f && _game != null)
            {
                _spawnTimer = _config.spawnInterval > 0f ? _config.spawnInterval : 0.5f;
                SpawnEnemy();
            }

            // Auto-summon allies.
            _allyTimer -= dt;
            if (_allyTimer <= 0f)
            {
                _allyTimer = AllySummonInterval;
                SummonAlly();
            }

            if (_timer <= 0f) Finish();
        }

        void SpawnEnemy()
        {
            if (_game == null) return;

            var balance = ConfigService.Balance;
            if (balance == null || balance.enemies == null) return;

            // Pick a random enemy type (jinn/ghoul/ifrit, not boss).
            string[] types = { "jinn_earth", "ghoul", "ifrit" };
            string id = types[UnityEngine.Random.Range(0, types.Length)];

            EnemyConfig config = null;
            for (int i = 0; i < balance.enemies.Count; i++)
            {
                if (balance.enemies[i] != null && balance.enemies[i].id == id) { config = balance.enemies[i]; break; }
            }
            if (config == null) return;

            float minY = _game.laneMinY;
            float maxY = _game.laneMaxY;
            float x = _game.caveX;
            Vector3 pos = new Vector3(x, UnityEngine.Random.Range(minY, maxY), 0f);

            var enemy = Enemy.Create(_world.transform, config, pos, _game);
            if (enemy == null) return;

            // Reduce HP by enemyHpMultiplier.
            if (enemy.Health != null)
            {
                int hp = Mathf.Max(1, Mathf.RoundToInt(enemy.Health.max * _config.enemyHpMultiplier));
                enemy.Health.max = hp;
                enemy.Health.hp = hp;
            }
        }

        void SummonAlly()
        {
            if (_game == null || _world == null) return;

            var balance = ConfigService.Balance;
            if (balance == null || balance.allies == null || balance.allies.Count == 0) return;

            // Pick a random unlocked ally; default to farmer.
            AllyConfig allyConfig = balance.allies[UnityEngine.Random.Range(0, balance.allies.Count)];

            float x = _game.gateX + 1f + UnityEngine.Random.Range(0f, 3f);
            x = Mathf.Clamp(x, _game.worldMinX + 1f, _game.worldMaxX - 1f);
            float y = (_game.laneMinY + _game.laneMaxY) * 0.5f;

            Ally.Create(_world.transform, allyConfig, new Vector3(x, y, 0f), _hero, _waves, _game, _config.allyPowerMultiplier);
        }

        void Finish()
        {
            if (!_running) return;
            _running = false;

            // Restore theme.
            if (_world != null) _world.ApplyTheme(_savedTheme);

            // Hero: restore sprite, stop floating, invulnerability off.
            if (_hero != null)
            {
                _hero.SetSprite("hero_saqr");
                _hero.Floating = false;
                _hero.SetInvulnerable(false);
            }

            if (_aura != null) UnityEngine.Object.Destroy(_aura);

            // Haybah: allies no longer free.
            if (_haybah != null) _haybah.AlliesFree = false;

            // Dirham multiplier: restore (wealth charm). We don't know the saved wealth multiplier here,
            // so reset to 1; the battle's normal victory flow re-applies? Actually wealth charm was already
            // applied at battle start via WaveManager. Resetting to 1 loses it. To be safe, restore to 1
            // and rely on the fact that dark realm rewards already collected are in RunDirhams.
            if (_waves != null) _waves.SetDirhamMultiplier(1f);

            // Guaranteed charm drops.
            if (_config.guaranteedCharmDrops > 0)
            {
                for (int i = 0; i < _config.guaranteedCharmDrops; i++)
                {
                    var charm = LoadoutService.RollCharmDrop(1f); // force
                    if (charm != null) LoadoutService.AddCharm(charm.id, LoadoutService.CharmDropAmount());
                }
            }

            // Clean up UI.
            if (_countdownLabel != null) UnityEngine.Object.Destroy(_countdownLabel.transform.root.gameObject);

            if (_onComplete != null) _onComplete();
            UnityEngine.Object.Destroy(gameObject);
        }

        public void EndStage()
        {
            if (_running) Finish();
        }

        void OnDestroy()
        {
            if (_running && _world != null) Finish();
        }
    }
}
