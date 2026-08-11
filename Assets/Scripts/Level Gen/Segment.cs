using UnityEngine;
using System.Collections.Generic;

public class Segment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject fencePrefab;
    [SerializeField] GameObject powerUpItemPrefab;
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameSettings settings;

    List<int> availableLanes = new List<int>();
    readonly List<GameObject> spawnedObjects = new List<GameObject>();
    bool shouldSpawnItems = true;

    void Start() {
        if (shouldSpawnItems) {
            Setup();
        }
    }

    public void DisableItemSpawn() {
        shouldSpawnItems = false;
    }

    public void Setup() {
        ClearSpawnedContent();
        ResetAvailableLanes();
        SpawnFences();
        SpawnPowerUpItems();
        SpawnCoins();
    }

    void ClearSpawnedContent() {
        for (int i = 0; i < spawnedObjects.Count; i++) {
            if (spawnedObjects[i] != null) {
                Destroy(spawnedObjects[i]);
            }
        }
        spawnedObjects.Clear();
    }

    void ResetAvailableLanes() {
        availableLanes.Clear();
        float[] lanes = settings.segment.lanes;
        for (int i = 0; i < lanes.Length; i++) {
            availableLanes.Add(i);
        }
    }

    void SpawnFences() {
        float[] lanes = settings.segment.lanes;
        int fencesToSpawn = Random.Range(0, lanes.Length);

        for (int i = 0; i < fencesToSpawn; i++) {
            if (availableLanes.Count <= 0) break;

            int selectedLane = SelectLane();
            Vector3 spawnPosition = new Vector3(lanes[selectedLane], transform.position.y, transform.position.z);
            SpawnChild(fencePrefab, spawnPosition);
        }
    }

    void SpawnPowerUpItems() {
        if (Random.value > settings.segment.powerUpItemSpawnChance || availableLanes.Count <= 0) return;

        float[] lanes = settings.segment.lanes;
        int selectedLane = SelectLane();
        Vector3 spawnPosition = new Vector3(lanes[selectedLane] - .25f, transform.position.y, transform.position.z);
        SpawnChild(powerUpItemPrefab, spawnPosition);
    }

    void SpawnCoins() {
        if (Random.value > settings.segment.coinSpawnChance || availableLanes.Count <= 0) return;

        float[] lanes = settings.segment.lanes;
        int selectedLane = SelectLane();

        int maxCoinsToSpawn = 6;
        int coinToSpawn = Random.Range(1, maxCoinsToSpawn);
        float topOfSegmentZPosition = transform.position.z + settings.segment.coinSpacing * 2f;

        for (int i = 0; i < coinToSpawn; i++) {
            float spawnZPosition = topOfSegmentZPosition - settings.segment.coinSpacing * i;
            Vector3 spawnPosition = new Vector3(lanes[selectedLane] - .25f, transform.position.y, spawnZPosition);
            SpawnChild(coinPrefab, spawnPosition);
        }
    }

    void SpawnChild(GameObject prefab, Vector3 worldPosition) {
        if (prefab == null) return;

        GameObject instance = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
        spawnedObjects.Add(instance);
    }

    int SelectLane() {
        int randomLaneIndex = Random.Range(0, availableLanes.Count);
        int selectedLane = availableLanes[randomLaneIndex];
        availableLanes.RemoveAt(randomLaneIndex);

        return selectedLane;
    }
}
