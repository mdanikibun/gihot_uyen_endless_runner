using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] float collisionCooldown = 1f;
    float cooldownTimer = 0f;
    const string animHit = "Hit";

    void Update() {
        cooldownTimer += Time.deltaTime;
    }
    
    void OnCollisionEnter(Collision other) {
        if (cooldownTimer < collisionCooldown) return;

        foreach (GameObject obstaclePrefab in obstaclePrefabs) {
            if (other.gameObject.name.Contains(obstaclePrefab.name)) {
                animator.SetTrigger(animHit);
                cooldownTimer = 0f;
            }
        }
    }
}
