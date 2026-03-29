using UnityEngine;
using KayakSimulator.Core;
using KayakSimulator.Physics;

namespace KayakSimulator.Controls
{
    /// <summary>
    /// Smooth camera controller supporting first-person, third-person chase,
    /// and orbital (cinematic) views.
    ///
    /// Interpolates between views with a configurable blend speed so
    /// transitions are never jarring.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Camera modes
        // ---------------------------------------------------------------
        public enum CameraMode { ThirdPerson, FirstPerson, Orbital }

        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Target")]
        [SerializeField] private Transform kayakTransform;

        [Header("Mode")]
        [SerializeField] private CameraMode initialMode = CameraMode.ThirdPerson;

        [Header("Third-Person Settings")]
        [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 2.5f, -5f);
        [SerializeField] private float   thirdPersonFOV    = 65f;

        [Header("First-Person Settings")]
        [SerializeField] private Vector3 firstPersonOffset = new Vector3(0f, 0.9f, 0.2f);
        [SerializeField] private float   firstPersonFOV    = 75f;

        [Header("Orbital Settings")]
        [SerializeField] private float orbitalRadius   = 8f;
        [SerializeField] private float orbitalHeight   = 3f;
        [SerializeField] private float orbitalSpeed    = 20f;  // degrees per second
        [SerializeField] private float orbitalFOV      = 55f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.15f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float fovSmoothTime      = 0.3f;
        [SerializeField] private float modeSwitchBlend    = 5f;

        [Header("Collision")]
        [Tooltip("Camera will be pushed forward to avoid clipping terrain/water.")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float     collisionRadius = 0.3f;

        // ---------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------
        private Camera     _cam;
        private CameraMode _currentMode;
        private InputManager _input;

        private Vector3    _posVelocity;
        private float      _fovVelocity;
        private float      _orbitalAngle;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            _cam         = GetComponent<Camera>();
            _currentMode = initialMode;
        }

        private void Start()
        {
            _input = InputManager.Instance;
        }

        private void LateUpdate()
        {
            if (kayakTransform == null) return;

            // Handle mode switching input
            if (_input != null && _input.CameraSwitchPressed)
                CycleMode();

            // Advance orbital angle
            if (_currentMode == CameraMode.Orbital)
                _orbitalAngle += orbitalSpeed * Time.deltaTime;

            // Compute desired position & rotation
            Vector3    desiredPos;
            Quaternion desiredRot;
            float      desiredFOV;
            GetDesiredTransform(out desiredPos, out desiredRot, out desiredFOV);

            // Smooth position
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPos, ref _posVelocity, positionSmoothTime);

            // Smooth rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation, desiredRot, Time.deltaTime / rotationSmoothTime);

            // Smooth FOV
            if (_cam != null)
                _cam.fieldOfView = Mathf.SmoothDamp(
                    _cam.fieldOfView, desiredFOV, ref _fovVelocity, fovSmoothTime);

            // Collision push-in
            ResolveCollision(desiredPos);
        }

        // ---------------------------------------------------------------
        // Desired transform computation
        // ---------------------------------------------------------------
        private void GetDesiredTransform(
            out Vector3 pos, out Quaternion rot, out float fov)
        {
            switch (_currentMode)
            {
                case CameraMode.FirstPerson:
                    pos = kayakTransform.TransformPoint(firstPersonOffset);
                    rot = kayakTransform.rotation;
                    fov = firstPersonFOV;
                    break;

                case CameraMode.Orbital:
                    float rad = _orbitalAngle * Mathf.Deg2Rad;
                    Vector3 orbitOffset = new Vector3(
                        Mathf.Sin(rad) * orbitalRadius,
                        orbitalHeight,
                        Mathf.Cos(rad) * orbitalRadius);
                    pos = kayakTransform.position + orbitOffset;
                    rot = Quaternion.LookRotation(kayakTransform.position - pos);
                    fov = orbitalFOV;
                    break;

                default: // ThirdPerson
                    pos = kayakTransform.TransformPoint(thirdPersonOffset);
                    rot = Quaternion.LookRotation(
                        kayakTransform.position - pos + Vector3.up * 0.5f);
                    fov = thirdPersonFOV;
                    break;
            }
        }

        private void ResolveCollision(Vector3 desiredPos)
        {
            if (kayakTransform == null) return;

            Vector3 direction = desiredPos - kayakTransform.position;
            float   dist      = direction.magnitude;

            if (UnityEngine.Physics.SphereCast(
                    kayakTransform.position, collisionRadius,
                    direction.normalized, out RaycastHit hit,
                    dist, collisionMask))
            {
                transform.position = kayakTransform.position + direction.normalized * hit.distance;
            }
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------
        public void SetMode(CameraMode mode) => _currentMode = mode;

        public void CycleMode()
        {
            _currentMode = (CameraMode)(((int)_currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length);
        }
    }
}
