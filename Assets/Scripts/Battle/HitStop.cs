using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Short freeze frame ("hit-stop") used by impact abilities.
    // Time.timeScale is only ever restored to the value it had before the freeze,
    // and the timer runs on unscaled time so the freeze always ends.
    public static class HitStop
    {
        static bool _active;
        static float _remaining;
        static float _savedScale = 1f;

        public static bool IsActive { get { return _active; } }

        public static void Play(float seconds)
        {
            if (seconds <= 0f) return;

            if (_active)
            {
                if (seconds > _remaining) _remaining = seconds;
                return;
            }

            _savedScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            _remaining = seconds;
            _active = true;
            Time.timeScale = 0f;
        }

        // Called once per frame by the battle world with unscaled time.
        public static void Tick()
        {
            if (!_active) return;

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining > 0f) return;

            Restore();
        }

        // Stops a running freeze immediately (pause, restart, leaving the battle).
        public static void Cancel()
        {
            if (_active) Restore();
        }

        static void Restore()
        {
            _active = false;
            _remaining = 0f;
            Time.timeScale = _savedScale;
        }
    }
}
