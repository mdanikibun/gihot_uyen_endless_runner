using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    readonly GameObject prefab;
    readonly Transform parent;
    readonly Queue<GameObject> available = new Queue<GameObject>();
    readonly List<GameObject> all = new List<GameObject>();

    public int AvailableCount => available.Count;
    public int TotalCount => all.Count;

    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null) {
        this.prefab = prefab;
        this.parent = parent;

        int warmCount = Mathf.Max(0, initialSize);
        for (int i = 0; i < warmCount; i++) {
            CreateNewObject(true);
        }
    }

    public GameObject GetInactive() {
        GameObject obj = TakeAvailable();
        if (obj == null) {
            CleanupDestroyed();
            obj = CreateNewObject(false);
        }

        return Rent(obj);
    }

    public void Return(GameObject obj) {
        if (obj == null) return;

        if (parent != null && obj.transform.parent != parent) {
            obj.transform.SetParent(parent, false);
        }

        if (obj.activeSelf) {
            obj.SetActive(false);
        }

        available.Enqueue(obj);
    }

    GameObject TakeAvailable() {
        while (available.Count > 0) {
            GameObject obj = available.Dequeue();
            if (obj != null) {
                return obj;
            }
        }

        return null;
    }

    GameObject Rent(GameObject obj) {
        if (obj == null) {
            CleanupDestroyed();
            obj = CreateNewObject(false);
        }

        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled != null) {
            pooled.MarkRented();
        }

        if (obj.activeSelf) {
            obj.SetActive(false);
        }

        return obj;
    }

    void CleanupDestroyed() {
        for (int i = all.Count - 1; i >= 0; i--) {
            if (all[i] == null) {
                all.RemoveAt(i);
            }
        }
    }

    GameObject CreateNewObject(bool enqueue) {
        GameObject obj = Object.Instantiate(prefab, parent);
        obj.SetActive(false);

        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null) {
            pooled = obj.AddComponent<PooledObject>();
        }
        pooled.SetPool(this);

        all.Add(obj);
        if (enqueue) {
            available.Enqueue(obj);
            pooled.MarkInPool();
        }

        return obj;
    }
}
