using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // The player hero. Moves left/right inside the world, faces the movement, auto-attacks the nearest enemy.
    public class Hero : Unit
    {
        public const string SpriteId = "hero_saqr";

        const float EdgeMargin = 0.7f;
        const float DepthScaleMax = 0.08f;

        SpriteRenderer _renderer;
        float _minX;
        float _maxX;
        float _laneMinY;
        float _laneMaxY;
        float _baseScale = 1f;
        bool _facingRight = true;

        // Abilities use this to aim in the direction the hero is facing.
        public bool IsFacingRight { get { return _facingRight; } }

        public static Hero Create(Transform parent, Vector3 position, GameConfig game, HeroConfig config)
        {
            var go = new GameObject("Hero", typeof(Hero));
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;

            var hero = go.GetComponent<Hero>();
            hero.Setup(game, config);
            return hero;
        }

        // Depth illusion: units lower on the screen are slightly bigger and drawn in front.
        public static int SortingOrderFor(float y)
        {
            return -(int)(y * 100f);
        }

        void Setup(GameConfig game, HeroConfig config)
        {
            if (game != null)
            {
                _minX = game.worldMinX + EdgeMargin;
                _maxX = game.worldMaxX - EdgeMargin;
                _laneMinY = game.laneMinY;
                _laneMaxY = game.laneMaxY;
            }

            var sprite = AssetLibrary.GetSprite(SpriteId);
            float worldHeight = AssetLibrary.GetWorldHeight(SpriteId);
            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            _baseScale = worldHeight / spriteHeight;

            var child = new GameObject("Sprite", typeof(SpriteRenderer), typeof(ProceduralAnim));
            child.transform.SetParent(transform, false);

            _renderer = child.GetComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.sortingOrder = SortingOrderFor(transform.position.y);

            Anim = child.GetComponent<ProceduralAnim>();
            Anim.SetBaseScale(Vector3.one * _baseScale);
            child.transform.localScale = Vector3.one * _baseScale;

            int baseHp = config != null ? config.hp : 0;
            int baseDamage = config != null ? config.damage : 0;

            // Upgrades are applied at creation time, read from the save (not the config).
            float heroMultiplier = UpgradeService.Multiplier("hero");
            float swordMultiplier = UpgradeService.Multiplier("sword");

            UnitStats stats = new UnitStats();
            stats.hp = Mathf.RoundToInt(baseHp * heroMultiplier);
            stats.damage = Mathf.RoundToInt(baseDamage * heroMultiplier * swordMultiplier);
            stats.range = config != null ? config.attackRange : 0f;
            stats.cooldown = config != null ? config.attackCooldown : 0f;
            stats.speed = config != null ? config.moveSpeed : 0f;

            Init(Team.Player, stats);

            ApplyDepth();
        }

        void Update()
        {
            // Auto-attack the nearest enemy in range, while still moving freely.
            HandleAttack();

            float axis = InputService.MoveAxis;

            if (Mathf.Abs(axis) > 0.01f)
            {
                float speed = Speed;
                var position = transform.position;
                position.x = Mathf.Clamp(position.x + axis * speed * Time.deltaTime, _minX, _maxX);
                transform.position = position;

                bool facingRight = axis > 0f;
                if (facingRight != _facingRight)
                {
                    _facingRight = facingRight;
                    if (_renderer != null) _renderer.flipX = !facingRight;
                }

                if (Anim != null) Anim.Walk(speed);
            }
            else if (Anim != null) Anim.Idle();

            ApplyDepth();
        }

        void ApplyDepth()
        {
            float y = transform.position.y;
            float depth = _laneMaxY > _laneMinY ? Mathf.InverseLerp(_laneMaxY, _laneMinY, y) : 0f;

            if (Anim != null) Anim.SetBaseScale(Vector3.one * (_baseScale * (1f + DepthScaleMax * depth)));
            if (_renderer != null) _renderer.sortingOrder = SortingOrderFor(y);
        }
    }
}
