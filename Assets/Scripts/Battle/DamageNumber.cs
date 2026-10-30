using TMPro;
using UnityEngine;
using SaqrAndAqab.UI;

namespace SaqrAndAqab.Battle
{
    // One floating damage number: rises and fades out, then returns to the pool.
    public class DamageNumber : MonoBehaviour
    {
        const float LifeTime = 0.9f;
        const float RiseDistance = 1.1f;

        static readonly Color NormalColor = new Color(1f, 1f, 1f, 1f);
        static readonly Color CritColor = new Color(1f, 0.85f, 0.15f, 1f);

        public TMP_Text Text;
        Vector3 _start;
        float _elapsed;

        public void Setup(Vector3 worldPos, int amount, bool crit)
        {
            _elapsed = 0f;
            _start = worldPos;
            transform.position = worldPos;

            if (Text == null)
            {
                var textGo = new GameObject("DamageNumberText", typeof(RectTransform), typeof(TextMeshPro));
                textGo.transform.SetParent(transform, false);

                Text = textGo.GetComponent<TextMeshPro>();
                var rect = textGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(2f, 1f);
                Text.font = UIFactory.RegularFont;
                Text.alignment = TextAlignmentOptions.Center;
                Text.fontSize = 5f;

                var meshRenderer = textGo.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.sortingOrder = 500;
            }

            Text.text = amount.ToString();
            Text.color = crit ? CritColor : NormalColor;
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / LifeTime);
            float alpha = 1f - progress;
            float rise = RiseDistance * (1f - progress);

            Color color = Text.color;
            color.a = alpha;
            Text.color = color;

            transform.position = new Vector3(_start.x, _start.y + rise, _start.z);

            if (_elapsed >= LifeTime) DamageNumbers.Return(this);
        }
    }
}
