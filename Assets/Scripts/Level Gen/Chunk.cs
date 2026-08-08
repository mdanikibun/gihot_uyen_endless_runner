using UnityEngine;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject fencePrefab;
    [SerializeField] GameObject powerUpItemPrefab;
    [SerializeField] GameObject coinPrefab;

    [Header("Settings")]
    [SerializeField] float powerUpItemSpawnChance = 0.3f;
    [SerializeField] float coinSpawnChance = 0.5f;
    [SerializeField] float coinSpacing = 2f;

    [SerializeField] float[] lanes = { -3f, 0f, 3f };

    List<int> availableLanes = new List<int>();
    readonly List<GameObject> spawnedObjects = new List<GameObject>();

    void Start() {
        Setup();
    }

    // Xóa fence/pickup cũ và random lại. Gọi khi spawn lần đầu hoặc recycle chunk.
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
        for (int i = 0; i < lanes.Length; i++) {
            availableLanes.Add(i);
        }
    }

    void SpawnFences() {
        int fencesToSpawn = Random.Range(0, lanes.Length);

        for (int i = 0; i < fencesToSpawn; i++) {
            if (availableLanes.Count <= 0) break;

            int selectedLane = SelectLane();
            Vector3 spawnPosition = new Vector3(lanes[selectedLane], transform.position.y, transform.position.z);
            SpawnChild(fencePrefab, spawnPosition);
        }
    }

    void SpawnPowerUpItems() {
        if (Random.value > powerUpItemSpawnChance || availableLanes.Count <= 0) return;

        int selectedLane = SelectLane();
        Vector3 spawnPosition = new Vector3(lanes[selectedLane] - .25f, transform.position.y, transform.position.z);
        SpawnChild(powerUpItemPrefab, spawnPosition);
    }

    void SpawnCoins() {
        if (Random.value > coinSpawnChance || availableLanes.Count <= 0) return;

        int selectedLane = SelectLane();

        int maxCoinsToSpawn = 6;
        int coinToSpawn = Random.Range(1, maxCoinsToSpawn);
        float topOfChunkZPosition = transform.position.z + coinSpacing * 2f;

        for (int i = 0; i < coinToSpawn; i++) {
            float spawnZPosition = topOfChunkZPosition - coinSpacing * i;
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
