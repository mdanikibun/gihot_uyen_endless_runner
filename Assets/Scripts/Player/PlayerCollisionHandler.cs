using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject[] obstaclePrefabs;

    [Header("Settings")]
    [SerializeField] float collisionCooldown = 1f;
    [SerializeField] float speedChangeAmount = -1f;

    float cooldownTimer = 0f;
    const string animHit = "Hit";
    const string animJump = "Jump";

    LevelGenerator levelGenerator;
    GameManager gameManager;

    void Start() {
        levelGenerator = FindAnyObjectByType<LevelGenerator>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update() {
        cooldownTimer += Time.deltaTime;
    }
    
    void OnCollisionEnter(Collision other) {
        if (cooldownTimer < collisionCooldown) return;

        foreach (GameObject obstaclePrefab in obstaclePrefabs) {
            if (other.gameObject.name.Contains(obstaclePrefab.name)) {
                animator.ResetTrigger(animJump);
                animator.SetTrigger(animHit);
                cooldownTimer = 0f;
                levelGenerator.ChangeChunkMoveSpeed(speedChangeAmount);
                gameManager.TakeDamage();
            }
        }
    }
}
