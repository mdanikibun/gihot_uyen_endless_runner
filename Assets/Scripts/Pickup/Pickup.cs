using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    const string playerTag = "Player";
    Transform visualRoot;
    Vector3 localRotationCenter;

    void Awake() {
        visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
        CacheRotationCenter();
    }

    void CacheRotationCenter() {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            localRotationCenter = Vector3.zero;
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }

        localRotationCenter = visualRoot.InverseTransformPoint(bounds.center);
    }

    void Update() {
        Vector3 worldCenter = visualRoot.TransformPoint(localRotationCenter);
        visualRoot.RotateAround(worldCenter, Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(playerTag)) {
            OnPickup();
            Destroy(gameObject);
        }
    }

    protected abstract void OnPickup();
}
