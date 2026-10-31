using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SaqrAndAqab.Core
{
    // The only place that reads the player's raw input.
    // MoveAxis: -1 = hold the left half of the screen, +1 = hold the right half.
    // Keyboard A/D and arrow keys also work (editor / PC).
    public static class InputService
    {
        // Touch that started on top of a UI element: ignored until the finger is lifted.
        static int _ignoredTouchId = -1;

        // Allows automated tests / bot to simulate movement without physical inputs.
        public static float? SimulatedMoveAxis { get; set; }

        public static float MoveAxis
        {
            get
            {
                if (SimulatedMoveAxis.HasValue) return SimulatedMoveAxis.Value;
                return Compute();
            }
        }

        public static bool IsPointerOverUI()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            return eventSystem.IsPointerOverGameObject();
        }

#if ENABLE_INPUT_SYSTEM
        static float Compute()
        {
            float axis = 0f;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) axis -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) axis += 1f;
                if (axis != 0f) return Mathf.Clamp(axis, -1f, 1f);
            }

            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                bool ignoredTouchStillDown = false;
                var touches = touchscreen.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    var touch = touches[i];
                    int touchId = touch.touchId.ReadValue();
                    if (!touch.press.isPressed) continue;

                    if (touchId == _ignoredTouchId)
                    {
                        ignoredTouchStillDown = true;
                        continue;
                    }

                    if (IsPointerOverUI(touchId))
                    {
                        _ignoredTouchId = touchId;
                        ignoredTouchStillDown = true;
                        continue;
                    }

                    float x = touch.position.ReadValue().x;
                    return x < UnityEngine.Screen.width * 0.5f ? -1f : 1f;
                }

                if (!ignoredTouchStillDown) _ignoredTouchId = -1;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                if (IsPointerOverUI()) return 0f;
                float x = mouse.position.ReadValue().x;
                return x < UnityEngine.Screen.width * 0.5f ? -1f : 1f;
            }

            return 0f;
        }

        static bool IsPointerOverUI(int pointerId)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            return eventSystem.IsPointerOverGameObject(pointerId);
        }
#else
        static float Compute()
        {
            float axis = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) axis -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) axis += 1f;
            if (axis != 0f) return Mathf.Clamp(axis, -1f, 1f);

            bool ignoredTouchStillDown = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began &&
                    touch.phase != TouchPhase.Moved &&
                    touch.phase != TouchPhase.Stationary) continue;

                if (touch.fingerId == _ignoredTouchId)
                {
                    ignoredTouchStillDown = true;
                    continue;
                }

                if (touch.phase == TouchPhase.Began && IsPointerOverUI(touch.fingerId))
                {
                    _ignoredTouchId = touch.fingerId;
                    ignoredTouchStillDown = true;
                    continue;
                }

                return touch.position.x < UnityEngine.Screen.width * 0.5f ? -1f : 1f;
            }

            if (!ignoredTouchStillDown) _ignoredTouchId = -1;

            if (Input.GetMouseButton(0))
            {
                if (IsPointerOverUI()) return 0f;
                return Input.mousePosition.x < UnityEngine.Screen.width * 0.5f ? -1f : 1f;
            }

            return 0f;
        }

        static bool IsPointerOverUI(int pointerId)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            return eventSystem.IsPointerOverGameObject(pointerId);
        }
#endif

#if UNITY_EDITOR
        // Debug keys (editor only, never active in a build): T dummy, N next wave, K kill all enemies.
        static bool _debugSpawnWasDown;
        static bool _debugNextWaveWasDown;
        static bool _debugKillAllWasDown;
        static bool _debugJumpToWave10WasDown;

        // Press T in the editor to spawn the training dummy next to the hero.
        public static bool DebugSpawnDummyPressed
        {
            get
            {
                bool now = false;
                var keyboard = Keyboard.current;
                if (keyboard != null) now = keyboard.tKey.isPressed;
                bool pressed = now && !_debugSpawnWasDown;
                _debugSpawnWasDown = now;
                return pressed;
            }
        }

        // Press N in the editor to start the next wave.
        public static bool DebugNextWavePressed
        {
            get
            {
                bool now = false;
                var keyboard = Keyboard.current;
                if (keyboard != null) now = keyboard.nKey.isPressed;
                bool pressed = now && !_debugNextWaveWasDown;
                _debugNextWaveWasDown = now;
                return pressed;
            }
        }

        // Press K in the editor to kill every enemy on the field.
        public static bool DebugKillAllPressed
        {
            get
            {
                bool now = false;
                var keyboard = Keyboard.current;
                if (keyboard != null) now = keyboard.kKey.isPressed;
                bool pressed = now && !_debugKillAllWasDown;
                _debugKillAllWasDown = now;
                return pressed;
            }
        }

        // Press J in the editor to jump straight to wave 10 (boss).
        public static bool DebugJumpToWave10Pressed
        {
            get
            {
                bool now = false;
                var keyboard = Keyboard.current;
                if (keyboard != null) now = keyboard.jKey.isPressed;
                bool pressed = now && !_debugJumpToWave10WasDown;
                _debugJumpToWave10WasDown = now;
                return pressed;
            }
        }
#endif
    }
}
