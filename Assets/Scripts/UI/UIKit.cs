using UnityEngine;
using UnityEngine.UI;

namespace SaqrAndAqab.UI
{
    // Procedural UI Kit: 9-slice dark wood & brass panels, parchment banners,
    // and wooden buttons with pressed states.
    public static class UIKit
    {
        static Sprite _panelSprite;
        static Sprite _bannerSprite;
        static Sprite _buttonNormalSprite;
        static Sprite _buttonPressedSprite;

        public static Sprite PanelSprite
        {
            get
            {
                if (_panelSprite == null) _panelSprite = CreatePanelSprite();
                return _panelSprite;
            }
        }

        public static Sprite BannerSprite
        {
            get
            {
                if (_bannerSprite == null) _bannerSprite = CreateBannerSprite();
                return _bannerSprite;
            }
        }

        public static Sprite ButtonNormalSprite
        {
            get
            {
                if (_buttonNormalSprite == null) _buttonNormalSprite = CreateButtonSprite(false);
                return _buttonNormalSprite;
            }
        }

        public static Sprite ButtonPressedSprite
        {
            get
            {
                if (_buttonPressedSprite == null) _buttonPressedSprite = CreateButtonSprite(true);
                return _buttonPressedSprite;
            }
        }

        public static void ApplyPanel(Image image)
        {
            if (image == null) return;
            image.sprite = PanelSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        public static void ApplyBanner(Image image)
        {
            if (image == null) return;
            image.sprite = BannerSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        public static void ApplyButton(Button button, Image image)
        {
            if (image != null)
            {
                image.sprite = ButtonNormalSprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }

            if (button != null)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                var ss = button.spriteState;
                ss.pressedSprite = ButtonPressedSprite;
                ss.highlightedSprite = ButtonNormalSprite;
                button.spriteState = ss;
            }
        }

        // ====================================================================
        // Procedural 9-Slice Generation
        // ====================================================================

        static Sprite CreatePanelSprite()
        {
            const int size = 64;
            const int border = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            Color woodCenter = new Color(0.14f, 0.10f, 0.08f, 0.94f); // Dark rich wood
            Color brassEdge = new Color(0.82f, 0.65f, 0.28f);         // Golden brass
            Color brassHighlight = new Color(0.96f, 0.85f, 0.45f);    // Highlight
            Color brassShadow = new Color(0.38f, 0.26f, 0.10f);       // Deep bevel
            Color rivetColor = new Color(0.98f, 0.90f, 0.60f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    int idx = y * size + x;

                    if (edgeDist < 2)
                    {
                        pixels[idx] = brassShadow;
                    }
                    else if (edgeDist >= 2 && edgeDist <= 5)
                    {
                        pixels[idx] = (edgeDist == 3) ? brassHighlight : brassEdge;
                    }
                    else if (edgeDist == 6)
                    {
                        pixels[idx] = brassShadow;
                    }
                    else
                    {
                        // Wood interior with subtle grain
                        float grain = ((x * 7 + y * 13) % 17) * 0.003f;
                        pixels[idx] = new Color(woodCenter.r + grain, woodCenter.g + grain, woodCenter.b + grain, woodCenter.a);
                    }

                    // Corner rivets
                    bool isCornerRivet = (x == 10 || x == size - 11) && (y == 10 || y == size - 11);
                    if (isCornerRivet)
                    {
                        pixels[idx] = rivetColor;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        static Sprite CreateBannerSprite()
        {
            const int size = 64;
            const int border = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            Color parchment = new Color(0.95f, 0.89f, 0.78f, 0.98f);
            Color parchmentEdge = new Color(0.82f, 0.72f, 0.55f);
            Color parchmentBorder = new Color(0.48f, 0.36f, 0.22f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    int idx = y * size + x;

                    if (edgeDist < 2)
                    {
                        pixels[idx] = parchmentBorder;
                    }
                    else if (edgeDist < 6)
                    {
                        pixels[idx] = parchmentEdge;
                    }
                    else
                    {
                        pixels[idx] = parchment;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        static Sprite CreateButtonSprite(bool pressed)
        {
            const int size = 64;
            const int border = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            Color wood = pressed ? new Color(0.24f, 0.15f, 0.08f) : new Color(0.42f, 0.26f, 0.14f);
            Color woodBevel = pressed ? new Color(0.18f, 0.10f, 0.05f) : new Color(0.56f, 0.36f, 0.20f);
            Color brassEdge = pressed ? new Color(0.65f, 0.48f, 0.18f) : new Color(0.85f, 0.68f, 0.28f);
            Color brassShadow = new Color(0.32f, 0.22f, 0.08f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    int idx = y * size + x;

                    if (edgeDist < 2)
                    {
                        pixels[idx] = brassShadow;
                    }
                    else if (edgeDist < 5)
                    {
                        pixels[idx] = brassEdge;
                    }
                    else if (edgeDist < 8)
                    {
                        pixels[idx] = woodBevel;
                    }
                    else
                    {
                        pixels[idx] = wood;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }
    }
}
