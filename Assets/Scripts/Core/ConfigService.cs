using UnityEngine;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Core
{
    // Loads and caches the balance numbers from Resources/Config/balance.json.
    public static class ConfigService
    {
        const string ResourcePath = "Config/balance";

        public static BalanceConfig Balance { get; private set; }

        public static void Load()
        {
            if (Balance != null) return;

            var textAsset = Resources.Load<TextAsset>(ResourcePath);
            if (textAsset == null)
            {
                Debug.LogError("[Config] Missing Resources/" + ResourcePath + ".json");
                Balance = new BalanceConfig();
                return;
            }

            Balance = JsonUtility.FromJson<BalanceConfig>(textAsset.text);
            if (Balance == null)
            {
                Debug.LogError("[Config] Could not parse Resources/" + ResourcePath + ".json");
                Balance = new BalanceConfig();
            }
        }

        public static void Reload()
        {
            Balance = null;
            Load();
        }
    }
}
