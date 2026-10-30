using UnityEngine;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.Battle
{
    // The crescent slash visual: one sprite that starts small in front of the hero,
    // grows to its full size and fades out. Uses unscaled time so it plays through the hit-stop.
    public class CrescentFx : MonoBehaviour
    {
        const string SpriteId = "fx_crescent";
        const float Duration = 0.34f;
        const float StartScale = 0.45f;
        const float EndScale = 1.15f;
        const float ForwardOffset = 0.4f;
        const int SortingOrder = 480;

        SpriteRenderer _renderer;
        float _time;
        float _scale = 1f;
        Color _color = Color.white;

        public static void Play(Transform parent, Vector3 heroPosition, bool facingRight, float range)
        {
            var go = new GameObject("Crescent FX", typeof(CrescentFx));
            if (parent != null) go.transform.SetParent(parent, false);

            var fx = go.GetComponent<CrescentFx>();
            fx.Setup(heroPosition, facingRight, range);
        }

        void Setup(Vector3 heroPosition, bool facingRight, float range)
        {
            var sprite = AssetLibrary.GetSprite(SpriteId);
            float worldHeight = AssetLibrary.GetWorldHeight(SpriteId);
            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            _scale = worldHeight / spriteHeight;

            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = sprite;
            _renderer.sortingOrder = SortingOrder;
            _renderer.flipX = !facingRight;
            _color = _renderer.color;

            float offset = range * ForwardOffset;
            transform.position = heroPosition + new Vector3(facingRight ? offset : -offset, 0f, 0f);
            transform.localScale = Vector3.one * (_scale * StartScale);
        }

        void Update()
        {
            _time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(_time / Duration);

            transform.localScale = Vector3.one * (_scale * Mathf.Lerp(StartScale, EndScale, progress));

            if (_renderer != null)
            {
                Color color = _color;
                color.a = _color.a * (1f - progress);
                _renderer.color = color;
            }

            if (progress >= 1f) Destroy(gameObject);
        }
    }
}
