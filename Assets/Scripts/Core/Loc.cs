using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaqrAndAqab.Core
{
    // Arabic strings live in Resources/Config/strings_ar.json. Never hard-code user text in C#.
    public static class Loc
    {
        const string ResourcePath = "Config/strings_ar";

        [Serializable]
        class Entry
        {
            public string key;
            public string value;
        }

        [Serializable]
        class StringsFile
        {
            public Entry[] entries;
        }

        static Dictionary<string, string> _entries;
        static readonly HashSet<string> _reportedMissing = new HashSet<string>();

        public static void Load()
        {
            _entries = new Dictionary<string, string>();
            _reportedMissing.Clear();

            var textAsset = Resources.Load<TextAsset>(ResourcePath);
            if (textAsset == null)
            {
                Debug.LogError("[Loc] Missing Resources/" + ResourcePath + ".json");
                return;
            }

            StringsFile file = null;
            try
            {
                file = JsonUtility.FromJson<StringsFile>(textAsset.text);
            }
            catch (Exception e)
            {
                Debug.LogError("[Loc] Could not parse " + ResourcePath + ".json: " + e.Message);
            }

            if (file == null || file.entries == null) return;

            for (int i = 0; i < file.entries.Length; i++)
            {
                var entry = file.entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.key)) continue;
                _entries[entry.key] = entry.value;
            }
        }

        public static string T(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_entries == null) Load();

            string value;
            if (_entries != null && _entries.TryGetValue(key, out value)) return value;

            if (_reportedMissing.Add(key))
                Debug.LogError("[Loc] Missing string key: " + key);

            return key;
        }

        public static string T(string key, params object[] args)
        {
            string format = T(key);
            if (args == null || args.Length == 0) return format;
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException e)
            {
                Debug.LogError("[Loc] Bad format for key " + key + ": " + e.Message);
                return format;
            }
        }
    }
}
