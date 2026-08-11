using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] TMP_Text scoreText;
    int score = 0;

    public int Score => score;

    public void AddScore(int amount) {
        if (gameManager.IsGameOver) return;

        score += amount;
        UpdateScoreText();
    }

    public void ResetScore() {
        score = 0;
        UpdateScoreText();
    }

    void UpdateScoreText() {
        scoreText.text = score.ToString();
    }
}
