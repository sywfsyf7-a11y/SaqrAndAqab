using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // The player hero. Moves left/right inside the world, faces the movement and animates procedurally.
    public class Hero : MonoBehaviour
    {
        public const string SpriteId = "hero_saqr";

        const float EdgeMargin = 0.7f;
        const float DepthScaleMax = 0.08f;

        HeroConfig _config;
        SpriteRenderer _renderer;
        ProceduralAnim _anim;

        float _minX;
        float _maxX;
        float _laneMinY;
        float _laneMaxY;
        float _baseScale = 1f;
        bool _facingRight = true;

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
            _config = config;

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

            _anim = child.GetComponent<ProceduralAnim>();
            _anim.SetBaseScale(Vector3.one * _baseScale);
            child.transform.localScale = Vector3.one * _baseScale;

            ApplyDepth();
        }

        void Update()
        {
            float axis = InputService.MoveAxis;

            if (Mathf.Abs(axis) > 0.01f)
            {
                float speed = _config != null ? _config.moveSpeed : 0f;
                var position = transform.position;
                position.x = Mathf.Clamp(position.x + axis * speed * Time.deltaTime, _minX, _maxX);
                transform.position = position;

                bool facingRight = axis > 0f;
                if (facingRight != _facingRight)
                {
                    _facingRight = facingRight;
                    if (_renderer != null) _renderer.flipX = !facingRight;
                }

                if (_anim != null) _anim.Walk(speed);
            }
            else if (_anim != null)
            {
                _anim.Idle();
            }

            ApplyDepth();
        }

        void ApplyDepth()
        {
            float y = transform.position.y;
            float depth = _laneMaxY > _laneMinY ? Mathf.InverseLerp(_laneMaxY, _laneMinY, y) : 0f;

            if (_anim != null) _anim.SetBaseScale(Vector3.one * (_baseScale * (1f + DepthScaleMax * depth)));
            if (_renderer != null) _renderer.sortingOrder = SortingOrderFor(y);
        }
    }
}
