using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Core
{
    // Reads upgrade levels from the save and applies them when units / abilities are created.
    // Levels start at 1 (base, no bonus); buying raises the level and the effect is
    // base * (1 + bonusPerLevel * (level - 1)).
    public static class UpgradeService
    {
        public static int Level(string id)
        {
            EnsureSave();

            var entry = FindLevel(id);
            return entry != null ? Mathf.Max(1, entry.level) : 1;
        }

        public static int MaxLevel(string id)
        {
            var config = FindConfig(id);
            return config != null ? Mathf.Max(1, config.maxLevel) : 1;
        }

        public static bool IsMax(string id)
        {
            return Level(id) >= MaxLevel(id);
        }

        // Cost to go from the current level to the next one. 0 when already at max.
        public static int Cost(string id)
        {
            var config = FindConfig(id);
            if (config == null) return 0;

            int level = Level(id);
            if (level >= config.maxLevel) return 0;

            return Mathf.RoundToInt(config.baseCost * Mathf.Pow(config.costGrowth, level - 1));
        }

        // Multiplier for one upgrade id.
        public static float Multiplier(string id)
        {
            var config = FindConfig(id);
            if (config == null) return 1f;

            int level = Level(id);
            return 1f + config.bonusPerLevel * (level - 1);
        }

        // Product of several upgrade multipliers (used when one stat depends on two upgrades).
        public static float CombinedMultiplier(params string[] ids)
        {
            float result = 1f;
            if (ids == null) return result;

            for (int i = 0; i < ids.Length; i++)
            {
                result *= Multiplier(ids[i]);
            }

            return result;
        }

        // Spends dirhams and raises the level. Returns false when it cannot be bought.
        public static bool TryBuy(string id)
        {
            EnsureSave();

            var config = FindConfig(id);
            if (config == null) return false;

            int level = Level(id);
            if (level >= config.maxLevel) return false;

            int cost = Cost(id);
            if (SaveService.Data.dirhams < cost) return false;

            SaveService.Data.dirhams -= cost;
            SetLevel(id, level + 1);
            SaveService.Save();
            return true;
        }

        public static void SetLevel(string id, int level)
        {
            EnsureSave();

            var entry = FindLevel(id);
            if (entry == null)
            {
                entry = new UpgradeLevel { id = id, level = level };
                SaveService.Data.upgradeLevels.Add(entry);
            }
            else
            {
                entry.level = level;
            }
        }

        static UpgradeConfig FindConfig(string id)
        {
            var balance = ConfigService.Balance;
            if (balance == null || balance.upgrades == null || string.IsNullOrEmpty(id)) return null;

            for (int i = 0; i < balance.upgrades.Count; i++)
            {
                var upgrade = balance.upgrades[i];
                if (upgrade != null && upgrade.id == id) return upgrade;
            }

            return null;
        }

        static UpgradeLevel FindLevel(string id)
        {
            if (SaveService.Data == null || SaveService.Data.upgradeLevels == null) return null;

            var levels = SaveService.Data.upgradeLevels;
            for (int i = 0; i < levels.Count; i++)
            {
                var entry = levels[i];
                if (entry != null && entry.id == id) return entry;
            }

            return null;
        }

        static void EnsureSave()
        {
            if (SaveService.Data == null) SaveService.Load();
        }
    }
}
