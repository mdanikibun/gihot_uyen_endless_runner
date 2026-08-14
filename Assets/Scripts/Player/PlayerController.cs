using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 movement;
    Rigidbody rb;
    Collider[] playerColliders;

    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] GameSettings settings;

    const string animJump = "Jump";
    const string animStumbleState = "Stumble";
    const float wallNormalMaxY = 0.4f;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        playerColliders = GetComponentsInChildren<Collider>();
        ApplyNoWallFrictionMaterial();
    }

    public void ResetToStartPosition() {
        if (rb == null) return;
        Vector3 startPosition = settings.player.startPosition;
        transform.SetPositionAndRotation(startPosition, Quaternion.identity);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate() {
        HandleMovement();
    }

    public void Move(InputAction.CallbackContext context) {
        movement = context.ReadValue<Vector2>();
    }

    void HandleMovement() {
        float moveX = movement.x * settings.player.speedMoveLeftRight;
        float moveZ = movement.y * settings.player.speedMoveLeftRight;

        if (!IsGrounded() && IsPushingIntoWall(moveX, out RaycastHit wallHit)) {
            moveX = 0f;
            Vector3 velocity = rb.linearVelocity;
            float intoWall = Vector3.Dot(velocity, -wallHit.normal);
            if (intoWall > 0f) {
                velocity += wallHit.normal * intoWall;
            }
            rb.linearVelocity = new Vector3(0f, velocity.y, moveZ);
            return;
        }

        rb.linearVelocity = new Vector3(moveX, rb.linearVelocity.y, moveZ);
    }

    public void Jump(InputAction.CallbackContext context) {
        if (!context.started) return;
        if (!IsGrounded()) return;
        if (IsHitPlaying()) return;

        animator.SetTrigger(animJump);

        Vector3 velocity = rb.linearVelocity;
        velocity.y = settings.player.jumpForce;
        rb.linearVelocity = velocity;
        GameEvents.RaisePlayerJumpSFX();
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
        if (!TryGetPlayerBounds(out Bounds bounds)) return false;

        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + 0.05f, bounds.center.z);
        float distance = settings.player.groundCheckDistance + 0.05f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore)) {
            return false;
        }

        return !IsOwnCollider(hit.collider);
    }

    bool IsPushingIntoWall(float moveX, out RaycastHit wallHit) {
        wallHit = default;
        if (Mathf.Abs(moveX) < 0.01f) return false;
        if (!TryGetPlayerBounds(out Bounds bounds)) return false;

        Vector3 direction = moveX > 0f ? Vector3.right : Vector3.left;
        float distance = bounds.extents.x + settings.player.wallCheckDistance;
        Vector3 origin = bounds.center;

        if (!Physics.Raycast(origin, direction, out wallHit, distance, ~0, QueryTriggerInteraction.Ignore)) {
            return false;
        }

        if (IsOwnCollider(wallHit.collider)) return false;

        return Mathf.Abs(wallHit.normal.y) < wallNormalMaxY;
    }

    bool TryGetPlayerBounds(out Bounds bounds) {
        bounds = default;
        if (playerColliders.Length == 0) return false;

        bounds = playerColliders[0].bounds;
        for (int i = 1; i < playerColliders.Length; i++) {
            bounds.Encapsulate(playerColliders[i].bounds);
        }

        return true;
    }

    bool IsOwnCollider(Collider other) {
        for (int i = 0; i < playerColliders.Length; i++) {
            if (playerColliders[i] == other) return true;
        }
        return false;
    }

    void ApplyNoWallFrictionMaterial() {
        PhysicsMaterial noFriction = new PhysicsMaterial("PlayerNoFriction") {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };

        for (int i = 0; i < playerColliders.Length; i++) {
            playerColliders[i].material = noFriction;
        }
    }
}
