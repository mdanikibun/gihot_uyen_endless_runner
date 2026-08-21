using UnityEngine;
using System.Collections.Generic;

public class Segment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameSettings settings;

    static GameObject fencePrefab;
    static GameObject powerUpItemPrefab;
    static GameObject coinPrefab;

    readonly List<int> availableLanes = new List<int>(4);
    readonly List<PooledObject> spawnedObjects = new List<PooledObject>(16);
    PoolManager poolManager;
    bool shouldSpawnItems = true;

    public static void ApplyItemPrefabs(GameObject fence, GameObject coin, GameObject powerUp, GameSettings settings) {
        fencePrefab = fence;
        coinPrefab = coin;
        powerUpItemPrefab = powerUp;
        RegisterItemPools(settings);
    }

    void Awake() {
        poolManager = PoolManager.Instance;
    }

    public void DisableItemSpawn() {
        shouldSpawnItems = false;
    }

    public void PrepareForReuse() {
        shouldSpawnItems = true;
    }

    public void ReleaseSpawnedContent() {
        ClearSpawnedContent();
    }

    public void Setup() {
        ClearSpawnedContent();
        SpawnItemsIfNeeded();
    }

    public void RepositionAndRespawn(Vector3 worldPosition) {
        ClearSpawnedContent();
        transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
        shouldSpawnItems = true;
        SpawnItemsIfNeeded();
    }

    static void RegisterItemPools(GameSettings settings) {
        PoolManager poolManager = PoolManager.Instance;
        poolManager.EnsurePool(fencePrefab, settings.segment.fencePoolSize);
        poolManager.EnsurePool(coinPrefab, settings.segment.coinPoolSize);
        poolManager.EnsurePool(powerUpItemPrefab, settings.segment.powerUpPoolSize);
    }

    void SpawnItemsIfNeeded() {
        if (!shouldSpawnItems) return;

        ResetAvailableLanes();
        SpawnFences();
        SpawnPowerUpItems();
        SpawnCoins();
    }

    void ClearSpawnedContent() {
        for (int i = 0; i < spawnedObjects.Count; i++) {
            PooledObject pooled = spawnedObjects[i];
            if (pooled != null) {
                pooled.ReturnToPool();
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
        float y = transform.position.y;
        float z = transform.position.z;

        for (int i = 0; i < fencesToSpawn; i++) {
            if (availableLanes.Count <= 0) break;

            int selectedLane = SelectLane();
            SpawnChild(fencePrefab, new Vector3(lanes[selectedLane], y, z));
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
        float laneX = lanes[selectedLane] - .25f;
        float y = transform.position.y;

        int maxCoinsToSpawn = 6;
        int coinToSpawn = Random.Range(1, maxCoinsToSpawn);
        float topOfSegmentZPosition = transform.position.z + settings.segment.coinSpacing * 2f;

        for (int i = 0; i < coinToSpawn; i++) {
            float spawnZPosition = topOfSegmentZPosition - settings.segment.coinSpacing * i;
            SpawnChild(coinPrefab, new Vector3(laneX, y, spawnZPosition));
        }
    }

    void SpawnChild(GameObject prefab, Vector3 worldPosition) {
        GameObject instance = poolManager.GetInactive(prefab);
        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(transform, false);
        instanceTransform.SetPositionAndRotation(worldPosition, Quaternion.identity);
        instance.SetActive(true);

        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled != null) {
            spawnedObjects.Add(pooled);
        }
    }

    int SelectLane() {
        int randomLaneIndex = Random.Range(0, availableLanes.Count);
        int selectedLane = availableLanes[randomLaneIndex];
        availableLanes.RemoveAt(randomLaneIndex);
        return selectedLane;
    }
}
