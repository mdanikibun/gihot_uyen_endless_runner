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
    const string animRunSpeed = "RunAnimSpeed";
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

    void OnEnable() {
        GameEvents.OnDistanceChanged += HandleDistanceChanged;
        GameEvents.OnSpeedUpCountdownChanged += HandleSpeedUpCountdownChanged;
        GameEvents.OnHealthChanged += HandleHealthChanged;
        GameEvents.OnRunPrepared += HandleRunPrepared;
    }

    void OnDisable() {
        GameEvents.OnDistanceChanged -= HandleDistanceChanged;
        GameEvents.OnSpeedUpCountdownChanged -= HandleSpeedUpCountdownChanged;
        GameEvents.OnHealthChanged -= HandleHealthChanged;
        GameEvents.OnRunPrepared -= HandleRunPrepared;
    }

    void Start() {
        Time.timeScale = 1f;
        GameEvents.RaiseHealthChanged(health, startingHealth);
        SetGameOverButtonsVisible(false);
    }

    public void PrepareForNewRun() {
        StopAllCoroutines();

        isGameOver = false;
        health = startingHealth;
        distanceValue = 0f;
        Time.timeScale = 1f;

        playerController.enabled = true;
        playerController.ResetToStartPosition();
        ResetPlayerAnimatorToRun();
        scoreManager.ResetScore();

        GameEvents.RaiseRunPrepared();
        GameEvents.RaiseHealthChanged(health, startingHealth);
        GameEvents.RaiseDistanceChanged(0f);
        GameEvents.RaiseSpeedUpCountdownChanged(0f);
    }

    void ResetPlayerAnimatorToRun() {
        if (playerAnimator == null) return;
        if (!playerAnimator.isActiveAndEnabled) return;
        if (playerAnimator.runtimeAnimatorController == null) return;

        playerAnimator.ResetTrigger(animDie);
        playerAnimator.ResetTrigger(animHit);
        playerAnimator.ResetTrigger(animJump);
        playerAnimator.SetFloat(animRunSpeed, 1f);
        playerAnimator.Play(animRun, 0, 0f);
    }

    public void TakeDamage() {
        if (isGameOver) return;

        health -= settings.gameplay.damageAmount;
        GameEvents.RaiseHealthChanged(health, startingHealth);
        if (health <= 0) {
            GameOver();
        }
    }

    void HandleDistanceChanged(float distance) {
        if (isGameOver) return;

        distanceValue = Mathf.Round(distance);
        distanceText.text = "Distance: " + distanceValue + "m";
    }

    void HandleSpeedUpCountdownChanged(float countdown) {
        if (isGameOver) return;

        if (countdown <= 0f) {
            speedUpCountdownText.SetActive(false);
            return;
        }

        speedUpCountdownText.SetActive(true);
        speedUpCountdownText.GetComponent<TMP_Text>().text = "Speed Up: " + countdown.ToString("F1") + "s";
    }

    void HandleHealthChanged(int currentHealth, int maxHealth) {
        healthHearts.SetHealth(currentHealth, maxHealth);
    }

    void HandleRunPrepared() {
        gameOverText.SetActive(false);
        SetGameOverButtonsVisible(false);
    }

    void GameOver() {
        if (isGameOver) return;
        isGameOver = true;

        scoreManager.AddLeaderboardEntry(gameFlow.PlayerName, distanceValue, scoreManager.Score);

        playerController.enabled = false;
        GameEvents.RaiseGameOver();
        StartCoroutine(HandleBeforeGameOver());
    }

    IEnumerator HandleBeforeGameOver() {
        if (playerAnimator != null) {
            playerAnimator.ResetTrigger(animJump);
            playerAnimator.ResetTrigger(animHit);
            playerAnimator.SetFloat(animRunSpeed, 1f);
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
