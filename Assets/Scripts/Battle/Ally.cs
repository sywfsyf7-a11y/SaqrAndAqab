using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // A player-side helper unit: the farmer produces dirhams near the gate,
    // the archer follows the hero and shoots pooled arrows at enemies in range.
    public class Ally : Unit
    {
        public const string FarmerId = "farmer";
        public const string ArcherId = "archer";

        // Temporary selection until the setup screen (task 09) picks the loadout.
        public static readonly string[] DefaultIds = { FarmerId, ArcherId };

        const string ArrowSpriteId = "fx_arrow";
        const float FollowOffset = 1.2f;
        const float GateGap = 0.5f;
        const float FollowMaxFromGate = 10f;
        const float SpawnHeight = 0.6f;

        AllyConfig _config;
        Hero _hero;
        WaveManager _waves;
        SpriteRenderer _renderer;
        float _gateX;
        float _incomeTimer;
        bool _isFarmer;

        public static Ally Create(Transform parent, AllyConfig config, Vector3 position,
            Hero hero, WaveManager waves, GameConfig game)
        {
            var go = new GameObject("Ally " + (config != null ? config.id : "?"), typeof(Ally));
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;

            var ally = go.GetComponent<Ally>();
            ally.Setup(config, hero, waves, game);
            return ally;
        }

        void Setup(AllyConfig config, Hero hero, WaveManager waves, GameConfig game)
        {
            _config = config;
            _hero = hero;
            _waves = waves;
            _gateX = game != null ? game.gateX : 0f;

            if (config == null)
            {
                Debug.LogError("[Ally] Missing ally config.");
                return;
            }

            _isFarmer = config.id == FarmerId;

            string spriteId = "ally_" + config.id;
            var sprite = AssetLibrary.GetSprite(spriteId);
            float worldHeight = AssetLibrary.GetWorldHeight(spriteId);
            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            float scale = worldHeight / spriteHeight;

            var child = new GameObject("Sprite", typeof(SpriteRenderer), typeof(ProceduralAnim));
            child.transform.SetParent(transform, false);

            _renderer = child.GetComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.sortingOrder = Hero.SortingOrderFor(transform.position.y);

            Anim = child.GetComponent<ProceduralAnim>();
            Anim.SetBaseScale(Vector3.one * scale);
            child.transform.localScale = Vector3.one * scale;

            var stats = new UnitStats();
            stats.hp = config.hp;
            stats.damage = config.damage;
            stats.range = config.attackRange;
            stats.cooldown = config.attackCooldown;
            stats.speed = config.moveSpeed;

            Init(Team.Player, stats);

            _incomeTimer = config.incomeInterval;
        }

        void Update()
        {
            if (Health == null || !Health.IsAlive) return;

            if (_isFarmer) HandleIncome();
            else FollowHero();

            HandleAttack();

            if (_renderer != null) _renderer.sortingOrder = Hero.SortingOrderFor(transform.position.y);
        }

        // Farmer: no attack, drops a dirham coin every incomeInterval seconds.
        void HandleIncome()
        {
            if (Anim != null) Anim.Idle();
            if (_config == null || _config.incomeInterval <= 0f || _config.incomeDirhams <= 0) return;

            _incomeTimer -= Time.deltaTime;
            if (_incomeTimer > 0f) return;

            _incomeTimer = _config.incomeInterval;
            CoinPickups.Spawn(transform.parent, transform.position + Vector3.up * SpawnHeight,
                _config.incomeDirhams, _hero, _waves, true);
        }

        // Archer: stays behind the hero, never further right than gateX + 10.
        void FollowHero()
        {
            float targetX = _hero != null ? _hero.transform.position.x - FollowOffset : transform.position.x;
            targetX = Mathf.Clamp(targetX, _gateX + GateGap, _gateX + FollowMaxFromGate);

            float dx = targetX - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                float step = Mathf.Min(Mathf.Abs(dx), Speed * Time.deltaTime);
                var position = transform.position;
                position.x += Mathf.Sign(dx) * step;
                transform.position = position;
                if (Anim != null) Anim.Walk(Speed);
            }
            else if (Anim != null)
            {
                Anim.Idle();
            }
        }

        protected override void Attack(Unit target)
        {
            if (target == null || target.Health == null || !target.Health.IsAlive) return;
            if (_config == null) return;

            Vector2 direction = (Vector2)(target.transform.position - transform.position);
            if (Anim != null) Anim.Lunge(direction);

            Projectile.Spawn(transform.parent, transform.position + Vector3.up * SpawnHeight,
                direction, _config.projectileSpeed, Damage, Team, ArrowSpriteId);
        }
    }
}
