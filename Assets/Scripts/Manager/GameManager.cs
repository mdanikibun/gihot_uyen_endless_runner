using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] HealthHeartsUI healthHearts;
    [SerializeField] TMP_Text distanceText;
    [SerializeField] GameObject speedUpCountdownText;
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject gameOverButtonsRoot;
    [SerializeField] Button restartButton;
    [SerializeField] Button leaderboardButton;
    [SerializeField] Button mainMenuButton;
    [SerializeField] Animator playerAnimator;
    [SerializeField] PlayerController playerController;
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameSettings settings;

    const string animDie = "Die";
    const string animHit = "Hit";
    const string animJump = "Jump";
    const string animRun = "Root|Run";
    bool isGameOver = false;
    float distanceValue = 0f;
    int health;
    int startingHealth;

    public bool IsGameOver => isGameOver;
    public float DistanceValue => distanceValue;

    void Awake() {
        startingHealth = settings.gameplay.startingHealth;
        health = startingHealth;
        EnsureGameOverButtons();
    }

    void Start() {
        Time.timeScale = 1f;
        healthHearts.SetHealth(health, startingHealth);
        SetGameOverButtonsVisible(false);
    }

    public void PrepareForNewRun() {
        StopAllCoroutines();

        isGameOver = false;
        health = startingHealth;
        distanceValue = 0f;
        Time.timeScale = 1f;

        playerController.enabled = true;
        gameOverText.SetActive(false);
        SetGameOverButtonsVisible(false);
        healthHearts.SetHealth(health, startingHealth);
        distanceText.text = "Distance: 0m";
        speedUpCountdownText.SetActive(false);
        ResetPlayerAnimatorToRun();

        scoreManager.ResetScore();
    }

    void ResetPlayerAnimatorToRun() {
        if (playerAnimator == null) return;
        if (!playerAnimator.isActiveAndEnabled) return;
        if (playerAnimator.runtimeAnimatorController == null) return;

        playerAnimator.ResetTrigger(animDie);
        playerAnimator.ResetTrigger(animHit);
        playerAnimator.ResetTrigger(animJump);
        playerAnimator.Play(animRun, 0, 0f);
    }

    public void TakeDamage() {
        if (isGameOver) return;
        
        health -= settings.gameplay.damageAmount;
        healthHearts.SetHealth(health, startingHealth);
        if (health <= 0) {
            GameOver();
        }
    }

    public void UpdateDistanceText(float distance) {
        if (isGameOver) return;
        
        distanceValue = Mathf.Round(distance);
        distanceText.text = "Distance: " + distanceValue + "m";
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

        scoreManager.AddLeaderboardEntry(gameFlow.PlayerName, distanceValue, scoreManager.Score);

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
        yield return new WaitForSecondsRealtime(settings.gameplay.gameOverSlowMoDuration);
        Time.timeScale = 0f;
        SetGameOverButtonsVisible(true);
    }

    void EnsureGameOverButtons() {
        SetGameOverButtonsVisible(false);
        restartButton.onClick.AddListener(OnRestartClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    void SetGameOverButtonsVisible(bool visible) {
        gameOverButtonsRoot.SetActive(visible);
    }

    void OnRestartClicked() {
        gameFlow.RestartRun();
    }

    void OnLeaderboardClicked() {
        gameFlow.ShowLeaderboard();
    }

    void OnMainMenuClicked() {
        gameFlow.BackToMainMenu();
    }
}
