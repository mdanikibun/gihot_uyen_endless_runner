using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int chunkCount = 12;
    [SerializeField] Transform chunkParent;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float moveSpeed = 10f;
    List<GameObject> chunks = new List<GameObject>();
    int chunkSpawnIndex;

    void Start() {
        SpawnStartingChunks();
    }

    void Update() {
        MoveChunks();
    }

    // tạo các khối đầu tiên
    void SpawnStartingChunks() {
        for (int i = 0; i < chunkCount; i++) {
            SpawnSingleChunk();
        }
    }

    // tạo 1 khối mới
    void SpawnSingleChunk() {
        float spawnPositionZ = getSpawnPositionZ();

        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
        GameObject newChunk = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity, chunkParent);

        // tô màu tạm cho khối, sau sẽ thay bằng nền hình ảnh
        Color chunkColor = chunkSpawnIndex % 2 == 0 ? Color.white : Color.black;
        SetChunkColor(newChunk, chunkColor);
        chunkSpawnIndex++;

        chunks.Add(newChunk);
    }

    // tô màu cho khối
    void SetChunkColor(GameObject chunk, Color color) {
        foreach (Renderer renderer in chunk.GetComponentsInChildren<Renderer>()) {
            renderer.material.color = color;
        }
    }

    // lấy vị trí z của khối mới
    float getSpawnPositionZ() {
        float spawnPositionZ;
        if (chunks.Count == 0) {
            spawnPositionZ = transform.position.z;
        } else {
            spawnPositionZ = chunks[chunks.Count - 1].transform.position.z + chunkLength;
        }

        return spawnPositionZ;
    }

    // di chuyển các khối
    void MoveChunks() {
        for (int i = 0; i < chunks.Count; i++) {
            GameObject chunk = chunks[i];
            chunk.transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));

            if (chunk.transform.position.z <= Camera.main.transform.position.z - chunkLength) {
                chunks.Remove(chunk);
                Destroy(chunk);
                SpawnSingleChunk();
            }
        }
    }
}
