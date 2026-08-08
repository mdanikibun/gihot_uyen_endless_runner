using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 movement;
    Rigidbody rb;
    Collider[] playerColliders;
    
    [Header("References")]
    [SerializeField] Animator animator;

    [Header("Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float jumpForce = 0f;
    [SerializeField] float groundCheckDistance = 0.1f;

    const string animJump = "Jump";
    const string animStumbleState = "Stumble";
    
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

    public void Jump(InputAction.CallbackContext context) {
        if (!context.started) return;
        if (!IsGrounded()) return;
        if (IsHitPlaying()) return;

        animator.SetTrigger(animJump);

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }
    
    bool IsHitPlaying() {
        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        if (current.IsName(animStumbleState)) return true;

        if (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(animStumbleState)) {
            return true;
        }

        return false;
    }

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
