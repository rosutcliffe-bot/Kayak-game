using UnityEngine;
using KayakSimulator.Physics;

namespace KayakSimulator.Water
{
    /// <summary>
    /// Spawns and manages water VFX triggered by objects interacting with the surface:
    ///   • Splash particles when an object enters the water at speed.
    ///   • Wake trail following objects moving through the water.
    ///   • Ripple decals expanding outward from impact points.
    ///
    /// Uses an object pool for all particle systems to avoid GC allocations.
    /// </summary>
    public class WaterEffects : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Singleton
        // ---------------------------------------------------------------
        public static WaterEffects Instance { get; private set; }

        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Prefabs")]
        [SerializeField] private ParticleSystem splashPrefab;
        [SerializeField] private ParticleSystem ripplePrefab;

        [Header("Splash Thresholds")]
        [Tooltip("Minimum entry speed (m/s) to spawn a splash.")]
        [SerializeField] private float splashMinSpeed = 0.5f;

        [Tooltip("Entry speed that produces maximum splash intensity.")]
        [SerializeField] private float splashMaxSpeed = 8f;

        [Header("Pool Size")]
        [SerializeField] private int splashPoolSize = 8;
        [SerializeField] private int ripplePoolSize = 16;

        // ---------------------------------------------------------------
        // Object pools
        // ---------------------------------------------------------------
        private ParticleSystem[] _splashPool;
        private ParticleSystem[] _ripplePool;
        private int              _splashIndex;
        private int              _rippleIndex;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            BuildPool(ref _splashPool, splashPrefab, splashPoolSize, "SplashPool");
            BuildPool(ref _ripplePool, ripplePrefab, ripplePoolSize, "RipplePool");
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>
        /// Spawn a splash at <paramref name="worldPos"/> sized by <paramref name="entrySpeed"/>.
        /// Call this when an object enters the water.
        /// </summary>
        public void SpawnSplash(Vector3 worldPos, float entrySpeed)
        {
            if (splashPrefab == null) return;
            float t = Mathf.InverseLerp(splashMinSpeed, splashMaxSpeed, entrySpeed);
            if (t <= 0f) return;

            ParticleSystem ps = GetPooled(ref _splashPool, ref _splashIndex);
            if (ps == null) return;

            ps.transform.position = worldPos;

            var main = ps.main;
            main.startSpeedMultiplier = Mathf.Lerp(1f, 4f, t);
            main.startSizeMultiplier  = Mathf.Lerp(0.3f, 1.5f, t);

            ps.gameObject.SetActive(true);
            ps.Play();
        }

        /// <summary>
        /// Spawn a radial ripple decal at <paramref name="worldPos"/>.
        /// </summary>
        public void SpawnRipple(Vector3 worldPos)
        {
            if (ripplePrefab == null) return;

            ParticleSystem ps = GetPooled(ref _ripplePool, ref _rippleIndex);
            if (ps == null) return;

            ps.transform.position = new Vector3(worldPos.x, worldPos.y + 0.02f, worldPos.z);
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        // ---------------------------------------------------------------
        // Pool helpers
        // ---------------------------------------------------------------
        private void BuildPool(ref ParticleSystem[] pool, ParticleSystem prefab, int size, string holderName)
        {
            pool = new ParticleSystem[size];
            if (prefab == null) return;

            GameObject holder = new GameObject(holderName);
            holder.transform.SetParent(transform);

            for (int i = 0; i < size; i++)
            {
                ParticleSystem ps = Instantiate(prefab, holder.transform);
                ps.Stop();
                ps.gameObject.SetActive(false);
                pool[i] = ps;
            }
        }

        private static ParticleSystem GetPooled(ref ParticleSystem[] pool, ref int index)
        {
            if (pool == null || pool.Length == 0) return null;

            // Advance to next slot (round-robin)
            index = (index + 1) % pool.Length;
            var ps = pool[index];
            if (ps == null) return null;

            // Recycle: stop current playback before reuse
            if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            return ps;
        }
    }
}
