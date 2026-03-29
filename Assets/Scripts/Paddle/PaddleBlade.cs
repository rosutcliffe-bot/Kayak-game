using UnityEngine;
using KayakSimulator.Physics;

namespace KayakSimulator.Paddle
{
    /// <summary>
    /// Paddle blade interaction system.
    ///
    /// Represents one blade of the double-bladed kayak paddle.
    /// When the blade enters the water it:
    ///   1. Computes hydrodynamic lift and drag forces on the Rigidbody.
    ///   2. Spawns splash/ripple VFX via the PaddleEffects component.
    ///   3. Reports interaction data (depth, velocity) for animation.
    ///
    /// Attach one of these to each blade transform (child of the paddle).
    /// </summary>
    public class PaddleBlade : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Blade Properties")]
        [Tooltip("Surface area of the blade face (m²).")]
        [SerializeField] private float bladeArea = 0.06f;

        [Tooltip("Drag coefficient of the blade (flat plate ≈ 1.28).")]
        [SerializeField] private float dragCoefficient = 1.2f;

        [Tooltip("Lift coefficient (for angled strokes).")]
        [SerializeField] private float liftCoefficient = 0.4f;

        [Header("VFX")]
        [Tooltip("Splash particle system spawned on blade entry.")]
        [SerializeField] private ParticleSystem splashParticles;

        [Tooltip("Ripple/wake trail particle system active while blade is submerged.")]
        [SerializeField] private ParticleSystem wakeTrailParticles;

        [Tooltip("Minimum blade speed (m/s) to trigger splash.")]
        [SerializeField] private float splashSpeedThreshold = 0.5f;

        // ---------------------------------------------------------------
        // References
        // ---------------------------------------------------------------
        private Rigidbody    _kayakRb;
        private WaterPhysics _water;

        // ---------------------------------------------------------------
        // State
        // ---------------------------------------------------------------
        private bool  _wasSubmerged;
        private float _submersionDepth;

        // ---------------------------------------------------------------
        // Public accessors
        // ---------------------------------------------------------------
        /// <summary>True when this blade is currently in the water.</summary>
        public bool IsSubmerged => _submersionDepth > 0f;

        /// <summary>Blade depth below the water surface (metres, clamped ≥ 0).</summary>
        public float SubmersionDepth => _submersionDepth;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Start()
        {
            // The Rigidbody lives on the kayak root — walk up to find it
            _kayakRb = GetComponentInParent<Rigidbody>();
            if (_kayakRb == null)
                Debug.LogError("[PaddleBlade] No Rigidbody found in parent hierarchy.");

            _water = WaterPhysics.Instance;
            if (_water == null)
                Debug.LogWarning("[PaddleBlade] WaterPhysics not found — hydrodynamics disabled.");
        }

        private void FixedUpdate()
        {
            UpdateSubmersion();

            if (_submersionDepth > 0f)
            {
                ApplyHydrodynamicForce();
                HandleWakeVFX(true);
            }
            else
            {
                HandleWakeVFX(false);
            }
        }

        // ---------------------------------------------------------------
        // Physics
        // ---------------------------------------------------------------
        private void UpdateSubmersion()
        {
            if (_water == null) return;

            Vector3 pos        = transform.position;
            float   surfaceY   = _water.GetSurfaceHeight(pos.x, pos.z);
            _submersionDepth   = Mathf.Max(0f, surfaceY - pos.y);

            bool isSubmerged   = _submersionDepth > 0.01f;

            // Rising edge — blade just entered water
            if (isSubmerged && !_wasSubmerged)
                OnBladeEnterWater();

            _wasSubmerged = isSubmerged;
        }

        private void ApplyHydrodynamicForce()
        {
            if (_kayakRb == null || _water == null) return;

            // Velocity of this point on the blade in world space
            Vector3 bladeVelocity = _kayakRb.GetPointVelocity(transform.position);

            // Relative velocity of blade through water
            Vector3 waterVel    = _water.GetWaterVelocityAt(transform.position);
            Vector3 relVelocity = bladeVelocity - waterVel;
            float   speed       = relVelocity.magnitude;
            if (speed < 0.001f) return;

            // Fluid density (water)
            const float rho = 1025f;

            // --- Drag force (opposes blade motion, propels kayak) ---
            float   dragMag   = 0.5f * rho * speed * speed * dragCoefficient * bladeArea;
            Vector3 dragForce = -relVelocity.normalized * dragMag;

            // --- Lift force (perpendicular, useful for sweep strokes) ---
            Vector3 bladeNormal = transform.up;          // blade face normal
            Vector3 liftDir     = Vector3.Cross(relVelocity.normalized, bladeNormal).normalized;
            float   liftMag     = 0.5f * rho * speed * speed * liftCoefficient * bladeArea;
            Vector3 liftForce   = liftDir * liftMag;

            // Attenuate by submersion depth (partial blade contact)
            float depthFactor = Mathf.Clamp01(_submersionDepth / 0.15f);

            _kayakRb.AddForceAtPosition(
                (dragForce + liftForce) * depthFactor,
                transform.position,
                ForceMode.Force);
        }

        // ---------------------------------------------------------------
        // VFX
        // ---------------------------------------------------------------
        private void OnBladeEnterWater()
        {
            if (splashParticles == null) return;

            float entrySpeed = _kayakRb != null
                ? _kayakRb.GetPointVelocity(transform.position).magnitude
                : 0f;

            if (entrySpeed >= splashSpeedThreshold)
            {
                splashParticles.transform.position = transform.position;
                splashParticles.Play();
            }
        }

        private void HandleWakeVFX(bool active)
        {
            if (wakeTrailParticles == null) return;
            if (active  && !wakeTrailParticles.isPlaying) wakeTrailParticles.Play();
            if (!active && wakeTrailParticles.isPlaying)  wakeTrailParticles.Stop();
        }
    }
}
