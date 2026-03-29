using System.Collections.Generic;
using UnityEngine;

namespace KayakSimulator.Physics
{
    /// <summary>
    /// High-level water physics façade.
    ///
    /// Aggregates wave height queries, surface normals, current velocity,
    /// and turbulence into a single component that other systems (buoyancy,
    /// paddle interaction, VFX) can depend on.
    ///
    /// Designed so that the underlying wave model can be swapped (Gerstner
    /// → FFT) without changing any callers.
    /// </summary>
    public class WaterPhysics : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Singleton
        // ---------------------------------------------------------------
        public static WaterPhysics Instance { get; private set; }

        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Wave Model")]
        [Tooltip("Drag the GerstnerWaveSystem component here, or leave null to auto-find.")]
        [SerializeField] private GerstnerWaveSystem waveSystem;

        [Header("Global Current")]
        [SerializeField] private Vector3 currentVelocity = new Vector3(0.3f, 0f, 0f);

        [Header("Turbulence")]
        [Tooltip("Peak random force magnitude added to objects in water (N).")]
        [SerializeField] private float turbulenceStrength = 15f;

        [Tooltip("Frequency of turbulence noise (Hz).")]
        [SerializeField] private float turbulenceFrequency = 0.4f;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (waveSystem == null)
                waveSystem = FindAnyObjectByType<GerstnerWaveSystem>();
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>Y height of the animated wave surface at world (x, z).</summary>
        public float GetSurfaceHeight(float x, float z)
        {
            return waveSystem != null ? waveSystem.GetWaveHeight(x, z) : 0f;
        }

        /// <summary>World-space surface normal at (x, z).</summary>
        public Vector3 GetSurfaceNormal(float x, float z)
        {
            return waveSystem != null ? waveSystem.GetSurfaceNormal(x, z) : Vector3.up;
        }

        /// <summary>
        /// Returns the total water velocity (current + surface wave orbital velocity)
        /// at a world position.  Used by paddle interaction to compute drag forces.
        /// </summary>
        public Vector3 GetWaterVelocityAt(Vector3 worldPos)
        {
            // Stokes orbital velocity approximation: horizontal component proportional
            // to wave gradient, vertical proportional to wave height change.
            Vector3 orbital = Vector3.zero;
            if (waveSystem != null)
            {
                const float eps = 0.1f;
                float hc  = waveSystem.GetWaveHeight(worldPos.x,       worldPos.z);
                float hpx = waveSystem.GetWaveHeight(worldPos.x + eps, worldPos.z);
                float hpz = waveSystem.GetWaveHeight(worldPos.x,       worldPos.z + eps);

                // Gradient of height → approximate horizontal orbital
                orbital = new Vector3((hpx - hc) / eps, 0f, (hpz - hc) / eps) * 2f;
            }

            // Turbulence (Perlin-based, seeded by position and time)
            float t  = Time.time * turbulenceFrequency;
            float tx = Mathf.PerlinNoise(worldPos.x * 0.1f, t)       - 0.5f;
            float tz = Mathf.PerlinNoise(worldPos.z * 0.1f, t + 100f) - 0.5f;
            Vector3 turbulence = new Vector3(tx, 0f, tz) * turbulenceStrength;

            return currentVelocity + orbital + turbulence;
        }

        /// <summary>Global ocean current vector.</summary>
        public Vector3 CurrentVelocity => currentVelocity;
    }
}
