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
    GameObject scenePlayer;
    GameObject runtimePlayer;
    int runtimeCharacterIndex = -1;
    bool leaderboardOpenedFromPause;
    bool leaderboardOpenedWhileMenusHidden;

    public GameFlowState CurrentState => currentState;
    public string PlayerName => playerName;
    public int SelectedCharacterIndex => selectedCharacterIndex;

    void Awake() {
        scenePlayer = player;
    }

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
        ApplySelectedPlayer();
        HideLeaderboard();
        SetMenusVisible(false);
        SetGameplayVisible(true);

        gameManager.PrepareForNewRun();
        levelGenerator.ResetForNewRun();
        ClearObstaclesForNewRun();

        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;
        GameEvents.RaiseMusicGameplay();
    }

    public void RestartRun() {
        ApplySelectedPlayer();
        HideLeaderboard();
        SetMenusVisible(false);
        SetGameplayVisible(true);

        gameManager.PrepareForNewRun();
        levelGenerator.ResetForNewRun();
        ClearObstaclesForNewRun();

        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;
        GameEvents.RaiseMusicGameplay();
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
        GameEvents.RaiseMusicRelaxed();
    }

    public void ResumeGame() {
        if (currentState != GameFlowState.Paused) return;

        HideLeaderboard();
        SetMenusVisible(false);
        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;
        GameEvents.RaiseMusicGameplay();
    }

    public void BackToMainMenu() {
        HideLeaderboard();
        EnterMainMenu();
    }

    public void ShowLeaderboard() {
        leaderboardOpenedFromPause = currentState == GameFlowState.Paused;
        leaderboardOpenedWhileMenusHidden = !canvasMenus.activeSelf;

        if (leaderboardOpenedWhileMenusHidden) {
            SetMenusVisible(true);
            SetPanelActive(panelMainMenu, false);
            SetPanelActive(panelCharacterSelect, false);
            SetPanelActive(panelHowToPlay, false);
            SetPanelActive(panelPause, false);
        }

        leaderboardUI.Show();
    }

    public void HideLeaderboard() {
        leaderboardUI.Hide();

        if (leaderboardOpenedFromPause && currentState == GameFlowState.Paused) {
            panelPause.SetActive(true);
        }

        if (leaderboardOpenedWhileMenusHidden) {
            SetMenusVisible(false);
        }

        leaderboardOpenedFromPause = false;
        leaderboardOpenedWhileMenusHidden = false;
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

    void ApplySelectedPlayer() {
        GameObject prefab = selectedCharacter != null ? selectedCharacter.playerPrefab : null;
        if (prefab == null) return;

        if (runtimePlayer != null && runtimeCharacterIndex == selectedCharacterIndex) {
            player = runtimePlayer;
            BindPlayer(runtimePlayer);
            return;
        }

        if (runtimePlayer != null) {
            Destroy(runtimePlayer);
            runtimePlayer = null;
        }

        if (scenePlayer != null) {
            scenePlayer.SetActive(false);
        }

        runtimePlayer = Instantiate(prefab);
        runtimePlayer.name = prefab.name;
        runtimePlayer.SetActive(false);
        runtimeCharacterIndex = selectedCharacterIndex;
        CopyScenePlayerLight(runtimePlayer);
        BindPlayer(runtimePlayer);
        player = runtimePlayer;
    }

    void BindPlayer(GameObject playerInstance) {
        PlayerController controller = playerInstance.GetComponent<PlayerController>();
        Animator animator = playerInstance.GetComponentInChildren<Animator>();
        gameManager.BindPlayer(controller, animator);
        levelGenerator.BindPlayerAnimator(animator);
    }

    void CopyScenePlayerLight(GameObject playerInstance) {
        if (playerInstance.GetComponentInChildren<Light>(true) != null) return;
        if (scenePlayer == null) return;

        Light sceneLight = scenePlayer.GetComponentInChildren<Light>(true);
        if (sceneLight == null) return;

        GameObject lightClone = Instantiate(sceneLight.gameObject, playerInstance.transform);
        lightClone.name = sceneLight.gameObject.name;
        lightClone.transform.localPosition = sceneLight.transform.localPosition;
        lightClone.transform.localRotation = sceneLight.transform.localRotation;
    }

    void EnterMainMenu() {
        SetGameplayVisible(false);
        SetMenusVisible(true);
        ShowOnlyPanel(panelMainMenu);

        gameManager.PrepareForNewRun();
        levelGenerator.EnterDemoMode();

        currentState = GameFlowState.MainMenu;
        Time.timeScale = 1f;
        GameEvents.RaiseMusicReset();
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
        if (player != null) {
            player.SetActive(visible);
        }
    }
}
