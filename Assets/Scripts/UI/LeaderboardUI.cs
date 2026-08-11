using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button closeButton;
    [SerializeField] Transform contentScores;
    [SerializeField] GameObject rowScorePrefab;

    void Awake() {
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void OnCloseClicked() {
        gameFlow.HideLeaderboard();
    }
}
