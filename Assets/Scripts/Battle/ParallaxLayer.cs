using System.Collections.Generic;
using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // One horizontal background band. It is built once from copies of a single sprite and then
    // only its root moves: the copies are laid out in local space, so there is no per-frame work.
    // Every second copy is mirrored (flipX) so the seams of a repeating sprite are not obvious.
    public class ParallaxLayer : MonoBehaviour
    {
        readonly List<SpriteRenderer> _copies = new List<SpriteRenderer>();

        Transform _camera;
        float _factor;

        public float Factor { get { return _factor; } }

        // spanMinX..spanMaxX are local X coordinates that must always be covered while the camera
        // moves inside its clamp range. bottomY is the bottom edge of the band in world space.
        public void Build(Transform cameraTransform, float factor, Sprite sprite, Vector2 copySize,
            float bottomY, float spanMinX, float spanMaxX, int sortingOrder, Color tint)
        {
            _camera = cameraTransform;
            _factor = factor;

            if (sprite == null || copySize.x <= 0f || copySize.y <= 0f) return;

            Vector2 spriteSize = sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

            var scale = new Vector3(copySize.x / spriteSize.x, copySize.y / spriteSize.y, 1f);

            // The sprite pivot may be bottom or center; place the copy so its bottom edge is at bottomY.
            float pivotY = sprite.rect.height > 0f ? sprite.pivot.y / sprite.rect.height : 0.5f;
            float y = bottomY + copySize.y * pivotY;

            float span = spanMaxX - spanMinX;
            int count = Mathf.Max(1, Mathf.CeilToInt(span / copySize.x));

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Copy " + i, typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(spanMinX + copySize.x * (i + 0.5f), y, 0f);
                go.transform.localScale = scale;

                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = tint;
                renderer.flipX = (i % 2) == 1;
                renderer.sortingOrder = sortingOrder;

                _copies.Add(renderer);
            }

            Refresh(cameraTransform != null ? cameraTransform.position.x : 0f);
        }

        // Factor 1 = glued to the world, factor 0 = glued to the camera.
        public void Refresh(float cameraX)
        {
            var position = transform.position;
            position.x = cameraX * (1f - _factor);
            transform.position = position;
        }

        public void SetTint(Color tint)
        {
            for (int i = 0; i < _copies.Count; i++) _copies[i].color = tint;
        }
    }
}
