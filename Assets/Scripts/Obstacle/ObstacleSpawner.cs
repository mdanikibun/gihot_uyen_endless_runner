using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform obstacleParent;
    [SerializeField] PoolManager poolManager;
    [SerializeField] GameSettings settings;
    
    GameObject[] obstaclePrefabs;

    Coroutine spawnCoroutine;

    public void ApplyPrefabs(GameObject[] prefabs) {
        obstaclePrefabs = prefabs;

        RegisterObstaclePools();
        EnsureSpawnRunning();
    }

    void RegisterObstaclePools() {
        int poolSize = Mathf.Max(1, settings.obstacleSpawn.poolSize);
        for (int i = 0; i < obstaclePrefabs.Length; i++) {
            poolManager.EnsurePool(obstaclePrefabs[i], poolSize);
        }
    }

    void EnsureSpawnRunning() {
        if (spawnCoroutine != null) return;
        spawnCoroutine = StartCoroutine(SpawnObstacleCoroutine());
    }

    public void ClearAndRestartObstacles() {
        ClearObstacleChildren();

        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnObstacleCoroutine());
    }

    public void ClearObstacleChildren() {
        for (int i = obstacleParent.childCount - 1; i >= 0; i--) {
            poolManager.Return(obstacleParent.GetChild(i).gameObject);
        }
    }

    IEnumerator SpawnObstacleCoroutine() {
        while (true) {
            yield return new WaitForSeconds(settings.obstacleSpawn.spawnTime);

            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            Vector3 spawnPosition = new Vector3(
                Random.Range(-settings.obstacleSpawn.spawnWidth, settings.obstacleSpawn.spawnWidth),
                transform.position.y,
                transform.position.z
            );

            GameObject obstacle = poolManager.GetInactive(obstaclePrefab);
            obstacle.transform.SetParent(obstacleParent, false);
            obstacle.transform.SetPositionAndRotation(spawnPosition, Random.rotation);
            obstacle.SetActive(true);
        }
    }
}
