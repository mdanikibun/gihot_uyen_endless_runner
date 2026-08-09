using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] TMP_Text scoreText;
    int score = 0;

    public void AddScore(int amount) {
        if (gameManager.IsGameOver) return;

        score += amount;
        scoreText.text = score.ToString();
    }
}
