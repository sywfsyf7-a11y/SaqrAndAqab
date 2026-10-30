using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.Battle
{
    // Pooled straight-line projectile (ifrit fireball, archer arrow).
    // Hits the first live unit of the opposing team.
    public class Projectile : MonoBehaviour
    {
        const string DefaultSpriteId = "fx_fireball";
        const float HitRadius = 0.5f;
        const float HitHeight = 0.9f;
        const float LifeTime = 4f;
        const int SortingOrder = 400;

        static readonly Stack<Projectile> Pool = new Stack<Projectile>();

        SpriteRenderer _renderer;
        string _spriteId;

        Vector2 _direction = Vector2.left;
        Team _targetTeam = Team.Player;
        int _damage;
        float _speed;
        float _life;

        public static Projectile Spawn(Transform parent, Vector3 position, Vector2 direction,
            float speed, int damage, Team ownerTeam, string spriteId = DefaultSpriteId)
        {
            Projectile projectile = null;
            while (projectile == null && Pool.Count > 0) projectile = Pool.Pop();

            if (projectile == null)
            {
                var go = new GameObject("Projectile", typeof(Projectile));
                projectile = go.GetComponent<Projectile>();
            }

            projectile.transform.SetParent(parent, false);
            projectile.transform.position = position;
            projectile._direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.left;
            projectile._speed = speed;
            projectile._damage = damage;
            projectile._targetTeam = ownerTeam == Team.Player ? Team.Enemy : Team.Player;
            projectile._life = LifeTime;
            projectile.ApplySprite(spriteId);
            projectile.transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(projectile._direction.y, projectile._direction.x) * Mathf.Rad2Deg);
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        void ApplySprite(string spriteId)
        {
            if (string.IsNullOrEmpty(spriteId)) spriteId = DefaultSpriteId;

            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
                if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();
                _renderer.sortingOrder = SortingOrder;
            }

            if (_spriteId == spriteId) return;
            _spriteId = spriteId;

            var sprite = AssetLibrary.GetSprite(spriteId);
            _renderer.sprite = sprite;

            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            float scale = AssetLibrary.GetWorldHeight(spriteId) / spriteHeight;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _life -= dt;
            if (_life <= 0f)
            {
                Return();
                return;
            }

            transform.position += (Vector3)(_direction * (_speed * dt));

            Unit hit = FindHit();
            if (hit == null) return;

            int dealt = hit.Health.TakeDamage(_damage);
            DamageNumbers.Show(hit, dealt, false);
            Return();
        }

        Unit FindHit()
        {
            var list = UnitRegistry.Get(_targetTeam);
            Vector3 position = transform.position;
            Unit best = null;
            float bestDistance = HitRadius;

            for (int i = 0; i < list.Count; i++)
            {
                Unit unit = list[i];
                if (unit == null || unit.Health == null || !unit.Health.IsAlive) continue;

                float dx = Mathf.Abs(unit.transform.position.x - position.x);
                float dy = Mathf.Abs(unit.transform.position.y - position.y);
                if (dx > HitRadius || dy > HitHeight) continue;

                if (best == null || dx < bestDistance)
                {
                    best = unit;
                    bestDistance = dx;
                }
            }

            return best;
        }

        void Return()
        {
            gameObject.SetActive(false);
            Pool.Push(this);
        }
    }
}
