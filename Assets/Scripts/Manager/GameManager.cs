using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text distanceText;
    [SerializeField] GameObject speedUpCountdownText;
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
    float distanceValue = 0f;

    public bool IsGameOver => isGameOver;
    public float DistanceValue => distanceValue;

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

    public void UpdateDistanceText(float distance) {
        if (isGameOver) return;
        
        distanceValue = Mathf.Round(distance);
        distanceText.text = distanceValue + "m";
    }
    public void UpdateSpeedUpCountdownText(float countdown) {
        if (isGameOver) return;

        speedUpCountdownText.SetActive(true);
        speedUpCountdownText.GetComponent<TMP_Text>().text = "Speed Up: " + countdown.ToString("F1") + "s";
        if (countdown <= 0f) {
            speedUpCountdownText.SetActive(false);
        }
    }

    void GameOver() {
        if (isGameOver) return;
        isGameOver = true;

        playerController.enabled = false;
        StartCoroutine(HandleBeforeGameOver());
    }

    IEnumerator HandleBeforeGameOver() {
        if (playerAnimator != null) {
            playerAnimator.ResetTrigger(animJump);
            playerAnimator.ResetTrigger(animHit);
            playerAnimator.SetTrigger(animDie);
        }
        yield return new WaitForSecondsRealtime(.25f);
        gameOverText.SetActive(true);
        Time.timeScale = 0.1f;
        StartCoroutine(FreezeAfterSlowMo());
    }

    IEnumerator FreezeAfterSlowMo() {
        yield return new WaitForSecondsRealtime(gameOverSlowMoDuration);
        Time.timeScale = 0f;
    }
}