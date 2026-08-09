using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CameraController cameraController;
    [SerializeField] GameObject chunkGatePrefab;
    [SerializeField] GameObject[] chunkPrefabs;
    [SerializeField] Transform chunkParent;

    [Header("Settings")]
    [SerializeField] int chunkCount = 12;
    [SerializeField] int chunkGateInterval = 8;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float minMoveSpeed = 2f;
    [SerializeField] float maxMoveSpeed = 20f;
    [SerializeField] float minGravityZ = -22f;
    [SerializeField] float maxGravityZ = -2f;

    List<GameObject> chunks = new List<GameObject>();
    int chunkSpawnedCount = 0;

    void Start() {
        SpawnStartingChunks();
    }

    void Update() {
        MoveChunks();
    }

    public void ChangeChunkMoveSpeed(float speedAmount) {
        float newMoveSpeed = moveSpeed + speedAmount;
        newMoveSpeed = Mathf.Clamp(newMoveSpeed, minMoveSpeed, maxMoveSpeed);

        if (newMoveSpeed != moveSpeed) {
            moveSpeed = newMoveSpeed;

            float newGravityZ = Physics.gravity.z - speedAmount;
            newGravityZ = Mathf.Clamp(newGravityZ, minGravityZ, maxGravityZ);
            Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, newGravityZ);

            cameraController.ChangeCameraFOV(speedAmount);
        }
    }

    void SpawnStartingChunks() {
        for (int i = 0; i < chunkCount; i++) {
            SpawnSingleChunk();
        }
    }

    void SpawnSingleChunk() {
        float spawnPositionZ = GetSpawnPositionZ();
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        GameObject newChunk;
        if (chunkSpawnedCount % chunkGateInterval == 0 && chunkSpawnedCount > 0) {
            newChunk = Instantiate(chunkGatePrefab, spawnPosition, Quaternion.identity, chunkParent);
        } else {
            newChunk = Instantiate(chunkPrefabs[Random.Range(0, chunkPrefabs.Length)], spawnPosition, Quaternion.identity, chunkParent);
        }
        chunks.Add(newChunk);
        chunkSpawnedCount++;
    }

    void RecycleChunk(GameObject chunk) {
        chunks.Remove(chunk);
        chunk.SetActive(false);

        float spawnPositionZ = GetSpawnPositionZ();
        chunk.transform.position = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        Chunk chunkComponent = chunk.GetComponent<Chunk>();
        if (chunkComponent != null) {
            chunkComponent.Setup();
        }

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
