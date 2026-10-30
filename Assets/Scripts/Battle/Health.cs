using System;

namespace SaqrAndAqab.Battle
{
    // Health of a unit: current hp, max hp, damage taken, heal, damaged/died events.
    public class Health
    {
        public int hp;
        public int max;

        // When true, TakeDamage always returns 0 (used by the Dark Realm).
        public bool Invulnerable;

        public event Action<int> OnDamaged;
        public event Action OnDied;

        public Health(int max)
        {
            this.max = max;
            hp = max;
        }

        public bool IsAlive
        {
            get { return hp > 0; }
        }

        public int TakeDamage(int damage)
        {
            if (Invulnerable) return 0;
            if (hp <= 0) return 0;

            int dealt = damage > hp ? hp : damage;
            hp -= dealt;

            if (OnDamaged != null) OnDamaged(dealt);

            if (hp <= 0)
            {
                if (OnDied != null) OnDied();
            }

            return dealt;
        }

        public void Heal(int amount)
        {
            if (hp <= 0) return;

            hp += amount;
            if (hp > max) hp = max;
        }
    }
}
