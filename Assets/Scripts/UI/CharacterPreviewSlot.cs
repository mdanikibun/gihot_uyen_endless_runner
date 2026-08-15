using UnityEngine;

public class CharacterPreviewSlot : MonoBehaviour
{
    Transform rotateRoot;
    Quaternion defaultRotation;
    float rotateSpeed;
    bool selected;

    public void Setup(Transform modelRoot, float speed) {
        rotateRoot = modelRoot;
        defaultRotation = modelRoot.localRotation;
        rotateSpeed = speed;
        selected = false;
    }

    public void SetSelected(bool isSelected) {
        selected = isSelected;
        if (!selected && rotateRoot != null) {
            rotateRoot.localRotation = defaultRotation;
        }
    }

    void Update() {
        if (!selected || rotateRoot == null) return;
        rotateRoot.Rotate(Vector3.up, rotateSpeed * Time.unscaledDeltaTime, Space.Self);
    }
}
