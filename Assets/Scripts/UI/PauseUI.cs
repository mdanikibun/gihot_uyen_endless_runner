using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button resumeButton;
    [SerializeField] Button leaderboardButton;
    [SerializeField] Button backToMainButton;

    void Awake() {
        resumeButton.onClick.AddListener(OnResumeClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        backToMainButton.onClick.AddListener(OnBackToMainClicked);
    }

    public void OnResumeClicked() {
        gameFlow.ResumeGame();
    }

    public void OnLeaderboardClicked() {
        gameFlow.ShowLeaderboard();
    }

    public void OnBackToMainClicked() {
        gameFlow.BackToMainMenu();
    }
}
