using UnityEngine;

public class Coin : Pickup
{
    protected override void OnPickup() {
        Debug.Log("Coin picked up, adding 100 points");
    }
}
