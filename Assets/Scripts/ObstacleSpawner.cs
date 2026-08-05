
using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] float spawnTime = 3f;

    void Start() {
        StartCoroutine(SpawnObstacleCoroutine());
    }

    IEnumerator SpawnObstacleCoroutine() {
        while (true) {
            yield return new WaitForSeconds(spawnTime);
            Instantiate(obstaclePrefab, transform.position, Random.rotation);
        }
    }
}
