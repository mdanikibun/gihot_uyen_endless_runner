using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] GameSettings settings;

    float cooldownTimer = 0f;
    const string animHit = "Hit";
    const string animJump = "Jump";

    LevelGenerator levelGenerator;
    GameManager gameManager;

    void Awake() {
        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start() {
        levelGenerator = FindAnyObjectByType<LevelGenerator>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update() {
        cooldownTimer += Time.deltaTime;
    }
    
    void OnCollisionEnter(Collision other) {
        if (cooldownTimer < settings.playerCollision.collisionCooldown) return;

        foreach (GameObject obstaclePrefab in obstaclePrefabs) {
            if (other.gameObject.name.Contains(obstaclePrefab.name)) {
                animator.ResetTrigger(animJump);
                animator.SetTrigger(animHit);
                cooldownTimer = 0f;
                levelGenerator.ChangeSegmentMoveSpeed(settings.playerCollision.speedChangeAmount);
                gameManager.TakeDamage();
                GameEvents.RaisePlayerHitSFX();
                return;
            }
        }
    }
}
