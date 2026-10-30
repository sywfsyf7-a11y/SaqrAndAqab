using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // Base of every player ability: an id, a cooldown and a TryUse gate.
    // Concrete abilities live in Assets/Scripts/Battle/Abilities and read their numbers from balance.json.
    public abstract class Ability
    {
        // Temporary loadout until the setup screen (task 09) picks the player's abilities.
        public static readonly string[] DefaultIds = { CrescentSlash.AbilityId, ArrowVolley.AbilityId };

        protected readonly AbilityConfig Config;
        protected readonly BattleWorld World;
        protected readonly Hero Hero;

        float _cooldownLeft;

        protected Ability(AbilityConfig config, BattleWorld world)
        {
            Config = config;
            World = world;
            Hero = world != null ? world.Hero : null;
        }

        public string Id { get { return Config != null ? Config.id : string.Empty; } }

        // UI icon id from assets.json.
        public abstract string IconId { get; }

        public float Cooldown { get { return Config != null ? Config.cooldown : 0f; } }

        public bool IsReady { get { return _cooldownLeft <= 0f; } }

        // 1 = just used, 0 = ready. Drives the circular cooldown fill.
        public float CooldownFraction
        {
            get
            {
                float cooldown = Cooldown;
                if (cooldown <= 0f) return 0f;
                return Mathf.Clamp01(_cooldownLeft / cooldown);
            }
        }

        public static Ability Create(AbilityConfig config, BattleWorld world)
        {
            if (config == null) return null;

            switch (config.id)
            {
                case CrescentSlash.AbilityId: return new CrescentSlash(config, world);
                case ArrowVolley.AbilityId: return new ArrowVolley(config, world);
                default:
                    Debug.LogWarning("[Ability] Unknown ability id: " + config.id);
                    return null;
            }
        }

        // UI icon id from assets.json, for the setup screen (no world needed).
        public static string IconFor(string id)
        {
            switch (id)
            {
                case CrescentSlash.AbilityId: return "ui_icon_slash";
                case ArrowVolley.AbilityId: return "ui_icon_bow";
                default: return null;
            }
        }

        // Spends the cooldown and runs the effect. False when the ability is not usable yet.
        public bool TryUse()
        {
            if (Config == null || Hero == null || Hero.Health == null || !Hero.Health.IsAlive) return false;
            if (!IsReady) return false;
            // Never fire while the game is paused or frozen by a hit-stop.
            if (Time.timeScale <= 0f) return false;

            OnUse();
            _cooldownLeft = Cooldown;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_cooldownLeft <= 0f) return;

            _cooldownLeft -= deltaTime;
            if (_cooldownLeft < 0f) _cooldownLeft = 0f;
        }

        protected abstract void OnUse();

        // The charm of power and the matching upgrade multiply the ability's damage.
        protected int ScaledDamage
        {
            get
            {
                float multiplier = Hero != null ? Hero.DamageMultiplier : 1f;

                string upgradeId = null;
                if (Id == CrescentSlash.AbilityId) upgradeId = "slash";
                else if (Id == ArrowVolley.AbilityId) upgradeId = "bow";

                if (upgradeId != null) multiplier *= UpgradeService.Multiplier(upgradeId);

                return Mathf.Max(0, Mathf.RoundToInt(Config.damage * multiplier));
            }
        }

        // Alive enemies within range of the hero on X, sorted nearest first.
        protected static void CollectEnemies(Vector3 origin, float range, float laneTolerance,
            System.Collections.Generic.List<Unit> buffer)
        {
            buffer.Clear();

            var enemies = UnitRegistry.Get(Team.Enemy);
            for (int i = 0; i < enemies.Count; i++)
            {
                Unit enemy = enemies[i];
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive) continue;
                if (Mathf.Abs(enemy.transform.position.y - origin.y) > laneTolerance) continue;

                float distance = Mathf.Abs(enemy.transform.position.x - origin.x);
                if (distance > range) continue;

                int insert = buffer.Count;
                for (int j = 0; j < buffer.Count; j++)
                {
                    if (distance < Mathf.Abs(buffer[j].transform.position.x - origin.x))
                    {
                        insert = j;
                        break;
                    }
                }

                buffer.Insert(insert, enemy);
            }
        }

        // The lane is a thin band, so anything inside its height counts as the same row.
        protected float LaneTolerance
        {
            get
            {
                var game = World != null ? World.Game : null;
                return game != null ? Mathf.Abs(game.laneMaxY - game.laneMinY) : 0f;
            }
        }
    }
}
