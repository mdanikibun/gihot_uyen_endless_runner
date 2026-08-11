using UnityEngine;
using UnityEngine.InputSystem;

public class GameFlowController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject panelMainMenu;
    [SerializeField] GameObject panelCharacterSelect;
    [SerializeField] GameObject panelHowToPlay;
    [SerializeField] GameObject panelPause;
    [SerializeField] LeaderboardUI leaderboardUI;

    [Header("Gameplay")]
    [SerializeField] GameObject canvasMenus;
    [SerializeField] GameObject canvasHud;
    [SerializeField] GameObject player;
    [SerializeField] LevelGenerator levelGenerator;
    [SerializeField] GameManager gameManager;
    [SerializeField] CharacterSelectUI characterSelectUI;
    [SerializeField] ObstacleSpawner obstacleSpawner;

    [Header("States")]
    [SerializeField] GameFlowState currentState = GameFlowState.MainMenu;

    string playerName = "Player";
    int selectedCharacterIndex = -1;
    CharacterOption selectedCharacter;
    bool leaderboardOpenedFromPause;

    public GameFlowState CurrentState => currentState;
    public string PlayerName => playerName;
    public int SelectedCharacterIndex => selectedCharacterIndex;

    void Start() {
        EnterMainMenu();
    }

    void Update() {
        HandlePauseInput();
    }

    public void ShowCharacterSelect() {
        SetGameplayVisible(false);
        ShowOnlyPanel(panelCharacterSelect);
        currentState = GameFlowState.CharacterSelect;
        Time.timeScale = 1f;
    }

    public void ShowHowToPlay() {
        ShowOnlyPanel(panelHowToPlay);
        currentState = GameFlowState.HowToPlay;
    }

    public void StartRun() {
        HideLeaderboard();
        SetMenusVisible(false);
        SetGameplayVisible(true);

        gameManager.PrepareForNewRun();
        levelGenerator.ResetForNewRun();
        ClearObstaclesForNewRun();

        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;
    }

    public void RestartRun() {
        HideLeaderboard();
        SetMenusVisible(false);
        SetGameplayVisible(true);

        gameManager.PrepareForNewRun();
        levelGenerator.ResetForNewRun();
        ClearObstaclesForNewRun();

        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;
    }

    void ClearObstaclesForNewRun() {
        obstacleSpawner.ClearAndRestartObstacles();
    }

    public void PauseGame() {
        if (currentState != GameFlowState.Playing) return;
        if (gameManager.IsGameOver) return;

        ShowOnlyPanel(panelPause);
        canvasMenus.SetActive(true);

        currentState = GameFlowState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame() {
        if (currentState != GameFlowState.Paused) return;

        HideLeaderboard();
        SetMenusVisible(false);
        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;
    }

    public void BackToMainMenu() {
        HideLeaderboard();
        EnterMainMenu();
    }

    public void ShowLeaderboard() {
        leaderboardOpenedFromPause = currentState == GameFlowState.Paused;

        leaderboardUI.Show();
    }

    public void HideLeaderboard() {
        leaderboardUI.Hide();

        if (leaderboardOpenedFromPause && currentState == GameFlowState.Paused) {
            panelPause.SetActive(true);
        }

        leaderboardOpenedFromPause = false;
    }

    public void QuitGame() {
        Debug.Log("Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetPlayerName(string name) {
        playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
    }

    public void SetSelectedCharacter(int index, CharacterOption option) {
        selectedCharacterIndex = index;
        selectedCharacter = option;
    }

    void EnterMainMenu() {
        SetGameplayVisible(false);
        SetMenusVisible(true);
        ShowOnlyPanel(panelMainMenu);

        gameManager.PrepareForNewRun();
        levelGenerator.EnterDemoMode();

        currentState = GameFlowState.MainMenu;
        Time.timeScale = 1f;
    }

    void HandlePauseInput() {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (gameManager.IsGameOver) return;

        if (currentState == GameFlowState.Playing) {
            PauseGame();
        } else if (currentState == GameFlowState.Paused) {
            if (leaderboardUI.gameObject.activeSelf) {
                HideLeaderboard();
            } else {
                ResumeGame();
            }
        }
    }

    void ShowOnlyPanel(GameObject panelToShow) {
        SetPanelActive(panelMainMenu, panelToShow == panelMainMenu);
        SetPanelActive(panelCharacterSelect, panelToShow == panelCharacterSelect);
        SetPanelActive(panelHowToPlay, panelToShow == panelHowToPlay);
        SetPanelActive(panelPause, panelToShow == panelPause);

        if (panelToShow != leaderboardUI.gameObject) {
            leaderboardUI.Hide();
        }
    }

    void SetPanelActive(GameObject panel, bool active) {
        panel.SetActive(active);
    }

    void SetMenusVisible(bool visible) {
        canvasMenus.SetActive(visible);
    }

    void SetGameplayVisible(bool visible) {
        canvasHud.SetActive(visible);
        player.SetActive(visible);
    }
}
