using UnityEngine;

public class PowerUpItem : Pickup
{
    LevelGenerator levelGenerator;

    void Start() {
        levelGenerator = FindAnyObjectByType<LevelGenerator>();
    }

    protected override void OnPickup() {
        levelGenerator.ChangeSegmentMoveSpeed(settings.powerUp.speedChangeAmount);
    }
}
