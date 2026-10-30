using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Stats of one combat unit. Real numbers come from balance.json (plus upgrade multipliers later).
    [System.Serializable]
    public class UnitStats
    {
        public int hp;
        public int damage;
        public float range;
        public float cooldown;
        public float speed;
    }

    // Base of every combat unit: health, stats, procedural animation and the auto-attack.
    public class Unit : MonoBehaviour
    {
        public Team Team { get; private set; }
        public Health Health { get; private set; }
        public ProceduralAnim Anim { protected get; set; }
        public int Damage { get; private set; }
        public float Range { get; private set; }
        public float Cooldown { get; private set; }
        public float Speed { get; private set; }

        public event System.Action<Unit> OnDied;

        float CooldownBase;

        void OnDisable() { UnitRegistry.Remove(this); }

        public void Init(Team team, UnitStats stats)
        {
            Team = team;
            Damage = stats.damage;
            Range = stats.range;
            Cooldown = stats.cooldown;
            CooldownBase = stats.cooldown;
            Speed = stats.speed;

            Health = new Health(stats.hp);
            Health.OnDied += HandleDied;

            // Register after the team is known, so the unit lands in the right list.
            UnitRegistry.Add(this);
        }

        void Update()
        {
            HandleAttack();
        }

        // Auto-attack the nearest opposing unit within range on cooldown.
        protected void HandleAttack()
        {
            if (Health == null || !Health.IsAlive) return;
            if (Damage <= 0) return; // units without damage (the farmer, a training dummy) never attack.

            if (Cooldown > 0f)
            {
                Cooldown -= Time.deltaTime;
                return;
            }

            Unit target = UnitRegistry.GetNearest(Team.Enemy, transform.position.x, Range);
            if (target == null) return;

            Cooldown = CooldownBase;
            Attack(target);
        }

        void Attack(Unit target)
        {
            if (target == null || target.Health == null || !target.Health.IsAlive) return;

            Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, 0f);
            if (Anim != null) Anim.Lunge(direction);

            int dealt = target.Health.TakeDamage(Damage);
            if (Anim != null) Anim.Flash();

            DamageNumbers.Show(this, dealt, false);
        }

        void HandleDied()
        {
            if (Anim != null) Anim.Die();

            UnitRegistry.Remove(this);

            var handler = OnDied;
            if (handler != null) handler(this);
        }
    }
}
