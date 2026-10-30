using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Reusable camera shake. The battle world owns one instance and adds its offset
    // on top of the camera follow position, so both effects compose.
    public class CameraShake : MonoBehaviour
    {
        const float Frequency = 24f;

        float _amplitude;
        float _duration;
        float _time;
        float _seed;
        Vector2 _offset;

        public Vector2 Offset { get { return _offset; } }

        public bool IsPlaying { get { return _duration > 0f; } }

        public static CameraShake Create(Transform parent)
        {
            var go = new GameObject("Camera Shake", typeof(CameraShake));
            if (parent != null) go.transform.SetParent(parent, false);
            return go.GetComponent<CameraShake>();
        }

        // Amplitude is in world units, duration in seconds. A running shake keeps the stronger values.
        public void Play(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;

            if (_amplitude < amplitude) _amplitude = amplitude;
            if (_duration < duration) _duration = duration;

            _time = 0f;
            _seed = Random.value * 100f;
        }

        void Update()
        {
            if (_duration <= 0f) return;

            _time += Time.deltaTime;
            float progress = _time / _duration;

            if (progress >= 1f)
            {
                _duration = 0f;
                _amplitude = 0f;
                _offset = Vector2.zero;
                return;
            }

            float falloff = 1f - progress;
            float phase = _time * Frequency;
            float x = Mathf.PerlinNoise(_seed, phase) * 2f - 1f;
            float y = Mathf.PerlinNoise(_seed + 17.3f, phase) * 2f - 1f;

            _offset = new Vector2(x, y) * (_amplitude * falloff);
        }
    }
}
