using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameFlowController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject panelLoading;
    [SerializeField] RectTransform loadingLabel;
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
    [SerializeField] GameAssetManager gameAssetManager;

    [Header("States")]
    [SerializeField] GameFlowState currentState = GameFlowState.Loading;

    string playerName = "Player";
    int selectedCharacterIndex = -1;
    GameObject runtimePlayer;
    int runtimeCharacterIndex = -1;
    bool leaderboardOpenedFromPause;
    bool leaderboardOpenedWhileMenusHidden;

    public GameFlowState CurrentState => currentState;
    public string PlayerName => playerName;
    public int SelectedCharacterIndex => selectedCharacterIndex;

    public PlayerController ActivePlayerController {
        get {
            return player != null ? player.GetComponent<PlayerController>() : null;
        }
    }

    void Start() {
        ShowLoading();
        StartCoroutine(LoadAssetsThenEnterMenu());
    }

    void Update() {
        HandlePauseInput();
    }

    public void ShowCharacterSelect() {
        SetGameplayVisible(false);
        ShowOnlyPanel(panelCharacterSelect);
        characterSelectUI.BuildPreviewsIfNeeded();
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
            SetPanelActive(panelLoading, false);
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

    public void SetSelectedCharacter(int index) {
        selectedCharacterIndex = index;
    }

    void ApplySelectedPlayer() {
        GameObject prefab = characterSelectUI.GetPlayerPrefab(selectedCharacterIndex);

        if (runtimePlayer != null && runtimeCharacterIndex == selectedCharacterIndex) {
            player = runtimePlayer;
            BindPlayer(runtimePlayer);
            return;
        }

        if (runtimePlayer != null) {
            Destroy(runtimePlayer);
            runtimePlayer = null;
        }

        Vector3 spawnPosition = new Vector3(0f, 1.2f, 0f);
        runtimePlayer = Instantiate(prefab, spawnPosition, Quaternion.identity);
        runtimePlayer.name = prefab.name;
        SetLayerRecursively(runtimePlayer, 0);
        runtimePlayer.SetActive(false);
        runtimeCharacterIndex = selectedCharacterIndex;
        BindPlayer(runtimePlayer);
        player = runtimePlayer;
    }

    void BindPlayer(GameObject playerInstance) {
        PlayerController controller = playerInstance.GetComponent<PlayerController>();
        Animator animator = playerInstance.GetComponentInChildren<Animator>();
        gameManager.BindPlayer(controller, animator);
        levelGenerator.BindPlayerAnimator(animator);
    }

    void SetLayerRecursively(GameObject target, int layer) {
        target.layer = layer;
        Transform targetTransform = target.transform;
        for (int i = 0; i < targetTransform.childCount; i++) {
            SetLayerRecursively(targetTransform.GetChild(i).gameObject, layer);
        }
    }

    Coroutine loadingEffectCoroutine;

    void ShowLoading() {
        SetGameplayVisible(false);
        SetMenusVisible(true);
        ShowOnlyPanel(panelLoading);
        currentState = GameFlowState.Loading;
        Time.timeScale = 1f;
        loadingEffectCoroutine = StartCoroutine(ScaleCoroutine(loadingLabel, 0.85f, 1f, 0.6f));
    }

    IEnumerator ScaleCoroutine(RectTransform target, float minScale, float maxScale, float duration) {
        while (true) {
            yield return LerpScale(target, maxScale, minScale, duration);
            yield return LerpScale(target, minScale, maxScale, duration);
        }
    }

    IEnumerator LerpScale(RectTransform target, float from, float to, float duration) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            float s = Mathf.Lerp(from, to, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = new Vector3(to, to, 1f);
    }

    IEnumerator LoadAssetsThenEnterMenu() {
        GameAssetManager assets = gameAssetManager;

        bool failed = false;
        yield return assets.LoadAsync(
            null,
            error => {
                failed = true;
                Debug.LogWarning(error);
            }
        );

        if (failed) {
            Debug.LogWarning("AssetBundle load failed.");
        } else {
            ApplyGameplayPrefabs(assets);
        }

        yield return new WaitForSeconds(3f);

        EnterMainMenu();
    }

    void ApplyGameplayPrefabs(GameAssetManager assets) {
        levelGenerator.ApplyPrefabs(
            assets.GetPrefabs(
                GameAssetManager.PrefabNames.StartText1,
                GameAssetManager.PrefabNames.StartText2,
                GameAssetManager.PrefabNames.StartText3,
                GameAssetManager.PrefabNames.StartTextRun,
                GameAssetManager.PrefabNames.StartTextNull
            ),
            assets.GetPrefabs(
                GameAssetManager.PrefabNames.Road1,
                GameAssetManager.PrefabNames.Road2
            ),
            assets.GetPrefab(GameAssetManager.PrefabNames.Gate)
        );

        obstacleSpawner.ApplyPrefabs(assets.GetPrefabs(
            GameAssetManager.PrefabNames.Rock,
            GameAssetManager.PrefabNames.Wheel,
            GameAssetManager.PrefabNames.Car,
            GameAssetManager.PrefabNames.BaseObstacle
        ));

        Segment.ApplyItemPrefabs(
            assets.GetPrefab(GameAssetManager.PrefabNames.Fence),
            assets.GetPrefab(GameAssetManager.PrefabNames.CoinPickup),
            assets.GetPrefab(GameAssetManager.PrefabNames.PowerUpPickup),
            levelGenerator.Settings
        );

        characterSelectUI.ApplyPrefabs(assets.GetPrefabs(
            GameAssetManager.PrefabNames.Player,
            GameAssetManager.PrefabNames.Player2,
            GameAssetManager.PrefabNames.Player3,
            GameAssetManager.PrefabNames.Player4
        ));
        characterSelectUI.PrewarmPreviews(this);
    }

    void EnterMainMenu() {
        if (loadingEffectCoroutine != null) { StopCoroutine(loadingEffectCoroutine); loadingEffectCoroutine = null; }
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
        SetPanelActive(panelLoading, panelToShow == panelLoading);
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
