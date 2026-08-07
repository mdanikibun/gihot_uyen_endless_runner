using UnityEngine;

public class Pickup : MonoBehaviour
{
    const string playerTag = "Player";

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(playerTag)) {
            // Debug.Log("Player picked up " + gameObject.name);
        }
    }
}
