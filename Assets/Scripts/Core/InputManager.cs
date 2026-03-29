using UnityEngine;
using UnityEngine.InputSystem;

namespace KayakSimulator.Core
{
    /// <summary>
    /// Centralised input manager that reads from Unity's new Input System
    /// and exposes clean paddle / steering values to other systems.
    /// Supports keyboard, mouse, and gamepad simultaneously.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Singleton
        // ---------------------------------------------------------------
        public static InputManager Instance { get; private set; }

        // ---------------------------------------------------------------
        // Cached input values (updated every frame in Update)
        // ---------------------------------------------------------------

        /// <summary>Left paddle stroke strength [0, 1].</summary>
        public float LeftPaddleInput { get; private set; }

        /// <summary>Right paddle stroke strength [0, 1].</summary>
        public float RightPaddleInput { get; private set; }

        /// <summary>Combined lean / tilt axis [-1, 1]. Negative = lean left.</summary>
        public float LeanAxis { get; private set; }

        /// <summary>Rudder / steering axis [-1, 1].</summary>
        public float SteerAxis { get; private set; }

        /// <summary>True during the frame the pause button is pressed.</summary>
        public bool PausePressed { get; private set; }

        /// <summary>True during the frame the camera-switch button is pressed.</summary>
        public bool CameraSwitchPressed { get; private set; }

        // ---------------------------------------------------------------
        // Inspector-configurable key / button bindings (keyboard fallback)
        // ---------------------------------------------------------------
        [Header("Keyboard Bindings")]
        [SerializeField] private KeyCode leftPaddleKey  = KeyCode.A;
        [SerializeField] private KeyCode rightPaddleKey = KeyCode.D;
        [SerializeField] private KeyCode leanLeftKey    = KeyCode.Q;
        [SerializeField] private KeyCode leanRightKey   = KeyCode.E;
        [SerializeField] private KeyCode pauseKey       = KeyCode.Escape;
        [SerializeField] private KeyCode cameraSwitchKey = KeyCode.C;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            ReadKeyboardInput();
            ReadGamepadInput();

            PausePressed        = Input.GetKeyDown(pauseKey);
            CameraSwitchPressed = Input.GetKeyDown(cameraSwitchKey);

            // Forward pause to GameManager
            if (PausePressed && GameManager.Instance != null)
                GameManager.Instance.TogglePause();
        }

        // ---------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------
        private void ReadKeyboardInput()
        {
            // Paddle strokes: analogue feel via hold duration is handled by
            // KayakPhysicsController, so we just expose a binary 0/1 here.
            LeftPaddleInput  = Input.GetKey(leftPaddleKey)  ? 1f : 0f;
            RightPaddleInput = Input.GetKey(rightPaddleKey) ? 1f : 0f;

            float leanInput = 0f;
            if (Input.GetKey(leanLeftKey))  leanInput -= 1f;
            if (Input.GetKey(leanRightKey)) leanInput += 1f;
            LeanAxis = leanInput;

            // Arrow keys / WASD steering
            SteerAxis = Input.GetAxis("Horizontal");
        }

        private void ReadGamepadInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            // Right trigger → right paddle; Left trigger → left paddle
            float rtValue = gamepad.rightTrigger.ReadValue();
            float ltValue = gamepad.leftTrigger.ReadValue();

            // Blend keyboard and gamepad (take the greater value)
            LeftPaddleInput  = Mathf.Max(LeftPaddleInput,  ltValue);
            RightPaddleInput = Mathf.Max(RightPaddleInput, rtValue);

            // Left stick horizontal → steer
            float stickX = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > Mathf.Abs(SteerAxis))
                SteerAxis = stickX;

            // Right stick horizontal → lean
            float rstickX = gamepad.rightStick.x.ReadValue();
            if (Mathf.Abs(rstickX) > Mathf.Abs(LeanAxis))
                LeanAxis = rstickX;
        }
    }
}
