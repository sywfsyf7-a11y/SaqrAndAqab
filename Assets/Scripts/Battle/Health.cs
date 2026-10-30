using System;

namespace SaqrAndAqab.Battle
{
    // Health of a unit: current hp, max hp, damage taken, heal, damaged/died events.
    public class Health
    {
        public int hp;
        public int max;

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
