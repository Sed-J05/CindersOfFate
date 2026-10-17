using System.Collections.Generic;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>Optional callbacks for pooled components.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    /// <summary>
    /// Generic prefab pool (Docs/02 §4). Instances are deactivated when free and re-parented under
    /// <c>parent</c>. Get/Release do not allocate after prewarm.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> free = new Stack<T>();
        private readonly List<T> active = new List<T>();

        public int CountFree => free.Count;
        public int CountActive => active.Count;
        public T Prefab => prefab;

        public ObjectPool(T prefab, Transform parent = null, int prewarm = 0)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < prewarm; i++) free.Push(Create());
        }

        private T Create()
        {
            T item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            return item;
        }

        public T Get()
        {
            T item = free.Count > 0 ? free.Pop() : Create();
            if (item == null) item = Create(); // destroyed externally while pooled
            item.gameObject.SetActive(true);
            active.Add(item);
            if (item is IPoolable p) p.OnSpawned();
            return item;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T item = free.Count > 0 ? free.Pop() : Create();
            if (item == null) item = Create();
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            active.Add(item);
            if (item is IPoolable p) p.OnSpawned();
            return item;
        }

        public T Get(Vector3 position) => Get(position, Quaternion.identity);

        public void Release(T item)
        {
            if (item == null) return;
            if (!active.Remove(item)) return; // not ours or already released
            if (item is IPoolable p) p.OnDespawned();
            item.gameObject.SetActive(false);
            if (parent != null) item.transform.SetParent(parent, false);
            free.Push(item);
        }

        /// <summary>Returns every active instance to the pool (room change, death reset).</summary>
        public void ReleaseAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                T item = active[i];
                active.RemoveAt(i);
                if (item == null) continue;
                if (item is IPoolable p) p.OnDespawned();
                item.gameObject.SetActive(false);
                if (parent != null) item.transform.SetParent(parent, false);
                free.Push(item);
            }
        }

        /// <summary>Destroys every instance. Use when the owning scene unloads.</summary>
        public void Clear()
        {
            ReleaseAll();
            while (free.Count > 0)
            {
                T item = free.Pop();
                if (item != null) Object.Destroy(item.gameObject);
            }
        }
    }
}
