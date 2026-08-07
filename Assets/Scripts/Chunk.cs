using UnityEngine;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    [SerializeField] GameObject fencePrefab;
    [SerializeField] GameObject powerUpItemPrefab;
    [SerializeField] GameObject coinPrefab;

    [SerializeField] float powerUpItemSpawnChance = 0.3f;
    [SerializeField] float coinSpawnChance = 0.5f;
    [SerializeField] float coinSpacing = 2f;

    [SerializeField] float[] lanes = {-3.5f, 0f, 3.5f};

    List<int> availableLanes = new List<int>() {0, 1, 2};

    void Start() {
        SpawnFences();
        SpawnPowerUpItems();
        SpawnCoins();
    }

    void SpawnFences() {
        int fencesToSpawn = Random.Range(0, lanes.Length);

        for (int i = 0; i < fencesToSpawn; i++) {

            if (availableLanes.Count <= 0) break;

            int selectedLane = SelectLane();
            Vector3 spawnPosition = new Vector3(lanes[selectedLane], transform.position.y, transform.position.z);
            Instantiate(fencePrefab, spawnPosition, Quaternion.identity, this.transform);
        }
    }

    void SpawnPowerUpItems() {
        if (Random.value > powerUpItemSpawnChance || availableLanes.Count <= 0) return;

        int selectedLane = SelectLane();
        Vector3 spawnPosition = new Vector3(lanes[selectedLane], transform.position.y, transform.position.z);
        Instantiate(powerUpItemPrefab, spawnPosition, Quaternion.identity, this.transform);
    }

    void SpawnCoins() {
        if (Random.value > coinSpawnChance || availableLanes.Count <= 0) return;

        int selectedLane = SelectLane();

        int maxCoinsToSpawn = 6;
        int coinToSpawn = Random.Range(1, maxCoinsToSpawn);
        float topOfChunkZPosition = transform.position.z + coinSpacing * 2f;

        for (int i = 0; i < coinToSpawn; i++) {
            float spawnZPosition = topOfChunkZPosition - coinSpacing * i;
            Vector3 spawnPosition = new Vector3(lanes[selectedLane], transform.position.y, spawnZPosition);
            Instantiate(coinPrefab, spawnPosition, Quaternion.identity, this.transform);
        }
    }

    int SelectLane() {
        int randomLaneIndex = Random.Range(0, availableLanes.Count);
        int selectedLane = availableLanes[randomLaneIndex];
        availableLanes.RemoveAt(randomLaneIndex);

        return selectedLane;
    }
}
