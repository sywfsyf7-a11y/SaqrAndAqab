using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.Data
{
    // What the player takes into one battle: the wave, the loadout and the charm that applies to the whole wave.
    // Built once when the battle starts; the charm is consumed at that moment.
    public class BattleSetup
    {
        public int wave = 1;
        public readonly List<string> allyIds = new List<string>();
        public readonly List<string> abilityIds = new List<string>();

        public string charmId;
        public float damageMultiplier = 1f;
        public float dirhamMultiplier = 1f;
        public float charmDropBonus;

        public static BattleSetup CreateForWave(int waveNumber)
        {
            var setup = new BattleSetup();
            setup.wave = Mathf.Max(1, waveNumber);

            var balance = ConfigService.Balance;
            if (SaveService.Data == null) SaveService.Load();
            if (balance == null) return setup;

            LoadoutService.SyncUnlocks();
            LoadoutService.NormalizeLoadout();

            int allySlots = LoadoutService.SlotCount("allies");
            int abilitySlots = LoadoutService.SlotCount("abilities");

            CollectAllies(setup.allyIds, balance, allySlots);
            CollectAbilities(setup.abilityIds, balance, abilitySlots);
            ConsumeCharm(setup, balance);

            return setup;
        }

        static void CollectAllies(List<string> result, BalanceConfig balance, int slots)
        {
            if (balance.allies == null || slots <= 0) return;

            var loadout = SaveService.Data.loadoutAllies;
            AddAllies(result, loadout, balance, slots);

            // A stale or empty loadout falls back to the first unlocked allies so Start always works.
            if (result.Count == 0)
            {
                var unlocked = LoadoutService.GetUnlockedAllies();
                AddAllies(result, unlocked, balance, slots);
            }
        }

        static void AddAllies(List<string> result, List<string> ids, BalanceConfig balance, int slots)
        {
            for (int i = 0; i < ids.Count && result.Count < slots; i++)
            {
                var config = FindAlly(balance, ids[i]);
                if (config != null && !result.Contains(config.id)) result.Add(config.id);
            }
        }

        static void CollectAbilities(List<string> result, BalanceConfig balance, int slots)
        {
            if (balance.abilities == null || slots <= 0) return;

            var loadout = SaveService.Data.loadoutAbilities;
            AddAbilities(result, loadout, balance, slots);

            if (result.Count == 0)
            {
                var unlocked = LoadoutService.GetUnlockedAbilities();
                AddAbilities(result, unlocked, balance, slots);
            }
        }

        static void AddAbilities(List<string> result, List<string> ids, BalanceConfig balance, int slots)
        {
            for (int i = 0; i < ids.Count && result.Count < slots; i++)
            {
                var config = FindAbility(balance, ids[i]);
                if (config != null && !result.Contains(config.id)) result.Add(config.id);
            }
        }

        // The charm in the charm slot is used up when the wave starts.
        static void ConsumeCharm(BattleSetup setup, BalanceConfig balance)
        {
            var loadout = SaveService.Data.loadoutCharms;
            if (loadout.Count == 0 || balance.charms == null) return;

            string id = loadout[0];
            var charm = FindCharm(balance, id);
            loadout.Clear();

            if (charm == null || !LoadoutService.ConsumeCharm(id)) return;

            setup.charmId = charm.id;
            setup.damageMultiplier = charm.damageMultiplier > 0f ? charm.damageMultiplier : 1f;
            setup.dirhamMultiplier = charm.dirhamMultiplier > 0f ? charm.dirhamMultiplier : 1f;
            setup.charmDropBonus = charm.charmDropChanceBonus;

            SaveService.Save();
        }

        static AllyConfig FindAlly(BalanceConfig balance, string id)
        {
            for (int i = 0; i < balance.allies.Count; i++)
            {
                var config = balance.allies[i];
                if (config != null && config.id == id) return config;
            }

            return null;
        }

        static AbilityConfig FindAbility(BalanceConfig balance, string id)
        {
            for (int i = 0; i < balance.abilities.Count; i++)
            {
                var config = balance.abilities[i];
                if (config != null && config.id == id) return config;
            }

            return null;
        }

        static CharmConfig FindCharm(BalanceConfig balance, string id)
        {
            if (balance.charms == null) return null;

            for (int i = 0; i < balance.charms.Count; i++)
            {
                var config = balance.charms[i];
                if (config != null && config.id == id) return config;
            }

            return null;
        }
    }
}
