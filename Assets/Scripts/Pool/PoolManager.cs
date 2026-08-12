using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public const int DefaultPoolSize = 10;

    public static PoolManager Instance { get; private set; }

    readonly Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

    public Transform PoolRoot => transform;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void EnsurePool(GameObject prefab, int poolSize) {
        if (prefab == null) return;
        if (pools.ContainsKey(prefab)) return;

        int size = Mathf.Max(1, poolSize);
        pools[prefab] = new ObjectPool(prefab, size, PoolRoot);
    }

    public GameObject GetInactive(GameObject prefab) {
        ObjectPool pool = GetOrCreatePool(prefab);
        return pool.GetInactive();
    }

    public void Return(GameObject obj) {
        if (obj == null) return;

        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled != null) {
            pooled.ReturnToPool();
            return;
        }

        Destroy(obj);
    }

    ObjectPool GetOrCreatePool(GameObject prefab) {
        if (prefab == null) {
            Debug.LogError("PoolManager.GetInactive called with null prefab.");
            return null;
        }

        if (!pools.TryGetValue(prefab, out ObjectPool pool)) {
            EnsurePool(prefab, DefaultPoolSize);
            pool = pools[prefab];
        }

        return pool;
    }
}
