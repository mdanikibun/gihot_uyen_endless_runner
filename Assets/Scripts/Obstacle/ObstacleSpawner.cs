using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] Transform obstacleParent;

    [Header("Settings")]
    [SerializeField] float spawnTime = 3f;
    [SerializeField] float spawnWidth = 4f;

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
            yield return new WaitForSeconds(spawnTime);
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnWidth, spawnWidth),
                transform.position.y,
                transform.position.z
            );

            Instantiate(obstaclePrefab, spawnPosition, Random.rotation, obstacleParent);
        }
    }
}
