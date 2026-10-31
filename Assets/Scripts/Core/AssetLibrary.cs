using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Data;
using SaqrAndAqab.Util;

namespace SaqrAndAqab.Core
{
    // The only way the game gets art. Missing PNGs become coloured placeholder sprites.
    public static class AssetLibrary
    {
        const string ArtFolder = "Art/";
        const string CatalogPath = "Config/assets";
        const float PixelsPerUnit = 100f;

        static AssetsConfig _catalog;
        static Dictionary<string, AssetEntry> _entries;
        static readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        public static AssetsConfig Catalog
        {
            get
            {
                EnsureCatalog();
                return _catalog;
            }
        }

        public static void WarmCatalog()
        {
            EnsureCatalog();
        }

        public static AssetEntry GetEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCatalog();

            AssetEntry entry;
            return _entries.TryGetValue(id, out entry) ? entry : null;
        }

        public static float GetWorldHeight(string id)
        {
            var entry = GetEntry(id);
            return entry != null ? entry.worldHeight : 1f;
        }

        public static Sprite GetSprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            Sprite cached;
            if (_sprites.TryGetValue(id, out cached)) return cached;

            Sprite sprite = Resources.Load<Sprite>(ArtFolder + id);
            if (sprite == null)
            {
                var texture = Resources.Load<Texture2D>(ArtFolder + id);
                if (texture != null)
                {
                    sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        PivotFor(id),
                        PixelsPerUnit,
                        0,
                        SpriteMeshType.FullRect);
                    sprite.name = id;
                }
            }

            if (sprite == null) sprite = CreatePlaceholder(id);

            _sprites[id] = sprite;
            return sprite;
        }

        static void EnsureCatalog()
        {
            if (_catalog != null) return;

            var textAsset = Resources.Load<TextAsset>(CatalogPath);
            if (textAsset != null) _catalog = JsonUtility.FromJson<AssetsConfig>(textAsset.text);
            if (_catalog == null)
            {
                Debug.LogError("[Assets] Missing or broken Resources/" + CatalogPath + ".json");
                _catalog = new AssetsConfig();
            }
            if (_catalog.assets == null) _catalog.assets = new List<AssetEntry>();

            _entries = new Dictionary<string, AssetEntry>();
            for (int i = 0; i < _catalog.assets.Count; i++)
            {
                var entry = _catalog.assets[i];
                if (entry == null || string.IsNullOrEmpty(entry.id)) continue;
                _entries[entry.id] = entry;
            }
        }

        static Vector2 PivotFor(string id)
        {
            var entry = GetEntry(id);
            string pivot = entry != null ? entry.pivot : null;

            if (pivot == "bottom") return new Vector2(0.5f, 0f);
            if (pivot == "top") return new Vector2(0.5f, 1f);
            return new Vector2(0.5f, 0.5f);
        }

        static Sprite CreatePlaceholder(string id)
        {
            var procSprite = ProceduralArt.GetSprite(id, PivotFor(id), PixelsPerUnit);
            if (procSprite != null) return procSprite;

            var entry = GetEntry(id);
            string category = entry != null && !string.IsNullOrEmpty(entry.category) ? entry.category : "ui";

            Color fill = CategoryColor(category);
            Color border = new Color(fill.r * 0.55f, fill.g * 0.55f, fill.b * 0.55f, 1f);
            Color highlight = Color.Lerp(fill, Color.white, 0.22f);

            bool rounded = category == "character" || category == "enemy" || category == "ally" ||
                           category == "boss" || category == "fx" || category == "story";

            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "placeholder_" + id;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[size * size];
            var clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f) / size * 2f - 1f;
                    float ny = (y + 0.5f) / size * 2f - 1f;

                    float distance = rounded
                        ? Mathf.Sqrt(nx * nx + ny * ny)
                        : Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));

                    Color pixel = clear;
                    if (distance <= 0.95f)
                    {
                        pixel = distance > 0.80f ? border : fill;
                        if (distance <= 0.35f) pixel = highlight;
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                PivotFor(id),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "placeholder_" + id;
            return sprite;
        }

        static Color CategoryColor(string category)
        {
            switch (category)
            {
                case "character": return new Color(0.90f, 0.72f, 0.30f, 1f);
                case "ally": return new Color(0.35f, 0.72f, 0.45f, 1f);
                case "enemy": return new Color(0.62f, 0.24f, 0.28f, 1f);
                case "boss": return new Color(0.45f, 0.18f, 0.55f, 1f);
                case "prop": return new Color(0.55f, 0.42f, 0.30f, 1f);
                case "background": return new Color(0.30f, 0.45f, 0.62f, 1f);
                case "fx": return new Color(0.95f, 0.85f, 0.35f, 1f);
                case "story": return new Color(0.28f, 0.55f, 0.60f, 1f);
                case "ui": return new Color(0.70f, 0.70f, 0.75f, 1f);
                default: return new Color(1f, 0f, 1f, 1f);
            }
        }
    }
}
