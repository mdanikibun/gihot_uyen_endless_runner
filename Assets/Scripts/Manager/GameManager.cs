using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text healthText;
    [SerializeField] GameObject gameOverText;
    [SerializeField] Animator playerAnimator;
    [SerializeField] PlayerController playerController;

    [Header("Settings")]
    [SerializeField] int health = 5;
    [SerializeField] int damageAmount = 1;
    [SerializeField] float gameOverSlowMoDuration = 10f;

    const string animDie = "Die";
    const string animHit = "Hit";
    const string animJump = "Jump";
    bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    void Start() {
        Time.timeScale = 1f;
        healthText.text = health.ToString();
    }

    public void TakeDamage() {
        if (isGameOver) return;
        
        health -= damageAmount;
        healthText.text = health.ToString();
        if (health <= 0) {
            GameOver();
        }
    }

    void GameOver() {
        if (isGameOver) return;
        isGameOver = true;

        playerController.enabled = false;
        gameOverText.SetActive(true);

        if (playerAnimator != null) {
            playerAnimator.ResetTrigger(animJump);
            playerAnimator.ResetTrigger(animHit);
            playerAnimator.SetTrigger(animDie);
        }

        Time.timeScale = 0.1f;
        StartCoroutine(FreezeAfterSlowMo());
    }

    IEnumerator FreezeAfterSlowMo() {
        yield return new WaitForSecondsRealtime(gameOverSlowMoDuration);
        Time.timeScale = 0f;
    }
}