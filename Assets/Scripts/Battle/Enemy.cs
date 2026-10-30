using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // A jinn walking out of the cave toward the gate. Melee enemies hit whatever is in reach;
    // ranged enemies (ifrit) stop at their range and shoot a pooled fireball.
    public class Enemy : Unit
    {
        SpriteRenderer _renderer;
        bool _isRanged;
        float _projectileSpeed;
        float _gateX;
        float _baseScale = 1f;

        public int DirhamReward { get; private set; }

        public static Enemy Create(Transform parent, EnemyConfig config, Vector3 position, GameConfig game)
        {
            var go = new GameObject("Enemy " + (config != null ? config.id : "?"), typeof(Enemy));
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;

            var enemy = go.GetComponent<Enemy>();
            enemy.Setup(game, config);
            return enemy;
        }

        void Setup(GameConfig game, EnemyConfig config)
        {
            if (game != null) _gateX = game.gateX;

            if (config == null)
            {
                Debug.LogError("[Enemy] Missing enemy config.");
                return;
            }

            string spriteId = "enemy_" + config.id;
            var sprite = AssetLibrary.GetSprite(spriteId);
            float worldHeight = AssetLibrary.GetWorldHeight(spriteId);
            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            _baseScale = worldHeight / spriteHeight;

            var child = new GameObject("Sprite", typeof(SpriteRenderer), typeof(ProceduralAnim));
            child.transform.SetParent(transform, false);

            _renderer = child.GetComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.flipX = true; // enemies face left, toward the gate
            _renderer.sortingOrder = Hero.SortingOrderFor(transform.position.y);

            Anim = child.GetComponent<ProceduralAnim>();
            Anim.SetBaseScale(Vector3.one * _baseScale);
            child.transform.localScale = Vector3.one * _baseScale;

            _isRanged = config.isRanged;
            _projectileSpeed = config.projectileSpeed;
            DirhamReward = config.dirhamReward;

            var stats = new UnitStats();
            stats.hp = config.hp;
            stats.damage = config.damage;
            stats.range = config.attackRange;
            stats.cooldown = config.attackCooldown;
            stats.speed = config.moveSpeed;

            Init(Team.Enemy, stats);
        }

        void Update()
        {
            HandleAttack();

            if (Health == null || !Health.IsAlive) return;

            // Walk left toward the gate until a player unit is in range, then stop and fight.
            Unit target = UnitRegistry.GetNearest(Team.Player, transform.position.x, Range);

            if (target == null)
            {
                var position = transform.position;
                position.x = Mathf.Max(position.x - Speed * Time.deltaTime, _gateX);
                transform.position = position;
                if (Anim != null) Anim.Walk(Speed);
            }
            else if (Anim != null)
            {
                Anim.Idle();
            }

            if (_renderer != null) _renderer.sortingOrder = Hero.SortingOrderFor(transform.position.y);
        }

        protected override void Attack(Unit target)
        {
            if (!_isRanged)
            {
                base.Attack(target);
                return;
            }

            if (target == null || target.Health == null || !target.Health.IsAlive) return;

            Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, 0f);
            if (Anim != null) Anim.Lunge(direction);

            Projectile.Spawn(transform.parent, transform.position, direction, _projectileSpeed, Damage, Team);
        }
    }
}
