using UnityEngine;

namespace KayakSimulator.Utilities
{
    /// <summary>
    /// Generic object pool for Unity components.
    /// Reduces GC pressure by pre-allocating a fixed number of instances
    /// and recycling them instead of Instantiate/Destroy.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T[]  _pool;
        private          int  _nextIndex;

        /// <summary>
        /// Creates a pool of <paramref name="size"/> instances of <paramref name="prefab"/>,
        /// parented under <paramref name="parent"/>.
        /// </summary>
        public ObjectPool(T prefab, int size, Transform parent = null)
        {
            _pool = new T[size];
            for (int i = 0; i < size; i++)
            {
                _pool[i] = Object.Instantiate(prefab, parent);
                _pool[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Returns the next available pooled instance (round-robin).
        /// The caller is responsible for activating the object and resetting its state.
        /// </summary>
        public T Get()
        {
            _nextIndex = (_nextIndex + 1) % _pool.Length;
            T instance = _pool[_nextIndex];
            instance.gameObject.SetActive(false); // ensure clean state
            return instance;
        }
    }
}
