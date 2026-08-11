using UnityEngine;

public class Coin : Pickup
{
    ScoreManager scoreManager;

    void Start() {
        scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    protected override void OnPickup() {
        scoreManager.AddScore(settings.coin.scoreAmount);
    }
}
