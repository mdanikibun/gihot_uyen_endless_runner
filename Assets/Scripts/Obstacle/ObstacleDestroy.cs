using UnityEngine;

public class ObstacleDestroyer : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        PooledObject pooled = other.GetComponentInParent<PooledObject>(true);
        if (pooled != null) {
            pooled.ReturnToPool();
            return;
        }

        if (other.gameObject.activeInHierarchy) {
            Destroy(other.gameObject);
        }
    }
}
