using System.Collections.Generic;
using UnityEngine;

namespace KayakSimulator.Physics
{
    /// <summary>
    /// Archimedes-principle buoyancy system.
    ///
    /// Attaches to any Rigidbody that should float on the water.
    /// Samples N buoyancy points on the object's hull; for each point
    /// below the dynamic wave surface it applies an upward buoyant force
    /// proportional to the submerged volume.
    ///
    /// Also applies a water drag and angular drag that increase with
    /// submersion depth to simulate hydrodynamic resistance.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyancySystem : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Buoyancy Points")]
        [Tooltip("Local-space positions on the hull used for buoyancy sampling. " +
                 "More points = more accurate tipping behaviour.")]
        [SerializeField] private List<Vector3> buoyancyPoints = new List<Vector3>
        {
            // Bow, stern, port, starboard, and centre — sensible defaults for a kayak
            new Vector3( 0f,   0f,  1.8f),
            new Vector3( 0f,   0f, -1.8f),
            new Vector3(-0.3f, 0f,  0f  ),
            new Vector3( 0.3f, 0f,  0f  ),
            new Vector3( 0f,   0f,  0f  ),
        };

        [Header("Fluid Properties")]
        [Tooltip("Density of water in kg/m³ (fresh water ≈ 1000, sea water ≈ 1025).")]
        [SerializeField] private float waterDensity = 1025f;

        [Tooltip("Volume of water displaced per buoyancy point when fully submerged (m³). " +
                 "Tune so that the kayak floats at the desired waterline.")]
        [SerializeField] private float displacedVolumePerPoint = 0.04f;

        [Header("Drag")]
        [Tooltip("Linear drag multiplier applied when the object is in water.")]
        [SerializeField] private float waterLinearDrag = 1.5f;

        [Tooltip("Angular drag multiplier applied when the object is in water.")]
        [SerializeField] private float waterAngularDrag = 2f;

        [Tooltip("Drag applied to lateral (sideways) velocity to resist capsizing slides.")]
        [SerializeField] private float lateralDampingForce = 800f;

        // ---------------------------------------------------------------
        // References
        // ---------------------------------------------------------------
        private Rigidbody   _rb;
        private GerstnerWaveSystem _waves;

        // Original drag values so we can restore if out of water
        private float _originalDrag;
        private float _originalAngularDrag;

        // Track how many points are currently submerged
        private int _submergedCount;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _originalDrag        = _rb.drag;
            _originalAngularDrag = _rb.angularDrag;
        }

        private void Start()
        {
            _waves = FindAnyObjectByType<GerstnerWaveSystem>();
            if (_waves == null)
                Debug.LogWarning("[BuoyancySystem] No GerstnerWaveSystem found in scene.");
        }

        private void FixedUpdate()
        {
            ApplyBuoyancy();
            ApplyWaterDrag();
            ApplyLateralDamping();
        }

        // ---------------------------------------------------------------
        // Public helpers
        // ---------------------------------------------------------------

        /// <summary>Fraction of buoyancy points that are currently submerged [0, 1].</summary>
        public float SubmersionFraction =>
            buoyancyPoints.Count > 0 ? (float)_submergedCount / buoyancyPoints.Count : 0f;

        // ---------------------------------------------------------------
        // Private physics
        // ---------------------------------------------------------------
        private void ApplyBuoyancy()
        {
            if (_waves == null) return;

            _submergedCount = 0;
            float g = Mathf.Abs(UnityEngine.Physics.gravity.y);

            foreach (Vector3 localPoint in buoyancyPoints)
            {
                Vector3 worldPoint  = transform.TransformPoint(localPoint);
                float   waveHeight  = _waves.GetWaveHeight(worldPoint.x, worldPoint.z);
                float   submergedBy = waveHeight - worldPoint.y;   // positive = below surface

                if (submergedBy > 0f)
                {
                    _submergedCount++;

                    // Clamp so we don't over-drive deeply submerged points
                    float depth         = Mathf.Min(submergedBy, 1f);
                    float buoyantForce  = waterDensity * g * displacedVolumePerPoint * depth;

                    _rb.AddForceAtPosition(Vector3.up * buoyantForce, worldPoint, ForceMode.Force);
                }
            }
        }

        private void ApplyWaterDrag()
        {
            if (_submergedCount > 0)
            {
                _rb.drag         = waterLinearDrag;
                _rb.angularDrag  = waterAngularDrag;
            }
            else
            {
                _rb.drag         = _originalDrag;
                _rb.angularDrag  = _originalAngularDrag;
            }
        }

        private void ApplyLateralDamping()
        {
            if (_submergedCount == 0) return;

            // Damp the sideways (local X) velocity to prevent unrealistic sliding
            Vector3 localVelocity   = transform.InverseTransformDirection(_rb.velocity);
            float   lateralVelocity = localVelocity.x;
            Vector3 dampForce       = -transform.right * (lateralVelocity * lateralDampingForce);
            _rb.AddForce(dampForce, ForceMode.Force);
        }

        // ---------------------------------------------------------------
        // Editor visualisation
        // ---------------------------------------------------------------
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            foreach (Vector3 lp in buoyancyPoints)
            {
                Vector3 wp = transform.TransformPoint(lp);
                Gizmos.DrawWireSphere(wp, 0.08f);
            }
        }
#endif
    }
}
