using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // Arrow volley: fires arrowCount pooled arrows at the nearest enemies inside the ability range.
    // With no enemy in range the arrows fly forward in a small fan, so the button always gives feedback.
    public class ArrowVolley : Ability
    {
        public const string AbilityId = "arrow_volley";

        const string SpriteId = "fx_arrow";
        const float SpawnHeight = 0.6f;
        const float TargetHeight = 0.6f;
        const float NoTargetSpreadDegrees = 10f;

        static readonly List<Unit> Targets = new List<Unit>();

        public ArrowVolley(AbilityConfig config, BattleWorld world) : base(config, world) { }

        public override string IconId { get { return "ui_icon_bow"; } }

        protected override void OnUse()
        {
            float speed = Config.projectileSpeed;
            if (speed <= 0f)
            {
                Debug.LogWarning("[Ability] arrow_volley needs projectileSpeed > 0 in balance.json.");
                return;
            }

            Vector3 origin = Hero.transform.position + Vector3.up * SpawnHeight;
            bool facingRight = Hero.IsFacingRight;
            float direction = facingRight ? 1f : -1f;
            int count = Mathf.Max(1, Config.arrowCount);

            CollectEnemies(Hero.transform.position, Config.range, LaneTolerance, Targets);

            for (int i = 0; i < count; i++)
            {
                Vector2 aim;
                if (Targets.Count > 0)
                {
                    // Cycle through the nearest enemies so every arrow is used.
                    Unit target = Targets[i % Targets.Count];
                    Vector3 point = target.transform.position + Vector3.up * TargetHeight;
                    aim = (Vector2)(point - origin);
                }
                else
                {
                    float spread = count > 1
                        ? Mathf.Lerp(-NoTargetSpreadDegrees, NoTargetSpreadDegrees, i / (count - 1f))
                        : 0f;
                    aim = Quaternion.Euler(0f, 0f, spread) * new Vector2(direction, 0f);
                }

                Projectile.Spawn(World != null ? World.transform : null, origin, aim, speed, Config.damage,
                    Team.Player, SpriteId);
            }

            if (Hero.Anim != null) Hero.Anim.Lunge(new Vector2(direction, 0f));
        }
    }
}
