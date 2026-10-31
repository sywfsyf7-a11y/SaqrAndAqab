using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Enhanced code-only cartoon animation for single-sprite characters:
    // energetic walk hop, body tilt, cartoon squash & stretch, breathing sway, and snappy lunges.
    public class ProceduralAnim : MonoBehaviour
    {
        const float WalkBob = 0.16f;
        const float WalkFrequency = 8.5f;
        const float WalkSquash = 0.10f;
        const float WalkTilt = 7.5f;

        const float IdleBob = 0.04f;
        const float IdleFrequency = 2.4f;
        const float IdleBreath = 0.05f;
        const float IdleSway = 2.2f;

        const float LungeDuration = 0.24f;
        const float LungeDistance = 0.50f;
        const float LungeTilt = 14f;

        const float KnockDuration = 0.28f;
        const float KnockDistance = 0.45f;
        const float FlashDuration = 0.12f;
        const float DieDuration = 0.70f;
        const float DieTilt = 72f;
        const float DieSink = 0.25f;

        SpriteRenderer _renderer;
        Vector3 _baseScale = Vector3.one;
        Color _baseColor = Color.white;

        float _time;
        bool _walking;
        float _walkSpeed = 1f;

        float _lungeTime = -1f;
        Vector2 _lungeDir = Vector2.right;
        float _knockTime = -1f;
        Vector2 _knockDir = Vector2.right;
        float _flashTime = -1f;
        float _dieTime = -1f;
        bool _dead;

        public bool IsDead { get { return _dead; } }

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            if (_renderer != null) _baseColor = _renderer.color;
        }

        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
        }

        public void Walk(float speed)
        {
            if (_dead) return;
            _walking = true;
            _walkSpeed = speed;
        }

        public void Idle()
        {
            if (_dead) return;
            _walking = false;
        }

        public void Lunge(Vector2 direction)
        {
            if (_dead) return;
            _lungeDir = SafeDirection(direction);
            _lungeTime = 0f;
        }

        public void Knockback(Vector2 direction)
        {
            if (_dead) return;
            _knockDir = SafeDirection(direction);
            _knockTime = 0f;
        }

        public void Flash()
        {
            if (_dead) return;
            _flashTime = 0f;
        }

        public void Die()
        {
            if (_dead) return;
            _dead = true;
            _walking = false;
            _dieTime = 0f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _time += dt;

            float bob;
            float squashX;
            float squashY;
            float tilt = 0f;

            if (_walking)
            {
                float phase = _time * WalkFrequency * Mathf.Clamp(_walkSpeed, 0.5f, 1.5f);
                bob = Mathf.Abs(Mathf.Sin(phase)) * WalkBob;
                float squash = Mathf.Sin(phase * 2f) * WalkSquash;
                squashX = 1f - squash * 0.5f;
                squashY = 1f + squash;
                tilt = Mathf.Sin(phase) * WalkTilt;
            }
            else
            {
                float phase = _time * IdleFrequency;
                bob = Mathf.Sin(phase) * IdleBob;
                float breath = Mathf.Sin(phase) * IdleBreath;
                squashX = 1f - breath;
                squashY = 1f + breath;
                tilt = Mathf.Sin(phase * 0.7f) * IdleSway;
            }

            var offset = new Vector3(0f, bob, 0f);
            float alpha = 1f;

            if (_lungeTime >= 0f)
            {
                _lungeTime += dt;
                float progress = _lungeTime / LungeDuration;
                if (progress >= 1f)
                {
                    _lungeTime = -1f;
                }
                else
                {
                    float curve = Mathf.Sin(progress * Mathf.PI);
                    offset += (Vector3)(_lungeDir * (curve * LungeDistance));
                    tilt += (_lungeDir.x > 0 ? -1f : 1f) * (curve * LungeTilt);
                }
            }

            if (_knockTime >= 0f)
            {
                _knockTime += dt;
                float progress = _knockTime / KnockDuration;
                if (progress >= 1f)
                {
                    _knockTime = -1f;
                }
                else
                {
                    float curve = Mathf.Sin(progress * Mathf.PI);
                    offset += (Vector3)(_knockDir * (curve * KnockDistance));
                    tilt += (_knockDir.x > 0 ? -1f : 1f) * (curve * 12f);
                }
            }

            if (_dieTime >= 0f)
            {
                _dieTime += dt;
                float progress = Mathf.Clamp01(_dieTime / DieDuration);
                tilt = DieTilt * progress;
                offset.y -= DieSink * progress;
                alpha = 1f - progress;

                if (progress >= 1f)
                {
                    if (_renderer != null) _renderer.enabled = false;
                    enabled = false;
                }
            }

            transform.localPosition = offset;
            transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            transform.localScale = new Vector3(_baseScale.x * squashX, _baseScale.y * squashY, _baseScale.z);

            if (_renderer == null) return;

            Color color = _baseColor;
            if (_flashTime >= 0f)
            {
                _flashTime += dt;
                float progress = _flashTime / FlashDuration;
                if (progress >= 1f) _flashTime = -1f;
                else color = Color.Lerp(Color.white, _baseColor, progress);
            }

            color.a = _baseColor.a * alpha;
            _renderer.color = color;
        }

        static Vector2 SafeDirection(Vector2 direction)
        {
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        }
    }
}
