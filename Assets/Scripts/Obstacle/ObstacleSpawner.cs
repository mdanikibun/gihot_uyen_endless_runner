using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] Transform obstacleParent;
    [SerializeField] GameSettings settings;

    Coroutine spawnCoroutine;

    void Start() {
        spawnCoroutine = StartCoroutine(SpawnObstacleCoroutine());
    }

    public void ClearAndRestartObstacles() {
        ClearObstacleChildren();

        StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnObstacleCoroutine());
    }

    public void ClearObstacleChildren() {
        for (int i = obstacleParent.childCount - 1; i >= 0; i--) {
            Destroy(obstacleParent.GetChild(i).gameObject);
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

            Instantiate(obstaclePrefab, spawnPosition, Random.rotation, obstacleParent);
        }
    }
}
