using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // Crescent slash: instantly cuts every enemy in front of the hero inside the ability range.
    // Visual: the crescent sprite grows and fades, the camera shakes and the world freezes for 40 ms.
    public class CrescentSlash : Ability
    {
        public const string AbilityId = "crescent_slash";

        const float HitStopSeconds = 0.04f;
        const float ShakeAmplitude = 0.18f;
        const float ShakeDuration = 0.22f;

        public CrescentSlash(AbilityConfig config, BattleWorld world) : base(config, world) { }

        public override string IconId { get { return "ui_icon_slash"; } }

        protected override void OnUse()
        {
            AudioService.Play("slash");
            bool facingRight = Hero.IsFacingRight;
            float direction = facingRight ? 1f : -1f;
            Vector3 origin = Hero.transform.position;
            float range = Config.range;
            float tolerance = LaneTolerance;

            var enemies = UnitRegistry.Get(Team.Enemy);
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Unit enemy = enemies[i];
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive) continue;

                // Only units in front of the hero and inside the slash range are hit.
                float dx = enemy.transform.position.x - origin.x;
                if (dx * direction < 0f || Mathf.Abs(dx) > range) continue;
                if (Mathf.Abs(enemy.transform.position.y - origin.y) > tolerance) continue;

                int dealt = enemy.Health.TakeDamage(ScaledDamage);
                DamageNumbers.Show(enemy, dealt, false);

                if (enemy.Anim != null)
                {
                    enemy.Anim.Flash();
                    enemy.Anim.Knockback(new Vector2(direction, 0f));
                }
            }

            if (Hero.Anim != null) Hero.Anim.Lunge(new Vector2(direction, 0f));

            CrescentFx.Play(World != null ? World.transform : null, origin, facingRight, range);
            if (World != null && World.Shake != null) World.Shake.Play(ShakeAmplitude, ShakeDuration);

            HitStop.Play(HitStopSeconds);
        }
    }
}
