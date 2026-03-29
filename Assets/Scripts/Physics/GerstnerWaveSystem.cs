using System.Collections.Generic;
using UnityEngine;

namespace KayakSimulator.Physics
{
    /// <summary>
    /// Gerstner wave simulation system.
    ///
    /// Gerstner waves (also known as trochoidal waves) are a physically based
    /// approximation of ocean surface geometry. Each wave is defined by:
    ///   - Amplitude  A  (metres)
    ///   - Wavelength λ  (metres)  → wave number k = 2π / λ
    ///   - Speed      c  (m/s)     → angular frequency ω = sqrt(g·k)
    ///   - Direction  D  (unit XZ) → steepness Q controls sharpening
    ///
    /// The CPU-side calculation keeps physics in sync with the GPU shader.
    /// </summary>
    public class GerstnerWaveSystem : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Wave descriptor
        // ---------------------------------------------------------------
        [System.Serializable]
        public class WaveDescriptor
        {
            [Tooltip("Wave travel direction (XZ, normalised at runtime).")]
            public Vector2 direction = new Vector2(1f, 0f);

            [Tooltip("Wave amplitude in metres.")]
            [Range(0.01f, 5f)]
            public float amplitude = 0.5f;

            [Tooltip("Wavelength in metres.")]
            [Range(1f, 200f)]
            public float wavelength = 20f;

            [Tooltip("Wave speed in metres per second.")]
            [Range(0.1f, 20f)]
            public float speed = 3f;

            [Tooltip("Steepness (Qi) controls peak sharpening. 0 = sinusoidal, higher = sharper crests.")]
            [Range(0f, 1f)]
            public float steepness = 0.5f;
        }

        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Wave Configuration")]
        [SerializeField] private List<WaveDescriptor> waves = new List<WaveDescriptor>
        {
            new WaveDescriptor { direction = new Vector2(1f,  0.3f), amplitude = 0.4f,  wavelength = 25f,  speed = 3f,  steepness = 0.5f },
            new WaveDescriptor { direction = new Vector2(0.8f, 1f),  amplitude = 0.25f, wavelength = 15f,  speed = 2.5f, steepness = 0.4f },
            new WaveDescriptor { direction = new Vector2(0.2f, 1f),  amplitude = 0.15f, wavelength = 8f,   speed = 1.5f, steepness = 0.3f },
            new WaveDescriptor { direction = new Vector2(1f, -0.5f), amplitude = 0.1f,  wavelength = 5f,   speed = 1f,   steepness = 0.2f },
        };

        [Header("Environment")]
        [SerializeField] private float gravity = 9.81f;

        [Header("Shader Sync")]
        [Tooltip("Water material to keep in sync with CPU wave data.")]
        [SerializeField] private Material waterMaterial;

        // ---------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------
        private static readonly int ShaderWaveParamsID = Shader.PropertyToID("_WaveParams");

        // Maximum waves the GPU shader supports (must match shader constant).
        private const int MaxShaderWaves = 4;

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>
        /// Returns the displaced world-space position of a water surface point
        /// at (x, z) for the current time, using all active Gerstner waves.
        /// </summary>
        public Vector3 GetWaveDisplacement(float x, float z)
        {
            float t = Time.time;
            Vector3 displacement = Vector3.zero;

            foreach (var wave in waves)
            {
                Vector2 dir    = wave.direction.normalized;
                float   k      = 2f * Mathf.PI / wave.wavelength;
                float   omega  = Mathf.Sqrt(gravity * k);
                float   phase  = k * (dir.x * x + dir.y * z) - omega * t;
                float   sinP   = Mathf.Sin(phase);
                float   cosP   = Mathf.Cos(phase);
                float   Qi     = wave.steepness / (k * wave.amplitude * waves.Count);

                displacement.x += Qi * wave.amplitude * dir.x * cosP;
                displacement.z += Qi * wave.amplitude * dir.y * cosP;
                displacement.y += wave.amplitude * sinP;
            }

            return new Vector3(x + displacement.x, displacement.y, z + displacement.z);
        }

        /// <summary>
        /// Returns only the Y height of the wave surface at world position (x, z).
        /// Fast path used by buoyancy sampling.
        /// </summary>
        public float GetWaveHeight(float x, float z)
        {
            return GetWaveDisplacement(x, z).y;
        }

        /// <summary>
        /// Returns the surface normal at (x, z) estimated via cross-product of
        /// finite-difference tangent vectors.
        /// </summary>
        public Vector3 GetSurfaceNormal(float x, float z, float epsilon = 0.1f)
        {
            Vector3 p0 = GetWaveDisplacement(x, z);
            Vector3 px = GetWaveDisplacement(x + epsilon, z);
            Vector3 pz = GetWaveDisplacement(x, z + epsilon);

            Vector3 tangentX = px - p0;
            Vector3 tangentZ = pz - p0;
            return Vector3.Cross(tangentZ, tangentX).normalized;
        }

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Update()
        {
            SyncShaderParameters();
        }

        // ---------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------

        /// <summary>
        /// Packs wave parameters into shader-compatible Vector4 arrays and
        /// uploads them to the water material each frame.
        /// </summary>
        private void SyncShaderParameters()
        {
            if (waterMaterial == null) return;

            // Pack up to MaxShaderWaves waves into Vector4 arrays
            // Layout per wave: x=dirX, y=dirZ, z=amplitude, w=wavelength
            // A second array carries speed and steepness.
            var packedA = new Vector4[MaxShaderWaves];
            var packedB = new Vector4[MaxShaderWaves];

            int count = Mathf.Min(waves.Count, MaxShaderWaves);
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = waves[i].direction.normalized;
                packedA[i] = new Vector4(dir.x, dir.y, waves[i].amplitude, waves[i].wavelength);
                packedB[i] = new Vector4(waves[i].speed, waves[i].steepness, 0f, 0f);
            }

            waterMaterial.SetVectorArray("_WaveA", packedA);
            waterMaterial.SetVectorArray("_WaveB", packedB);
            waterMaterial.SetInt("_WaveCount", count);
        }
    }
}
