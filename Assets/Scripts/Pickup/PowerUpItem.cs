using UnityEngine;

public class PowerUpItem : Pickup
{
    LevelGenerator levelGenerator;
    [SerializeField] float speedChangeAmount = 2f;

    void Start() {
        levelGenerator = FindAnyObjectByType<LevelGenerator>();
    }

    protected override void OnPickup() {
        levelGenerator.ChangeChunkMoveSpeed(speedChangeAmount);
    }
}
