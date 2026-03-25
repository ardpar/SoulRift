using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SoulRift.Core
{
    /// <summary>
    /// Prefab bazli object pool yoneticisi. Unity ObjectPool&lt;T&gt; uzerine kurulu.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Dictionary<int, ObjectPool<GameObject>> _pools = new();
        private readonly Dictionary<int, Transform> _poolParents = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            int key = prefab.GetInstanceID();

            if (!_pools.ContainsKey(key))
                CreatePool(prefab);

            var obj = _pools[key].Get();
            obj.transform.SetPositionAndRotation(position, rotation);

            if (obj.TryGetComponent(out IPoolable poolable))
                poolable.OnSpawn();

            return obj;
        }

        public void Release(GameObject obj, GameObject prefab)
        {
            int key = prefab.GetInstanceID();

            if (obj.TryGetComponent(out IPoolable poolable))
                poolable.OnDespawn();

            if (_pools.ContainsKey(key))
                _pools[key].Release(obj);
            else
                Destroy(obj);
        }

        public void Warmup(GameObject prefab, int count)
        {
            int key = prefab.GetInstanceID();

            if (!_pools.ContainsKey(key))
                CreatePool(prefab);

            var temp = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
                temp.Add(_pools[key].Get());

            foreach (var obj in temp)
                _pools[key].Release(obj);
        }

        private void CreatePool(GameObject prefab)
        {
            int key = prefab.GetInstanceID();

            var parent = new GameObject($"Pool_{prefab.name}").transform;
            parent.SetParent(transform);
            _poolParents[key] = parent;

            _pools[key] = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var obj = Instantiate(prefab, parent);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy,
                defaultCapacity: 20,
                maxSize: 200
            );
        }
    }
}
