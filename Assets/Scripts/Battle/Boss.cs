using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // The boss of wave 10: a big marid that walks toward the gate, slams the ground
    // (telegraphed red circle, then area damage) and summons jinn at half health.
    public class Boss : Enemy
    {
        const float SlamRadius = 2.5f;
        const float SlamTelegraphSeconds = 1f;
        const float SlamCooldownSeconds = 3f;
        const float SummonHpFraction = 0.5f;
        const int SummonCount = 6;
        const float SummonSpread = 1.6f;

        EnemyConfig _config;
        GameConfig _game;
        float _baseSpeed;
        bool _summoned;

        bool _slamming;
        float _telegraphTimer;
        float _slamCooldown;
        GameObject _telegraph;

        public static new Boss Create(Transform parent, EnemyConfig config, Vector3 position, GameConfig game)
        {
            var go = new GameObject("Boss " + (config != null ? config.id : "?"), typeof(Boss));
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;

            var boss = go.GetComponent<Boss>();
            boss.Setup(game, config);
            return boss;
        }

        protected override void Setup(GameConfig game, EnemyConfig config)
        {
            base.Setup(game, config);

            _game = game;
            _config = config;
            _baseSpeed = config != null ? config.moveSpeed : 0f;
            _slamCooldown = 1f; // first slam shortly after spawning
        }

        void Update()
        {
            if (Health == null || !Health.IsAlive) return;

            if (_slamming)
            {
                _telegraphTimer -= Time.deltaTime;
                if (_telegraphTimer <= 0f) FinishSlam();
                UpdateSorting();
                return;
            }

            // Summon once at half health.
            if (!_summoned && Health.hp <= Mathf.RoundToInt(Health.max * SummonHpFraction))
            {
                SummonJinn();
                _summoned = true;
                Speed = Speed * 1.2f;
            }

            // Walk toward the gate.
            Unit target = UnitRegistry.GetNearest(Team.Player, transform.position.x, Range);
            if (target == null)
            {
                var position = transform.position;
                float minX = _game != null ? _game.gateX : transform.position.x;
                position.x = Mathf.Max(position.x - Speed * Time.deltaTime, minX);
                transform.position = position;
                if (Anim != null) Anim.Walk(Speed);
            }
            else if (Anim != null)
            {
                Anim.Idle();
            }

            // Periodic slam.
            _slamCooldown -= Time.deltaTime;
            if (_slamCooldown <= 0f) StartSlam();

            UpdateSorting();
        }

        void StartSlam()
        {
            _slamming = true;
            _telegraphTimer = SlamTelegraphSeconds;

            if (Anim != null) Anim.Idle();

            // Red circle on the ground showing the slam radius.
            _telegraph = new GameObject("Slam Telegraph");
            _telegraph.transform.SetParent(transform.parent, false);
            _telegraph.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            var renderer = _telegraph.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.color = new Color(0.95f, 0.20f, 0.20f, 0.35f);
            renderer.sortingOrder = -100;
            _telegraph.transform.localScale = new Vector3(SlamRadius * 2f, SlamRadius * 2f, 1f);
        }

        void FinishSlam()
        {
            _slamming = false;

            if (_telegraph != null)
            {
                UnityEngine.Object.Destroy(_telegraph);
                _telegraph = null;
            }

            int damage = _config != null ? Mathf.RoundToInt(_config.damage * 1.5f) : 0;
            Vector3 center = transform.position;

            var players = UnitRegistry.Get(Team.Player);
            for (int i = players.Count - 1; i >= 0; i--)
            {
                Unit player = players[i];
                if (player == null || player.Health == null || !player.Health.IsAlive) continue;

                float distance = Vector3.Distance(player.transform.position, center);
                if (distance > SlamRadius) continue;

                int dealt = player.Health.TakeDamage(damage);
                if (player.Anim != null) player.Anim.Flash();
                DamageNumbers.Show(player, dealt, true);
            }

            AudioService.Play("boss_slam");
            if (Anim != null) Anim.Lunge(Vector2.down);

            _slamCooldown = SlamCooldownSeconds;
        }

        void SummonJinn()
        {
            if (_game == null) return;

            var balance = ConfigService.Balance;
            if (balance == null || balance.enemies == null) return;

            EnemyConfig jinnConfig = null;
            for (int i = 0; i < balance.enemies.Count; i++)
            {
                var candidate = balance.enemies[i];
                if (candidate != null && candidate.id == "jinn_earth") { jinnConfig = candidate; break; }
            }

            if (jinnConfig == null) return;

            Transform parent = transform.parent;
            float minY = _game.laneMinY;
            float maxY = _game.laneMaxY;

            for (int i = 0; i < SummonCount; i++)
            {
                float angle = (i / (float)SummonCount) * Mathf.PI * 2f;
                float offsetX = Mathf.Cos(angle) * SummonSpread;
                float offsetY = Mathf.Sin(angle) * 0.4f;

                Vector3 position = transform.position + new Vector3(offsetX, offsetY, 0f);
                position.x = Mathf.Clamp(position.x, _game.worldMinX + 1f, _game.worldMaxX - 1f);
                position.y = Mathf.Clamp(position.y, minY, maxY);

                Enemy.Create(parent, jinnConfig, position, _game);
            }
        }

        void UpdateSorting()
        {
            if (_renderer != null) _renderer.sortingOrder = Hero.SortingOrderFor(transform.position.y);
        }

        void OnDestroy()
        {
            if (_telegraph != null) UnityEngine.Object.Destroy(_telegraph);
        }

        static Sprite CreateCircleSprite()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "boss_slam_circle";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    pixels[y * size + x] = distance <= radius
                        ? new Color(1f, 1f, 1f, 1f)
                        : new Color(0f, 0f, 0f, 0f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
