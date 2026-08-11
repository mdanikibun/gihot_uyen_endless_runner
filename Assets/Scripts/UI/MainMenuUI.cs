using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button startButton;
    [SerializeField] Button leaderboardButton;
    [SerializeField] Button exitButton;

    void Awake() {
        startButton.onClick.AddListener(OnStartClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    public void OnStartClicked() {
        gameFlow.ShowCharacterSelect();
    }

    public void OnLeaderboardClicked() {
        gameFlow.ShowLeaderboard();
    }

    public void OnExitClicked() {
        gameFlow.QuitGame();
    }
}
