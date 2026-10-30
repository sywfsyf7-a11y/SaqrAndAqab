using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaqrAndAqab.Core
{
    [Serializable]
    public class UpgradeLevel
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class CharmCount
    {
        public string id;
        public int count;
    }

    [Serializable]
    public class SaveData
    {
        public int dirhams;
        public List<UpgradeLevel> upgradeLevels = new List<UpgradeLevel>();
        public int highestWaveCleared;
        public List<string> unlockedAllies = new List<string>();
        public List<string> unlockedAbilities = new List<string>();
        public List<CharmCount> charmCounts = new List<CharmCount>();
        public int dateCount;
        public bool datesInitialized;
        public List<string> loadoutAllies = new List<string>();
        public List<string> loadoutAbilities = new List<string>();
        public List<string> loadoutCharms = new List<string>();
        public bool introSeen;
        public bool chapterComplete;
        public bool tutorialProtectShown;
        public bool tutorialMoveShown;
        public bool tutorialHaybahShown;
        public bool tutorialSlashShown;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
    }

    // One JSON file in Application.persistentDataPath. Must never crash on a missing or broken file.
    public static class SaveService
    {
        const string FileName = "save.json";

        public static SaveData Data { get; private set; }

        static string FilePath
        {
            get { return Path.Combine(Application.persistentDataPath, FileName); }
        }

        public static void Load()
        {
            SaveData data = null;
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    if (!string.IsNullOrEmpty(json)) data = JsonUtility.FromJson<SaveData>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] Could not read save file, starting fresh: " + e.Message);
                data = null;
            }

            Data = data ?? new SaveData();
            EnsureLists();
        }

        public static void Save()
        {
            if (Data == null) Data = new SaveData();
            EnsureLists();

            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] Could not write save file: " + e.Message);
            }
        }

        public static void Reset()
        {
            Data = new SaveData();
            EnsureLists();

            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] Could not delete save file: " + e.Message);
            }

            Save();
        }

        static void EnsureLists()
        {
            if (Data.upgradeLevels == null) Data.upgradeLevels = new List<UpgradeLevel>();
            if (Data.unlockedAllies == null) Data.unlockedAllies = new List<string>();
            if (Data.unlockedAbilities == null) Data.unlockedAbilities = new List<string>();
            if (Data.charmCounts == null) Data.charmCounts = new List<CharmCount>();
            if (Data.loadoutAllies == null) Data.loadoutAllies = new List<string>();
            if (Data.loadoutAbilities == null) Data.loadoutAbilities = new List<string>();
            if (Data.loadoutCharms == null) Data.loadoutCharms = new List<string>();
        }
    }
}
