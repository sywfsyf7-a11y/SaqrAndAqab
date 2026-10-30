using UnityEngine;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.Battle
{
    // One dirham pickup: pops out of a dead enemy, waits, then flies to the hero when close.
    // Pooled through CoinPickups. Values and magnet rules are set by the task spec.
    public class CoinPickup : MonoBehaviour
    {
        const string SpriteId = "ui_dirham";
        const float CoinHeight = 0.45f;
        const int SortingOrder = 450;

        const float PopTime = 0.45f;
        const float PopGravity = 16f;
        const float MagnetRange = 2.5f;
        const float MagnetSpeed = 9f;
        const float CollectRange = 0.25f;
        const float AimHeight = 0.6f;
        const float LifeTime = 15f;

        Vector3 _velocity;
        float _age;
        Hero _hero;
        WaveManager _waves;

        public int Value { get; private set; }

        void Awake()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();

            var sprite = AssetLibrary.GetSprite(SpriteId);
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOrder;

            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            float scale = CoinHeight / spriteHeight;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        public void Setup(Transform parent, Vector3 position, int value, Hero hero, WaveManager waves)
        {
            transform.SetParent(parent, false);
            transform.position = position;

            Value = value;
            _hero = hero;
            _waves = waves;
            _age = 0f;
            _velocity = new Vector3(Random.Range(-1.1f, 1.1f), Random.Range(4.2f, 5.4f), 0f);
        }

        void Update()
        {
            if (_hero == null || _waves == null)
            {
                CoinPickups.Return(this);
                return;
            }

            float dt = Time.deltaTime;
            _age += dt;

            // Short pop out of the enemy, then the coin hovers until the hero is close.
            if (_age < PopTime)
            {
                _velocity.y -= PopGravity * dt;
                transform.position += _velocity * dt;
                return;
            }

            Vector3 target = _hero.transform.position + Vector3.up * AimHeight;
            float distance = Vector3.Distance(transform.position, target);

            if (distance > MagnetRange)
            {
                if (_age > LifeTime) CoinPickups.Return(this);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target, MagnetSpeed * dt);

            if (distance <= CollectRange)
            {
                _waves.AddDirhams(Value);
                CoinPickups.Return(this);
            }
        }
    }
}
