using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaqrAndAqab.Util
{
    // Generates procedural fallback art for skies, parallax dunes/mountains, grounds,
    // props (gate, cave) and UI icons when real PNG assets are missing.
    // Real art in Resources/Art always takes precedence.
    public static class ProceduralArt
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite GetSprite(string id, Vector2 pivot, float ppu = 100f)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (Cache.TryGetValue(id, out var cached) && cached != null)
            {
                return cached;
            }

            Sprite sprite = null;

            if (id.StartsWith("bg_") && id.EndsWith("_sky"))
            {
                string theme = id.Substring(3, id.Length - 7); // e.g. "day", "night", "dark", "barzakh", "fort", "tower"
                sprite = CreateSkySprite(id, theme, pivot, ppu);
            }
            else if (id.StartsWith("bg_") && id.EndsWith("_far"))
            {
                string theme = id.Substring(3, id.Length - 7);
                sprite = CreateFarSprite(id, theme, pivot, ppu);
            }
            else if (id.StartsWith("ground_") || (id.StartsWith("bg_") && id.EndsWith("_ground")))
            {
                string theme = id.Replace("ground_", "").Replace("bg_", "").Replace("_ground", "");
                sprite = CreateGroundSprite(id, theme, pivot, ppu);
            }
            else if (id == "prop_gate")
            {
                sprite = CreateGateSprite(id, pivot, ppu);
            }
            else if (id == "prop_cave")
            {
                sprite = CreateCaveSprite(id, pivot, ppu);
            }
            else if (id.StartsWith("ui_"))
            {
                sprite = CreateIconSprite(id, pivot, ppu);
            }
            else
            {
                sprite = CreateFallbackBadge(id, pivot, ppu);
            }

            if (sprite != null)
            {
                sprite.name = "proc_" + id;
                Cache[id] = sprite;
            }

            return sprite;
        }

        // ====================================================================
        // 1. SKIES: Gradients, Stars, Ash Specks, Swirls
        // ====================================================================

        static Sprite CreateSkySprite(string id, string theme, Vector2 pivot, float ppu)
        {
            const int w = 512;
            const int h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[w * h];

            // Theme color stops (bottom to top)
            Color cBottom, cMid, cTop;
            bool addStars = false;
            bool addAsh = false;
            bool addSwirl = false;

            switch (theme.ToLowerInvariant())
            {
                case "night":
                    cBottom = new Color(0.08f, 0.12f, 0.22f); // Deep indigo
                    cMid = new Color(0.04f, 0.06f, 0.14f);
                    cTop = new Color(0.02f, 0.03f, 0.08f);
                    addStars = true;
                    break;
                case "dark":
                case "darkrealm":
                    cBottom = new Color(0.12f, 0.05f, 0.18f); // Purple-black
                    cMid = new Color(0.07f, 0.03f, 0.12f);
                    cTop = new Color(0.03f, 0.01f, 0.06f);
                    addSwirl = true;
                    break;
                case "barzakh":
                    cBottom = new Color(0.60f, 0.58f, 0.56f); // Pale dusty grey
                    cMid = new Color(0.42f, 0.40f, 0.40f);
                    cTop = new Color(0.24f, 0.23f, 0.25f);
                    addAsh = true;
                    break;
                case "fort":
                    cBottom = new Color(0.92f, 0.48f, 0.18f); // Orange dusk
                    cMid = new Color(0.65f, 0.25f, 0.35f);
                    cTop = new Color(0.18f, 0.12f, 0.28f);
                    break;
                case "tower":
                    cBottom = new Color(0.22f, 0.30f, 0.38f); // Stormy blue
                    cMid = new Color(0.14f, 0.18f, 0.26f);
                    cTop = new Color(0.08f, 0.10f, 0.16f);
                    break;
                case "day":
                default:
                    cBottom = new Color(0.98f, 0.82f, 0.52f); // Gold
                    cMid = new Color(0.95f, 0.65f, 0.48f);    // Peach
                    cTop = new Color(0.35f, 0.65f, 0.88f);    // Light sky blue
                    break;
            }

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                Color c = t < 0.5f ? Color.Lerp(cBottom, cMid, t * 2f) : Color.Lerp(cMid, cTop, (t - 0.5f) * 2f);

                for (int x = 0; x < w; x++)
                {
                    pixels[y * w + x] = c;
                }
            }

            if (addStars)
            {
                // Crescent moon
                int mx = (int)(w * 0.78f);
                int my = (int)(h * 0.72f);
                int mr = 18;
                for (int dy = -mr; dy <= mr; dy++)
                {
                    for (int dx = -mr; dx <= mr; dx++)
                    {
                        float d1 = Mathf.Sqrt(dx * dx + dy * dy);
                        float d2 = Mathf.Sqrt((dx - 6) * (dx - 6) + (dy - 4) * (dy - 4));
                        if (d1 <= mr && d2 > mr * 0.82f)
                        {
                            int px = mx + dx;
                            int py = my + dy;
                            if (px >= 0 && px < w && py >= 0 && py < h)
                            {
                                pixels[py * w + px] = new Color(0.95f, 0.95f, 0.88f, 0.95f);
                            }
                        }
                    }
                }

                // Deterministic starry sky
                var rand = new System.Random(777);
                for (int s = 0; s < 95; s++)
                {
                    int sx = rand.Next(0, w);
                    int sy = rand.Next((int)(h * 0.35f), h);
                    float brightness = (float)rand.NextDouble() * 0.6f + 0.4f;
                    pixels[sy * w + sx] = new Color(1f, 1f, 1f, brightness);
                }
            }

            if (addAsh)
            {
                var rand = new System.Random(1337);
                for (int a = 0; a < 140; a++)
                {
                    int ax = rand.Next(0, w);
                    int ay = rand.Next(0, h);
                    byte v = (byte)rand.Next(30, 80);
                    pixels[ay * w + ax] = new Color32(v, v, v, 210);
                }
            }

            if (addSwirl)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float swirl = Mathf.Sin(x * 0.04f + Mathf.Cos(y * 0.05f) * 2f);
                        if (swirl > 0.6f)
                        {
                            var p = pixels[y * w + x];
                            pixels[y * w + x] = Color.Lerp(p, new Color(0.18f, 0.28f, 0.48f, 1f), (swirl - 0.6f) * 0.5f);
                        }
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
        }

        // ====================================================================
        // 2. FAR LAYER: Layered Silhouettes, Dunes, Domes & Minarets
        // ====================================================================

        static Sprite CreateFarSprite(string id, string theme, Vector2 pivot, float ppu)
        {
            const int w = 512;
            const int h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[w * h];

            Color duneColor1, duneColor2;
            bool isTower = theme.IndexOf("tower", StringComparison.OrdinalIgnoreCase) >= 0;

            if (theme.IndexOf("night", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                duneColor1 = new Color(0.10f, 0.14f, 0.26f, 0.70f);
                duneColor2 = new Color(0.06f, 0.09f, 0.18f, 0.95f);
            }
            else if (theme.IndexOf("dark", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                duneColor1 = new Color(0.18f, 0.08f, 0.22f, 0.70f);
                duneColor2 = new Color(0.10f, 0.04f, 0.14f, 0.95f);
            }
            else if (theme.IndexOf("fort", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                duneColor1 = new Color(0.55f, 0.28f, 0.22f, 0.75f);
                duneColor2 = new Color(0.38f, 0.18f, 0.16f, 0.95f);
            }
            else if (isTower)
            {
                duneColor1 = new Color(0.20f, 0.26f, 0.32f, 0.75f);
                duneColor2 = new Color(0.12f, 0.16f, 0.22f, 0.95f);
            }
            else
            {
                // Day sand dunes
                duneColor1 = new Color(0.85f, 0.62f, 0.38f, 0.72f);
                duneColor2 = new Color(0.72f, 0.48f, 0.26f, 0.95f);
            }

            // Layer 1 (Far dunes/peaks)
            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / w * Mathf.PI * 4f;
                float heightRatio = 0.42f + Mathf.Sin(nx) * 0.08f + Mathf.Cos(nx * 2.3f) * 0.04f;
                int duneHeight = (int)(h * heightRatio);

                for (int y = 0; y < duneHeight; y++)
                {
                    pixels[y * w + x] = duneColor1;
                }
            }

            // Architectural silhouettes on layer 1 (Domes & Minarets or Tower)
            if (isTower)
            {
                // Jagged tower peak
                int tx = (int)(w * 0.45f);
                int tw = 24;
                for (int y = (int)(h * 0.45f); y < (int)(h * 0.88f); y++)
                {
                    int span = (int)(tw * (1f - (y - h * 0.45f) / (h * 0.43f) * 0.6f));
                    for (int dx = -span; dx <= span; dx++)
                    {
                        int px = tx + dx;
                        if (px >= 0 && px < w)
                        {
                            pixels[y * w + px] = new Color(0.18f, 0.22f, 0.32f, 0.96f);
                        }
                    }
                }
                // Glowing beacon atop tower
                for (int dy = -4; dy <= 4; dy++)
                {
                    for (int dx = -4; dx <= 4; dx++)
                    {
                        if (dx * dx + dy * dy <= 16)
                        {
                            int px = tx + dx;
                            int py = (int)(h * 0.88f) + dy;
                            if (px >= 0 && px < w && py >= 0 && py < h)
                                pixels[py * w + px] = new Color(0.2f, 0.85f, 1f, 1f);
                        }
                    }
                }
            }
            else
            {
                // Arabic Minarets and Domes along the horizon
                DrawMinaret(pixels, w, h, (int)(w * 0.22f), (int)(h * 0.46f), 10, (int)(h * 0.32f), duneColor1);
                DrawDome(pixels, w, h, (int)(w * 0.30f), (int)(h * 0.46f), 24, duneColor1);
                DrawMinaret(pixels, w, h, (int)(w * 0.68f), (int)(h * 0.44f), 9, (int)(h * 0.28f), duneColor1);
                DrawDome(pixels, w, h, (int)(w * 0.75f), (int)(h * 0.44f), 20, duneColor1);
            }

            // Layer 2 (Closer front dunes)
            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / w * Mathf.PI * 4f + 1.2f;
                float heightRatio = 0.28f + Mathf.Sin(nx) * 0.09f + Mathf.Sin(nx * 3.1f) * 0.03f;
                int duneHeight = (int)(h * heightRatio);

                for (int y = 0; y < duneHeight; y++)
                {
                    pixels[y * w + x] = duneColor2;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
        }

        static void DrawMinaret(Color32[] pixels, int w, int h, int cx, int baseY, int width, int height, Color color)
        {
            int topY = Mathf.Min(baseY + height, h - 1);
            for (int y = baseY; y < topY; y++)
            {
                for (int dx = -width / 2; dx <= width / 2; dx++)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < w && y < h)
                    {
                        pixels[y * w + px] = color;
                    }
                }
            }
            // Spire
            for (int dy = 0; dy < 16; dy++)
            {
                int span = Mathf.Max(0, (width / 2) - dy);
                int y = topY + dy;
                if (y >= h) break;
                for (int dx = -span; dx <= span; dx++)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < w) pixels[y * w + px] = color;
                }
            }
        }

        static void DrawDome(Color32[] pixels, int w, int h, int cx, int baseY, int radius, Color color)
        {
            for (int dy = 0; dy <= radius; dy++)
            {
                int y = baseY + dy;
                if (y >= h) break;
                int span = (int)Mathf.Sqrt(radius * radius - dy * dy);
                for (int dx = -span; dx <= span; dx++)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < w) pixels[y * w + px] = color;
                }
            }
            // Crescent finial on top of dome
            int fy = baseY + radius + 4;
            if (fy < h && cx >= 0 && cx < w) pixels[fy * w + cx] = color;
        }

        // ====================================================================
        // 3. GROUND: Noisy Sand Strip, Wavy Top, Rocks & Grass Tufts
        // ====================================================================

        static Sprite CreateGroundSprite(string id, string theme, Vector2 pivot, float ppu)
        {
            const int w = 256;
            const int h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[w * h];

            Color sandTop, sandBottom, rockColor, grassColor;

            if (theme.IndexOf("dark", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sandTop = new Color(0.24f, 0.16f, 0.28f);
                sandBottom = new Color(0.12f, 0.08f, 0.16f);
                rockColor = new Color(0.35f, 0.25f, 0.40f);
                grassColor = new Color(0.40f, 0.20f, 0.50f);
            }
            else if (theme.IndexOf("night", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sandTop = new Color(0.18f, 0.22f, 0.32f);
                sandBottom = new Color(0.09f, 0.11f, 0.18f);
                rockColor = new Color(0.12f, 0.15f, 0.22f);
                grassColor = new Color(0.14f, 0.28f, 0.35f);
            }
            else
            {
                sandTop = new Color(0.86f, 0.68f, 0.44f); // Golden sand
                sandBottom = new Color(0.62f, 0.45f, 0.26f);
                rockColor = new Color(0.48f, 0.38f, 0.30f);
                grassColor = new Color(0.42f, 0.55f, 0.25f);
            }

            var rand = new System.Random(42);

            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / w * Mathf.PI * 6f;
                int waveY = (int)(h * 0.78f + Mathf.Sin(nx) * 5f + Mathf.Sin(nx * 3.4f) * 2f);

                for (int y = 0; y < h; y++)
                {
                    if (y <= waveY)
                    {
                        float t = (float)y / waveY;
                        Color c = Color.Lerp(sandBottom, sandTop, t);
                        // Fine grain noise
                        float noise = ((float)rand.NextDouble() - 0.5f) * 0.06f;
                        c.r = Mathf.Clamp01(c.r + noise);
                        c.g = Mathf.Clamp01(c.g + noise);
                        c.b = Mathf.Clamp01(c.b + noise);
                        pixels[y * w + x] = c;
                    }
                    else
                    {
                        pixels[y * w + x] = new Color32(0, 0, 0, 0);
                    }
                }
            }

            // Small rocks along the surface
            for (int r = 0; r < 14; r++)
            {
                int rx = rand.Next(10, w - 10);
                float nx = (float)rx / w * Mathf.PI * 6f;
                int ry = (int)(h * 0.78f + Mathf.Sin(nx) * 5f + Mathf.Sin(nx * 3.4f) * 2f) - rand.Next(2, 8);
                int rsize = rand.Next(2, 5);

                for (int dy = -rsize; dy <= rsize; dy++)
                {
                    for (int dx = -rsize; dx <= rsize; dx++)
                    {
                        if (dx * dx + dy * dy <= rsize * rsize)
                        {
                            int px = rx + dx;
                            int py = ry + dy;
                            if (px >= 0 && px < w && py >= 0 && py < h)
                                pixels[py * w + px] = rockColor;
                        }
                    }
                }
            }

            // Grass tufts
            for (int g = 0; g < 8; g++)
            {
                int gx = rand.Next(10, w - 10);
                float nx = (float)gx / w * Mathf.PI * 6f;
                int gy = (int)(h * 0.78f + Mathf.Sin(nx) * 5f + Mathf.Sin(nx * 3.4f) * 2f);

                for (int blade = -2; blade <= 2; blade++)
                {
                    for (int hg = 0; hg < 7; hg++)
                    {
                        int px = gx + blade * 2 + (blade * hg / 3);
                        int py = gy + hg;
                        if (px >= 0 && px < w && py >= 0 && py < h)
                            pixels[py * w + px] = grassColor;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
        }

        // ====================================================================
        // 4. PROPS: Mud-Brick Gate & Mysterious Cave
        // ====================================================================

        static Sprite CreateGateSprite(string id, Vector2 pivot, float ppu)
        {
            const int w = 256;
            const int h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[w * h];

            Color mudBrick = new Color(0.76f, 0.58f, 0.40f);
            Color mudDark = new Color(0.55f, 0.40f, 0.28f);
            Color woodDoor = new Color(0.36f, 0.22f, 0.12f);
            Color ironStud = new Color(0.20f, 0.20f, 0.22f);
            Color bannerRed = new Color(0.82f, 0.20f, 0.18f);

            // Mud brick towers (left & right) with center arch
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isLeftTower = x >= 20 && x <= 95 && y <= 210;
                    bool isRightTower = x >= 160 && x <= 235 && y <= 210;
                    bool isCenterLintel = x >= 95 && x <= 160 && y >= 140 && y <= 190;

                    // Crenellations (battlements) atop towers
                    bool isLeftCrenel = x >= 20 && x <= 95 && y > 210 && y <= 235 && ((x / 15) % 2 == 0);
                    bool isRightCrenel = x >= 160 && x <= 235 && y > 210 && y <= 235 && ((x / 15) % 2 == 0);

                    // Wooden arched door
                    float archDx = x - 128f;
                    float archDy = Mathf.Max(0f, y - 75f);
                    bool isDoor = x >= 96 && x <= 160 && y <= 140 && (archDx * archDx + archDy * archDy <= 34f * 34f);

                    if (isDoor)
                    {
                        // Door planks + iron studs
                        Color dc = (x % 16 == 0) ? mudDark : woodDoor;
                        if ((x % 16 == 8) && (y % 28 == 14)) dc = ironStud;
                        pixels[y * w + x] = dc;
                    }
                    else if (isLeftTower || isRightTower || isCenterLintel || isLeftCrenel || isRightCrenel)
                    {
                        // Brick mortar pattern
                        bool isMortar = (y % 16 == 0) || ((x + (y / 16) * 12) % 24 == 0);
                        pixels[y * w + x] = isMortar ? mudDark : mudBrick;
                    }
                    else
                    {
                        pixels[y * w + x] = new Color32(0, 0, 0, 0);
                    }
                }
            }

            // Red banner flying from the left tower
            for (int by = 190; by <= 230; by++)
            {
                int wave = (int)(Mathf.Sin((by - 190) * 0.2f) * 6f);
                for (int bx = 2; bx <= 22; bx++)
                {
                    int px = bx + wave;
                    int py = by;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        pixels[py * w + px] = bannerRed;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
        }

        static Sprite CreateCaveSprite(string id, Vector2 pivot, float ppu)
        {
            const int w = 256;
            const int h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[w * h];

            Color rockDark = new Color(0.24f, 0.22f, 0.28f);
            Color rockMid = new Color(0.38f, 0.35f, 0.44f);
            Color caveVoid = new Color(0.04f, 0.03f, 0.06f);
            Color runeGlow = new Color(0.15f, 0.85f, 1f, 0.95f); // Cyan magic glow

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - 128f) / 100f;
                    float dy = y / 200f;

                    // Rough rocky mouth
                    float noise = Mathf.Sin(dx * 8f) * 0.08f + Mathf.Cos(dy * 10f) * 0.06f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) + noise;

                    if (r < 0.65f)
                    {
                        // Inside darkness
                        pixels[y * w + x] = caveVoid;
                    }
                    else if (r >= 0.65f && r <= 0.78f)
                    {
                        // Glowing rune ring around entrance
                        float glow = Mathf.Sin(r * 40f + dx * 10f);
                        pixels[y * w + x] = glow > 0.4f ? runeGlow : rockDark;
                    }
                    else if (r > 0.78f && r < 1.15f)
                    {
                        // Surrounding mountain rock
                        pixels[y * w + x] = ((x + y) % 18 < 3) ? rockDark : rockMid;
                    }
                    else
                    {
                        pixels[y * w + x] = new Color32(0, 0, 0, 0);
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
        }

        // ====================================================================
        // 5. PROCEDURAL ICONS (128x128 SDF Icons & Brass Badges)
        // ====================================================================

        static Sprite CreateIconSprite(string id, Vector2 pivot, float ppu)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            bool hasBrassRing = id.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                id == "ui_dirham" || id == "ui_dinar" || id == "ui_haybah" || id == "ui_date";

            Color brassOuter = new Color(0.85f, 0.68f, 0.28f); // Golden brass
            Color brassInner = new Color(0.48f, 0.35f, 0.12f);
            Color bgDark = new Color(0.12f, 0.10f, 0.14f, 0.95f);

            float center = (size - 1) * 0.5f;
            float maxR = 56f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > maxR)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    if (hasBrassRing && dist >= maxR - 8f)
                    {
                        // Brass border with metallic bevel
                        float t = (dist - (maxR - 8f)) / 8f;
                        pixels[y * size + x] = Color.Lerp(brassOuter, brassInner, t);
                    }
                    else
                    {
                        pixels[y * size + x] = bgDark;
                    }
                }
            }

            // Draw custom SDF glyph inside the icon based on ID
            DrawGlyph(pixels, size, id);

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), pivot, ppu);
        }

        static void DrawGlyph(Color32[] pixels, int size, string id)
        {
            float c = (size - 1) * 0.5f;
            string key = id.ToLowerInvariant();

            Color gold = new Color(0.98f, 0.85f, 0.32f);
            Color silver = new Color(0.88f, 0.90f, 0.94f);
            Color cyan = new Color(0.20f, 0.82f, 0.95f);
            Color red = new Color(0.90f, 0.22f, 0.20f);
            Color green = new Color(0.25f, 0.82f, 0.35f);
            Color white = Color.white;
            Color brown = new Color(0.55f, 0.32f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    int idx = y * size + x;

                    if (key == "ui_dirham")
                    {
                        // Silver coin with 8-pointed star
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d <= 36f)
                        {
                            bool star = IsStarPoint(dx, dy, 20f, 8);
                            pixels[idx] = star ? white : silver;
                        }
                    }
                    else if (key == "ui_dinar")
                    {
                        // Gold coin with 8-pointed star
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d <= 36f)
                        {
                            bool star = IsStarPoint(dx, dy, 20f, 8);
                            pixels[idx] = star ? white : gold;
                        }
                    }
                    else if (key == "ui_date")
                    {
                        // Oval palm date fruit
                        float od = (dx * dx) / (18f * 18f) + (dy * dy) / (28f * 28f);
                        if (od <= 1f) pixels[idx] = (dx < -4f && dy > 4f) ? new Color(0.75f, 0.45f, 0.25f) : brown;
                    }
                    else if (key == "ui_haybah")
                    {
                        // Shield shape with wings
                        bool shield = Mathf.Abs(dx) <= 24f * (1f - Mathf.Clamp01(-dy / 30f)) && dy >= -28f && dy <= 24f;
                        if (shield) pixels[idx] = cyan;
                    }
                    else if (key.Contains("slash"))
                    {
                        // Crescent scimitar slash
                        float d1 = Mathf.Sqrt(dx * dx + dy * dy);
                        float d2 = Mathf.Sqrt((dx - 10) * (dx - 10) + (dy - 10) * (dy - 10));
                        if (d1 <= 32f && d2 > 26f && dy >= -20f && dx <= 25f) pixels[idx] = cyan;
                    }
                    else if (key.Contains("bow"))
                    {
                        // Curved bow
                        float d1 = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d1 >= 26f && d1 <= 32f && dx > -12f) pixels[idx] = gold;
                        if (Mathf.Abs(dx + 10f) < 2f && Mathf.Abs(dy) <= 28f) pixels[idx] = white;
                    }
                    else if (key.Contains("hourglass"))
                    {
                        // Hourglass
                        float span = Mathf.Abs(dy) * 0.7f + 4f;
                        if (Mathf.Abs(dx) <= span && Mathf.Abs(dy) <= 26f) pixels[idx] = gold;
                    }
                    else if (key.Contains("bell"))
                    {
                        // War bell
                        float span = (25f - dy) * 0.6f;
                        if (Mathf.Abs(dx) <= span && dy >= -18f && dy <= 22f) pixels[idx] = gold;
                        if (Mathf.Sqrt(dx * dx + (dy + 24f) * (dy + 24f)) <= 5f) pixels[idx] = gold;
                    }
                    else if (key.Contains("seal"))
                    {
                        // Royal wax seal stamp
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d <= 28f) pixels[idx] = (d >= 22f || IsStarPoint(dx, dy, 16f, 6)) ? gold : red;
                    }
                    else if (key.Contains("tornado"))
                    {
                        // Swirling whirlwind
                        float span = (dy + 30f) * 0.5f;
                        float wave = Mathf.Sin(dy * 0.25f) * 10f;
                        if (Mathf.Abs(dx - wave) <= span && dy >= -26f && dy <= 28f) pixels[idx] = silver;
                    }
                    else if (key.Contains("sunburst"))
                    {
                        // Radiant sunburst
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d <= 14f || IsStarPoint(dx, dy, 32f, 12)) pixels[idx] = gold;
                    }
                    else if (key.Contains("hirz"))
                    {
                        // Leather amulet diamond with engraved symbol
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) <= 34f)
                        {
                            pixels[idx] = (Mathf.Abs(dx) + Mathf.Abs(dy) >= 28f) ? gold : brown;
                        }
                    }
                    else if (key.Contains("lock"))
                    {
                        // Padlock
                        bool body = Mathf.Abs(dx) <= 22f && dy >= -24f && dy <= 4f;
                        float shackle = Mathf.Sqrt(dx * dx + (dy - 10f) * (dy - 10f));
                        bool shackleRing = shackle >= 12f && shackle <= 18f && dy >= 4f;
                        if (body || shackleRing) pixels[idx] = gold;
                    }
                    else if (key.Contains("check"))
                    {
                        // Green checkmark
                        float checkDist = CheckmarkSDF(dx, dy);
                        if (checkDist <= 5f) pixels[idx] = green;
                    }
                    else if (key.Contains("pause"))
                    {
                        // Two vertical bars
                        if ((Mathf.Abs(dx - 12f) <= 5f || Mathf.Abs(dx + 12f) <= 5f) && Mathf.Abs(dy) <= 22f)
                            pixels[idx] = white;
                    }
                    else if (key.Contains("back") || key.Contains("arrow"))
                    {
                        // Arrow pointing back
                        if ((dy >= -6f && dy <= 6f && dx >= -18f && dx <= 18f) ||
                            (dx <= -6f && Mathf.Abs(dy) <= (18f + dx)))
                            pixels[idx] = white;
                    }
                    else if (key.Contains("chest"))
                    {
                        // Treasure chest
                        if (Mathf.Abs(dx) <= 26f && Mathf.Abs(dy) <= 20f)
                        {
                            bool border = Mathf.Abs(dx) >= 22f || Mathf.Abs(dy) >= 16f || dy == 0;
                            pixels[idx] = border ? gold : brown;
                        }
                    }
                    else if (key.Contains("wheel"))
                    {
                        // Wheel of fortune
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d <= 32f)
                        {
                            float angle = Mathf.Atan2(dy, dx);
                            int wedge = (int)((angle + Mathf.PI) / (Mathf.PI * 2f) * 8f) % 2;
                            pixels[idx] = wedge == 0 ? gold : red;
                        }
                    }
                    else if (key.Contains("coffee") || key.Contains("pot"))
                    {
                        // Arabic Dallah coffee pot shape
                        float potSpan = Mathf.Clamp(20f - dy * 0.4f, 6f, 22f);
                        if (Mathf.Abs(dx) <= potSpan && dy >= -25f && dy <= 22f) pixels[idx] = gold;
                    }
                    else if (key.Contains("potion") || key.Contains("revive"))
                    {
                        // Magic potion flask
                        float flask = Mathf.Sqrt(dx * dx + (dy + 6f) * (dy + 6f));
                        bool neck = Mathf.Abs(dx) <= 6f && dy >= 0f && dy <= 24f;
                        if (flask <= 24f || neck) pixels[idx] = cyan;
                    }
                }
            }
        }

        static bool IsStarPoint(float dx, float dy, float radius, int points)
        {
            float angle = Mathf.Atan2(dy, dx);
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float mod = Mathf.Cos(angle * points);
            float rTarget = radius * (0.6f + 0.4f * Mathf.Max(0f, mod));
            return dist <= rTarget;
        }

        static float CheckmarkSDF(float x, float y)
        {
            // Simple checkmark distance from line segments
            float d1 = DistanceToSegment(new Vector2(x, y), new Vector2(-18f, 0f), new Vector2(-4f, -14f));
            float d2 = DistanceToSegment(new Vector2(x, y), new Vector2(-4f, -14f), new Vector2(20f, 16f));
            return Mathf.Min(d1, d2);
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a;
            Vector2 ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        // ====================================================================
        // 6. DETERMINISTIC FALLBACK BADGE (Never plain grey square)
        // ====================================================================

        static Sprite CreateFallbackBadge(string id, Vector2 pivot, float ppu)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            // Deterministic pleasant color from hash
            int hash = Math.Abs(id.GetHashCode());
            float hue = (hash % 360) / 360f;
            Color baseColor = Color.HSVToRGB(hue, 0.55f, 0.75f);
            Color borderColor = Color.HSVToRGB(hue, 0.85f, 0.40f);
            Color centerColor = Color.HSVToRGB(hue, 0.35f, 0.90f);

            float c = (size - 1) * 0.5f;
            float radius = 54f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    // Rounded badge shape
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > radius)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                    }
                    else if (dist >= radius - 8f)
                    {
                        pixels[y * size + x] = borderColor;
                    }
                    else
                    {
                        float innerT = dist / (radius - 8f);
                        pixels[y * size + x] = Color.Lerp(centerColor, baseColor, innerT);
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), pivot, ppu);
        }
    }
}
