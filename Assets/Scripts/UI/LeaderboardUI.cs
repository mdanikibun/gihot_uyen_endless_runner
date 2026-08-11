using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] Button closeButton;
    [SerializeField] Transform contentScores;
    [SerializeField] GameObject rowScorePrefab;

    void Awake() {
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    public void Show() {
        Refresh();
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void OnCloseClicked() {
        gameFlow.HideLeaderboard();
    }

    void Refresh() {
        for (int i = contentScores.childCount - 1; i >= 0; i--) {
            Destroy(contentScores.GetChild(i).gameObject);
        }

        List<LeaderboardEntry> scores = scoreManager.GetScores();
        if (scores == null || scores.Count == 0) return;

        for (int i = 0; i < scores.Count; i++) {
            LeaderboardEntry entry = scores[i];
            GameObject row = Instantiate(rowScorePrefab, contentScores);
            row.GetComponent<LeaderboardRowUI>().Setup(i + 1, entry.name, entry.distance, entry.score);
        }
    }
}
