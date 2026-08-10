using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CameraController cameraController;
    [SerializeField] GameObject[] chunkStartingPrefabs;
    [SerializeField] GameObject chunkGatePrefab;
    [SerializeField] GameObject[] chunkPrefabs;
    [SerializeField] Transform chunkParent;

    [Header("Settings")]
    [SerializeField] int chunkCount = 12;
    [SerializeField] int chunkGateInterval = 8;
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float speedDefault = 10f;
    [SerializeField] float minMoveSpeed = 2f;
    [SerializeField] float maxMoveSpeed = 20f;
    [SerializeField] float minGravityZ = -22f;
    [SerializeField] float maxGravityZ = -2f;
    [SerializeField] float buffDuration = 5f;
    [SerializeField] float stumbleDuration = 1f;

    GameManager gameManager;
    Coroutine speedBuffCoroutine;

    List<GameObject> chunks = new List<GameObject>();
    readonly HashSet<GameObject> startingChunks = new HashSet<GameObject>();

    readonly float gravityZDefault = -9.81f;
    int chunkSpawnedCount = 0;
    float moveSpeed;
    float activeSpeedAmount;
    float totalDistance = 0f;
    float speedUpCountdown = 0f;
    bool canCountDistance;

    void Start() {
        moveSpeed = speedDefault;
        Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, gravityZDefault);
        SpawnStartingChunks();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update() {
        MoveChunks();

        UpdateSpeedUpCountdown();

        if (canCountDistance) {
            totalDistance += moveSpeed / 2.5f * Time.deltaTime;
            gameManager.UpdateDistanceText(totalDistance);
        }

        gameManager.UpdateSpeedUpCountdownText(speedUpCountdown);
    }

    void UpdateSpeedUpCountdown() {
        if (speedUpCountdown <= 0f || activeSpeedAmount <= 0f) return;

        speedUpCountdown -= Time.deltaTime;
        if (speedUpCountdown <= 0f) {
            speedUpCountdown = 0f;
            EndSpeedBuff();
            speedBuffCoroutine = null;
        }
    }

    public void ChangeChunkMoveSpeed(float speedAmount) {
        if (speedAmount < 0f) {
            StartStumble(speedAmount);
            return;
        }

        StartOrStackSpeedUp(speedAmount);
    }

    void StartOrStackSpeedUp(float speedAmount) {
        if (activeSpeedAmount > 0f && speedUpCountdown > 0f) {
            speedUpCountdown += buffDuration;
            return;
        }

        if (speedBuffCoroutine != null) {
            StopCoroutine(speedBuffCoroutine);
            speedBuffCoroutine = null;
            EndSpeedBuff();
        }

        ApplySpeedChange(speedAmount);
        speedUpCountdown = buffDuration;
    }

    void StartStumble(float speedAmount) {
        speedUpCountdown = 0f;

        if (speedBuffCoroutine != null) {
            StopCoroutine(speedBuffCoroutine);
            speedBuffCoroutine = null;
        }

        if (activeSpeedAmount != 0f) {
            EndSpeedBuff();
        }

        speedBuffCoroutine = StartCoroutine(StumbleRoutine(speedAmount));
    }

    IEnumerator StumbleRoutine(float speedAmount) {
        ApplySpeedChange(speedAmount);
        yield return new WaitForSeconds(stumbleDuration);
        EndSpeedBuff();
        speedBuffCoroutine = null;
    }

    void ApplySpeedChange(float speedAmount) {
        float newMoveSpeed = Mathf.Clamp(speedDefault + speedAmount, minMoveSpeed, maxMoveSpeed);
        if (Mathf.Approximately(newMoveSpeed, moveSpeed) && Mathf.Approximately(newMoveSpeed, speedDefault)) {
            return;
        }

        moveSpeed = newMoveSpeed;

        if (speedAmount > speedDefault) {
            float newGravityZ = Mathf.Clamp(gravityZDefault - speedAmount, minGravityZ, maxGravityZ);
            Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, newGravityZ);
        }

        activeSpeedAmount = speedAmount;

        if (cameraController != null) {
            cameraController.ChangeCameraFOV(speedAmount, moveSpeed, speedDefault);
        }
    }

    void EndSpeedBuff() {
        ResetSpeedToDefault();

        if (cameraController != null && activeSpeedAmount != 0f) {
            cameraController.ChangeCameraFOV(-activeSpeedAmount, moveSpeed, speedDefault);
        }

        activeSpeedAmount = 0f;
        speedUpCountdown = 0f;
    }

    void ResetSpeedToDefault() {
        moveSpeed = speedDefault;
        Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, gravityZDefault);
    }

    void SpawnStartingChunks() {
        if (chunkStartingPrefabs != null) {
            for (int i = 0; i < chunkStartingPrefabs.Length; i++) {
                if (chunkStartingPrefabs[i] == null) continue;

                float spawnPositionZ = GetSpawnPositionZ();
                Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
                GameObject introChunk = Instantiate(chunkStartingPrefabs[i], spawnPosition, Quaternion.identity, chunkParent);

                Chunk introChunkComponent = introChunk.GetComponent<Chunk>();
                if (introChunkComponent != null) {
                    introChunkComponent.DisableItemSpawn();
                }

                chunks.Add(introChunk);
                startingChunks.Add(introChunk);
            }
        }

        canCountDistance = startingChunks.Count == 0;

        while (chunks.Count < chunkCount) {
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

    void RemoveStartingChunk(GameObject chunk) {
        chunks.Remove(chunk);
        startingChunks.Remove(chunk);
        Destroy(chunk);

        SpawnSingleChunk();

        if (startingChunks.Count == 0) {
            canCountDistance = true;
        }
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

        while (chunks.Count > 0 && chunks[0].transform.position.z <= recycleZ) {
            GameObject frontChunk = chunks[0];
            if (startingChunks.Contains(frontChunk)) {
                RemoveStartingChunk(frontChunk);
            } else {
                RecycleChunk(frontChunk);
            }
        }
    }
}
