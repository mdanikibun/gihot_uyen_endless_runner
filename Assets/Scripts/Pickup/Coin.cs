using UnityEngine;

public class Coin : Pickup
{

    [Header("Settings")]
    [SerializeField] int scoreAmount = 10;

    ScoreManager scoreManager;

    void Start() {
        scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    protected override void OnPickup() {
        scoreManager.AddScore(scoreAmount);
    }
}
