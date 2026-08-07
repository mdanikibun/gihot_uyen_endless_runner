using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    const string playerTag = "Player";

    void Update() {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(playerTag)) {
            OnPickup();
            Destroy(gameObject);
        }
    }

    protected abstract void OnPickup();
}
