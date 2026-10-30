using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.Battle
{
    // Small procedural burst effects (hit sparks, death puff, coin sparkle).
    // Created on demand; no per-frame allocations beyond the burst itself.
    public static class Fx
    {
        public static void SpawnBurst(Vector3 position, Color color, int count, float speed, float life)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.4f, 0.4f);
                Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed
                    + Vector3.up * UnityEngine.Random.Range(0f, speed * 0.5f);

                var particle = FxParticle.Create(position, velocity, color, life * UnityEngine.Random.Range(0.7f, 1.1f));
            }
        }

        public static void DeathPuff(Vector3 position)
        {
            SpawnBurst(position, new Color(0.7f, 0.65f, 0.6f, 1f), 8, 2.5f, 0.5f);
        }

        public static void HitSpark(Vector3 position, Color color)
        {
            SpawnBurst(position, color, 4, 1.8f, 0.3f);
        }

        public static void CoinSparkle(Vector3 position)
        {
            SpawnBurst(position, new Color(1f, 0.9f, 0.4f, 1f), 6, 2.2f, 0.4f);
        }
    }

    public class FxParticle : MonoBehaviour
    {
        Vector3 _velocity;
        float _life;
        float _age;
        SpriteRenderer _renderer;
        Color _color;

        public static FxParticle Create(Vector3 position, Vector3 velocity, Color color, float life)
        {
            var go = new GameObject("Fx Particle", typeof(FxParticle), typeof(SpriteRenderer));
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.25f;

            var p = go.GetComponent<FxParticle>();
            p._velocity = velocity;
            p._life = Mathf.Max(0.05f, life);
            p._color = color;

            var r = go.GetComponent<SpriteRenderer>();
            p._renderer = r;
            r.sprite = AssetLibrary.GetSprite("fx_crescent"); // placeholder round-ish sprite
            r.color = color;
            r.sortingOrder = 520;

            return p;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _age += dt;

            if (_age >= _life)
            {
                Destroy(gameObject);
                return;
            }

            _velocity.y -= 6f * dt;
            transform.position += _velocity * dt;

            float alpha = 1f - (_age / _life);
            if (_renderer != null)
            {
                var c = _color;
                c.a = alpha;
                _renderer.color = c;
                transform.localScale = Vector3.one * (0.25f * (1f - _age / _life * 0.5f));
            }
        }
    }
}
