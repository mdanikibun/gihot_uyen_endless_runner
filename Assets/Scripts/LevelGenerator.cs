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
        float spawnPositionZ = GetSpawnPositionZ();
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
        GameObject newChunk = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity, chunkParent);

        chunks.Add(newChunk);
    }

    // tái sử dụng chunk đã qua camera: ẩn -> đẩy về cuối hàng -> hiện lại
    void RecycleChunk(GameObject chunk) {
        chunks.Remove(chunk);
        chunk.SetActive(false);

        float spawnPositionZ = GetSpawnPositionZ();
        chunk.transform.position = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        // gọi reset/random object con tại đây

        chunks.Add(chunk);
        chunk.SetActive(true);
    }

    // lấy vị trí z của khối mới
    float GetSpawnPositionZ() {
        if (chunks.Count == 0) {
            return transform.position.z;
        }

        return chunks[chunks.Count - 1].transform.position.z + chunkLength;
    }

    // di chuyển các khối và recycle khi ra khỏi camera
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
