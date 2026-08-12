using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameSettings settings;

    const string playerTag = "Player";
    Transform visualRoot;
    Vector3 localRotationCenter;
    Vector3 centerInRootLocal;
    Vector3 initialLocalPosition;
    Quaternion initialLocalRotation;

    void Awake() {
        visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
        CacheRotationCenter();

        initialLocalPosition = visualRoot.localPosition;
        initialLocalRotation = visualRoot.localRotation;
        centerInRootLocal = transform.InverseTransformPoint(visualRoot.TransformPoint(localRotationCenter));
    }

    void OnEnable() {
        visualRoot.localPosition = initialLocalPosition;
        visualRoot.localRotation = initialLocalRotation;
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
        float angle = settings.pickup.rotationSpeed * Time.time;
        Quaternion spin = Quaternion.AngleAxis(angle, Vector3.up);

        visualRoot.localRotation = spin * initialLocalRotation;

        Vector3 currentCenterLocal = transform.InverseTransformPoint(visualRoot.TransformPoint(localRotationCenter));
        visualRoot.localPosition += centerInRootLocal - currentCenterLocal;
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(playerTag)) {
            OnPickup();

            PooledObject pooled = GetComponent<PooledObject>();
            if (pooled != null) {
                pooled.ReturnToPool();
                return;
            }

            Destroy(gameObject);
        }
    }

    protected abstract void OnPickup();
}
