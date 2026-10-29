using System;
using System.Collections.Generic;

namespace SaqrAndAqab.Data
{
    // Serializable mirror of Assets/Resources/Config/assets.json.

    [Serializable]
    public class AssetEntry
    {
        public string id;
        public string category;
        public float worldHeight;
        public string pivot;
    }

    [Serializable]
    public class AssetsConfig
    {
        public List<AssetEntry> assets;
    }
}
