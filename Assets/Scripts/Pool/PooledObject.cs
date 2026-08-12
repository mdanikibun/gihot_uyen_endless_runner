using UnityEngine;

public class PooledObject : MonoBehaviour
{
    ObjectPool pool;
    bool isInPool = true;

    public void SetPool(ObjectPool objectPool) {
        pool = objectPool;
    }

    public void MarkRented() {
        isInPool = false;
    }

    public void MarkInPool() {
        isInPool = true;
    }

    public void ReturnToPool() {
        if (isInPool) return;

        if (pool != null) {
            isInPool = true;
            pool.Return(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
