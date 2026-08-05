using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 movement;
    Rigidbody rb;
    Collider[] playerColliders;

    [SerializeField] float speed = 10f;
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float groundCheckDistance = 0.1f;
    
    void Awake() {
        rb = GetComponent<Rigidbody>();
        playerColliders = GetComponentsInChildren<Collider>();
    }

    void FixedUpdate() {
        HandleMovement();
    }

    public void Move(InputAction.CallbackContext context) {
        movement = context.ReadValue<Vector2>();
    }

    void HandleMovement() {
        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.y * speed);
    }

    // TODO: xử lý trái phải trước, nhảy sẽ sử dụng sau
    public void Jump(InputAction.CallbackContext context) {
        return; // TODO: xử lý trái phải trước, nhảy sẽ sử dụng sau
        if (!context.started) return;
        if (!IsGrounded()) return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }
    
    // TODO: xử lý trái phải trước, nhảy sẽ sử dụng sau
    bool IsGrounded() {
        if (playerColliders == null || playerColliders.Length == 0) return false;

        Bounds bounds = playerColliders[0].bounds;
        for (int i = 1; i < playerColliders.Length; i++) {
            bounds.Encapsulate(playerColliders[i].bounds);
        }

        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + 0.05f, bounds.center.z);
        float distance = groundCheckDistance + 0.05f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore)) {
            return false;
        }

        for (int i = 0; i < playerColliders.Length; i++) {
            if (hit.collider == playerColliders[i]) return false;
        }

        return true;
    }
}
