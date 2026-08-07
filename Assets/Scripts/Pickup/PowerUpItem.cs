using UnityEngine;

public class PowerUpItem : Pickup
{
    LevelGenerator levelGenerator;
    [SerializeField] float speedChangeAmount = 2f;

    void Start() {
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
    }

    protected override void OnPickup() {
        levelGenerator.ChangeChunkMoveSpeed(speedChangeAmount);
    }
}
