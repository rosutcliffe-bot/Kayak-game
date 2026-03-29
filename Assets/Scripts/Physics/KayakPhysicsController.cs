using UnityEngine;
using KayakSimulator.Core;

namespace KayakSimulator.Physics
{
    /// <summary>
    /// Main kayak physics controller.
    ///
    /// Reads paddle inputs from InputManager and translates them into
    /// physically-plausible forces/torques on the kayak's Rigidbody.
    ///
    /// Two modes are supported:
    ///   • Arcade      – simplified steering, forgiving momentum
    ///   • Simulation  – realistic paddle strokes with angular momentum,
    ///                   edging, and bracing penalties
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BuoyancySystem))]
    public class KayakPhysicsController : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector — common
        // ---------------------------------------------------------------
        [Header("Propulsion")]
        [Tooltip("Peak forward force (N) produced by a single full paddle stroke.")]
        [SerializeField] private float paddleForce = 120f;

        [Tooltip("Turning torque (N·m) applied when strokes are asymmetric.")]
        [SerializeField] private float yawTorque = 60f;

        [Tooltip("Forward speed cap (m/s).")]
        [SerializeField] private float maxSpeed = 6f;

        [Header("Arcade Mode")]
        [Tooltip("Direct steering torque scale in arcade mode.")]
        [SerializeField] private float arcadeSteerTorque = 80f;

        [Header("Simulation Mode")]
        [Tooltip("Stroke power ramps up over this time (seconds) to simulate blade catch.")]
        [SerializeField] private float strokeRampTime = 0.15f;

        [Tooltip("Stroke power decays over this time after the key is released.")]
        [SerializeField] private float strokeDecayTime = 0.25f;

        [Tooltip("Maximum lean angle before capsize (degrees).")]
        [SerializeField] private float capsizeAngle = 45f;

        [Tooltip("Righting torque applied to fight capsizing.")]
        [SerializeField] private float rightingTorque = 200f;

        [Header("Water Current")]
        [Tooltip("Global water current velocity vector applied to the kayak.")]
        [SerializeField] private Vector3 waterCurrent = new Vector3(0.2f, 0f, 0f);

        // ---------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------
        private Rigidbody      _rb;
        private BuoyancySystem _buoyancy;
        private InputManager   _input;

        // Stroke power accumulator per side [0, 1]
        private float _leftStrokePower;
        private float _rightStrokePower;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            _rb       = GetComponent<Rigidbody>();
            _buoyancy = GetComponent<BuoyancySystem>();
        }

        private void Start()
        {
            _input = InputManager.Instance;
            if (_input == null)
                Debug.LogWarning("[KayakPhysicsController] InputManager not found.");
        }

        private void FixedUpdate()
        {
            if (_input == null) return;
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

            // Only apply propulsion when kayak is on the water
            if (_buoyancy.SubmersionFraction < 0.1f) return;

            UpdateStrokePower();
            ApplyPropulsion();
            ApplyRighting();
            ApplyWaterCurrent();
            ClampSpeed();
        }

        // ---------------------------------------------------------------
        // Stroke power simulation
        // ---------------------------------------------------------------
        private void UpdateStrokePower()
        {
            // Left side
            if (_input.LeftPaddleInput > 0.01f)
                _leftStrokePower  = Mathf.MoveTowards(_leftStrokePower,  1f, Time.fixedDeltaTime / strokeRampTime);
            else
                _leftStrokePower  = Mathf.MoveTowards(_leftStrokePower,  0f, Time.fixedDeltaTime / strokeDecayTime);

            // Right side
            if (_input.RightPaddleInput > 0.01f)
                _rightStrokePower = Mathf.MoveTowards(_rightStrokePower, 1f, Time.fixedDeltaTime / strokeRampTime);
            else
                _rightStrokePower = Mathf.MoveTowards(_rightStrokePower, 0f, Time.fixedDeltaTime / strokeDecayTime);
        }

        // ---------------------------------------------------------------
        // Force / torque application
        // ---------------------------------------------------------------
        private void ApplyPropulsion()
        {
            bool isArcade = GameManager.Instance != null &&
                            GameManager.Instance.CurrentSimulationMode == GameManager.SimulationMode.Arcade;

            float combinedPower = _leftStrokePower + _rightStrokePower;
            float netYaw        = _rightStrokePower - _leftStrokePower; // positive = turn right

            // Forward force proportional to combined stroke power
            Vector3 forwardForce = transform.forward * (paddleForce * combinedPower);
            _rb.AddForce(forwardForce, ForceMode.Force);

            if (isArcade)
            {
                // Arcade: add direct steering from steer axis on top
                float steer = _input.SteerAxis;
                _rb.AddTorque(transform.up * (arcadeSteerTorque * steer), ForceMode.Force);
            }
            else
            {
                // Simulation: yaw comes purely from stroke asymmetry
                _rb.AddTorque(transform.up * (yawTorque * netYaw), ForceMode.Force);
            }
        }

        /// <summary>
        /// Applies a restoring torque around the Z axis to counteract roll
        /// and prevent easy capsizing.
        /// </summary>
        private void ApplyRighting()
        {
            float rollAngle = Vector3.SignedAngle(Vector3.up, transform.up, transform.forward);

            if (Mathf.Abs(rollAngle) > capsizeAngle)
            {
                // Capsize — let physics take over (could trigger game-over here)
                return;
            }

            // Lean input modifies the target lean angle (simulation mode only)
            float targetRoll = 0f;
            bool isArcade = GameManager.Instance != null &&
                            GameManager.Instance.CurrentSimulationMode == GameManager.SimulationMode.Arcade;
            if (!isArcade)
                targetRoll = _input.LeanAxis * 15f; // up to 15° intentional lean

            float rollError  = targetRoll - rollAngle;
            float correction = rollError  * rightingTorque;
            _rb.AddTorque(transform.forward * correction, ForceMode.Force);
        }

        private void ApplyWaterCurrent()
        {
            // Apply a gentle constant nudge from the current
            _rb.AddForce(waterCurrent * _buoyancy.SubmersionFraction, ForceMode.Force);
        }

        private void ClampSpeed()
        {
            Vector3 vel = _rb.velocity;
            float   horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
            if (horizontalSpeed > maxSpeed)
            {
                Vector3 clampedXZ = new Vector3(vel.x, 0f, vel.z).normalized * maxSpeed;
                _rb.velocity = new Vector3(clampedXZ.x, vel.y, clampedXZ.z);
            }
        }

        // ---------------------------------------------------------------
        // Public accessors
        // ---------------------------------------------------------------

        /// <summary>Current forward speed in m/s (always positive).</summary>
        public float ForwardSpeed =>
            Vector3.Dot(_rb.velocity, transform.forward);

        /// <summary>Stroke powers for UI / animation feedback.</summary>
        public float LeftStrokePower  => _leftStrokePower;
        public float RightStrokePower => _rightStrokePower;
    }
}
