using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Core
{
    // Save-side helpers for unlocks, the chosen loadout and the charm inventory.
    public static class LoadoutService
    {
        // Adds the starting unlocks and everything the player has already earned.
        public static void SyncUnlocks()
        {
            EnsureSave();

            var balance = ConfigService.Balance;
            if (balance == null) return;

            var start = balance.startingUnlocks;
            if (start != null)
            {
                if (start.allies != null)
                {
                    for (int i = 0; i < start.allies.Length; i++) AddUnlockedAlly(start.allies[i]);
                }

                if (start.abilities != null)
                {
                    for (int i = 0; i < start.abilities.Length; i++) AddUnlockedAbility(start.abilities[i]);
                }
            }

            ApplyUnlocks(SaveService.Data.highestWaveCleared);
        }

        // Applies every unlock earned up to clearedWave and returns the ones that are new.
        public static List<UnlockConfig> ApplyUnlocks(int clearedWave)
        {
            var newlyUnlocked = new List<UnlockConfig>();

            EnsureSave();

            var balance = ConfigService.Balance;
            if (balance == null || balance.unlocks == null) return newlyUnlocked;

            for (int i = 0; i < balance.unlocks.Count; i++)
            {
                var unlock = balance.unlocks[i];
                if (unlock == null || unlock.afterWave > clearedWave) continue;

                bool added = false;
                if (unlock.type == "ally") added = AddUnlockedAlly(unlock.id);
                else if (unlock.type == "ability") added = AddUnlockedAbility(unlock.id);

                if (added) newlyUnlocked.Add(unlock);
            }

            return newlyUnlocked;
        }

        public static bool IsAllyUnlocked(string id)
        {
            EnsureSave();
            return !string.IsNullOrEmpty(id) && SaveService.Data.unlockedAllies.Contains(id);
        }

        public static bool IsAbilityUnlocked(string id)
        {
            EnsureSave();
            return !string.IsNullOrEmpty(id) && SaveService.Data.unlockedAbilities.Contains(id);
        }

        public static List<string> GetUnlockedAllies()
        {
            var result = new List<string>();
            EnsureSave();

            var balance = ConfigService.Balance;
            if (balance == null || balance.allies == null) return result;

            for (int i = 0; i < balance.allies.Count; i++)
            {
                var ally = balance.allies[i];
                if (ally != null && IsAllyUnlocked(ally.id)) result.Add(ally.id);
            }

            return result;
        }

        public static List<string> GetUnlockedAbilities()
        {
            var result = new List<string>();
            EnsureSave();

            var balance = ConfigService.Balance;
            if (balance == null || balance.abilities == null) return result;

            for (int i = 0; i < balance.abilities.Count; i++)
            {
                var ability = balance.abilities[i];
                if (ability != null && IsAbilityUnlocked(ability.id)) result.Add(ability.id);
            }

            return result;
        }

        // Charms are owned through drops, so only charms with a count above zero are shown.
        public static List<string> GetOwnedCharms()
        {
            var result = new List<string>();
            EnsureSave();

            var balance = ConfigService.Balance;
            if (balance == null || balance.charms == null) return result;

            for (int i = 0; i < balance.charms.Count; i++)
            {
                var charm = balance.charms[i];
                if (charm != null && GetCharmCount(charm.id) > 0) result.Add(charm.id);
            }

            return result;
        }

        public static int GetCharmCount(string id)
        {
            EnsureSave();

            var entry = FindCharmCount(id);
            return entry != null ? entry.count : 0;
        }

        public static void AddCharm(string id, int amount)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureSave();

            var entry = FindCharmCount(id);
            if (entry == null)
            {
                entry = new CharmCount { id = id, count = 0 };
                SaveService.Data.charmCounts.Add(entry);
            }

            entry.count += Mathf.Max(1, amount);
        }

        // One charm is used up when the wave starts.
        public static bool ConsumeCharm(string id)
        {
            EnsureSave();

            var entry = FindCharmCount(id);
            if (entry == null || entry.count <= 0) return false;

            entry.count--;
            return true;
        }

        public static int CharmDropAmount()
        {
            var balance = ConfigService.Balance;
            var drop = balance != null ? balance.charmDrop : null;
            return drop != null ? Mathf.Max(1, drop.startCountEach) : 1;
        }

        // Rolls the victory charm drop. bonusChance comes from the luck charm used this wave.
        public static CharmConfig RollCharmDrop(float bonusChance)
        {
            var balance = ConfigService.Balance;
            if (balance == null || balance.charms == null || balance.charms.Count == 0) return null;

            float baseChance = balance.charmDrop != null ? balance.charmDrop.baseChance : 0f;
            float chance = Mathf.Clamp01(baseChance + bonusChance);
            if (Random.value >= chance) return null;

            var charm = balance.charms[Random.Range(0, balance.charms.Count)];
            return charm;
        }

        // Keeps the saved loadout inside the unlocked items and the slot limits,
        // and fills empty ally / ability slots with the first unlocked items.
        public static void NormalizeLoadout()
        {
            EnsureSave();

            var data = SaveService.Data;
            int allySlots = SlotCount("allies");
            int abilitySlots = SlotCount("abilities");
            int charmSlots = SlotCount("charms");

            TrimToUnlocked(data.loadoutAllies, data.unlockedAllies, allySlots);
            if (data.loadoutAllies.Count == 0)
                FillFirst(data.loadoutAllies, GetUnlockedAllies(), allySlots);

            TrimToUnlocked(data.loadoutAbilities, data.unlockedAbilities, abilitySlots);
            if (data.loadoutAbilities.Count == 0)
                FillFirst(data.loadoutAbilities, GetUnlockedAbilities(), abilitySlots);

            for (int i = data.loadoutCharms.Count - 1; i >= 0; i--)
            {
                string id = data.loadoutCharms[i];
                if (i >= charmSlots || GetCharmCount(id) <= 0) data.loadoutCharms.RemoveAt(i);
            }

            SaveService.Save();
        }

        public static int SlotCount(string tab)
        {
            var balance = ConfigService.Balance;
            var slots = balance != null ? balance.slots : null;
            if (slots == null) return 0;

            if (tab == "allies") return Mathf.Max(0, slots.allies);
            if (tab == "abilities") return Mathf.Max(0, slots.abilities);
            if (tab == "charms") return Mathf.Max(0, slots.charms);
            return 0;
        }

        static void EnsureSave()
        {
            if (SaveService.Data == null) SaveService.Load();
        }

        static bool AddUnlockedAlly(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            EnsureSave();
            if (SaveService.Data.unlockedAllies.Contains(id)) return false;

            SaveService.Data.unlockedAllies.Add(id);
            return true;
        }

        static bool AddUnlockedAbility(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            EnsureSave();
            if (SaveService.Data.unlockedAbilities.Contains(id)) return false;

            SaveService.Data.unlockedAbilities.Add(id);
            return true;
        }

        static CharmCount FindCharmCount(string id)
        {
            if (string.IsNullOrEmpty(id) || SaveService.Data == null) return null;

            var charms = SaveService.Data.charmCounts;
            for (int i = 0; i < charms.Count; i++)
            {
                var entry = charms[i];
                if (entry != null && entry.id == id) return entry;
            }

            return null;
        }

        static void TrimToUnlocked(List<string> loadout, List<string> unlocked, int slots)
        {
            for (int i = loadout.Count - 1; i >= 0; i--)
            {
                string id = loadout[i];
                if (i >= slots || !unlocked.Contains(id)) loadout.RemoveAt(i);
            }
        }

        static void FillFirst(List<string> loadout, List<string> available, int slots)
        {
            for (int i = 0; i < available.Count && loadout.Count < slots; i++)
            {
                string id = available[i];
                if (!loadout.Contains(id)) loadout.Add(id);
            }
        }
    }
}
