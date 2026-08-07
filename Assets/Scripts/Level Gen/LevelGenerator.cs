using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int chunkCount = 12;
    [SerializeField] Transform chunkParent;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float minMoveSpeed = 2f;
    List<GameObject> chunks = new List<GameObject>();

    void Start() {
        SpawnStartingChunks();
    }

    void Update() {
        MoveChunks();
    }

    public void ChangeChunkMoveSpeed(float speedAmount) {
        moveSpeed += speedAmount;
        if (moveSpeed < minMoveSpeed) {
            moveSpeed = minMoveSpeed;
        }

        Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, Physics.gravity.z - speedAmount);
    }

    void SpawnStartingChunks() {
        for (int i = 0; i < chunkCount; i++) {
            SpawnSingleChunk();
        }
    }

    void SpawnSingleChunk() {
        float spawnPositionZ = GetSpawnPositionZ();
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
        GameObject newChunk = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity, chunkParent);

        chunks.Add(newChunk);
    }

    void RecycleChunk(GameObject chunk) {
        chunks.Remove(chunk);
        chunk.SetActive(false);

        float spawnPositionZ = GetSpawnPositionZ();
        chunk.transform.position = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        // gọi reset/random object con tại đây

        chunks.Add(chunk);
        chunk.SetActive(true);
    }

    float GetSpawnPositionZ() {
        if (chunks.Count == 0) {
            return transform.position.z;
        }

        return chunks[chunks.Count - 1].transform.position.z + chunkLength;
    }

    void MoveChunks() {
        float recycleZ = Camera.main.transform.position.z - chunkLength;

        for (int i = 0; i < chunks.Count; i++) {
            chunks[i].transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));
        }

        // check chỉ tái sử dụng chunk đã qua camera
        while (chunks.Count > 0 && chunks[0].transform.position.z <= recycleZ) {
            RecycleChunk(chunks[0]);
        }
    }
}
