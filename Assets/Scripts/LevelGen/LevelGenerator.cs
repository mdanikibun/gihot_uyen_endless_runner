using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CameraController cameraController;
    [SerializeField] GameObject[] segmentStartingPrefabs;
    [SerializeField] GameObject segmentGatePrefab;
    [SerializeField] GameObject[] segmentPrefabs;
    [SerializeField] Transform segmentParent;
    [SerializeField] PoolManager poolManager;
    [SerializeField] Animator playerAnimator;
    [SerializeField] GameSettings settings;

    Coroutine speedBuffCoroutine;

    List<GameObject> segments = new List<GameObject>();
    readonly HashSet<GameObject> startingSegments = new HashSet<GameObject>();

    readonly float gravityZDefault = -9.81f;
    const string animRunSpeed = "RunAnimSpeed";
    int segmentSpawnedCount = 0;
    float moveSpeed;
    float activeSpeedAmount;
    float totalDistance = 0f;
    float speedUpCountdown = 0f;
    bool canCountDistance;
    bool isDemoMode;
    bool isPlaying;

    public bool IsDemoMode => isDemoMode;
    public bool IsPlaying => isPlaying;
    public float TotalDistance => totalDistance;

    public void BindPlayerAnimator(Animator animator) {
        playerAnimator = animator;
    }

    void Awake() {
        RegisterSegmentPools();
    }

    void Start() {
        moveSpeed = settings.level.speedDefault;
        Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, gravityZDefault);

        // Có GameFlow thì để menu gọi EnterDemoMode / ResetForNewRun
        if (FindAnyObjectByType<GameFlowController>() == null) {
            SpawnStartingSegments();
            isPlaying = true;
        }
    }

    void Update() {
        MoveSegments();
        UpdateSpeedUpCountdown();

        if (canCountDistance) {
            totalDistance += moveSpeed / 2.5f * Time.deltaTime;
            GameEvents.RaiseDistanceChanged(totalDistance);
        }

        GameEvents.RaiseSpeedUpCountdownChanged(speedUpCountdown);
    }

    void RegisterSegmentPools() {
        for (int i = 0; i < segmentPrefabs.Length; i++) {
            if (segmentPrefabs[i] != null) {
                poolManager.EnsurePool(segmentPrefabs[i], settings.level.segmentPoolSize);
            }
        }

        if (segmentGatePrefab != null) {
            poolManager.EnsurePool(segmentGatePrefab, settings.level.gatePoolSize);
        }

        for (int i = 0; i < segmentStartingPrefabs.Length; i++) {
            if (segmentStartingPrefabs[i] != null) {
                poolManager.EnsurePool(segmentStartingPrefabs[i], settings.level.startingPoolSize);
            }
        }
    }

    public void ClearAllSegments() {
        for (int i = 0; i < segments.Count; i++) {
            if (segments[i] != null) {
                ReturnSegment(segments[i]);
            }
        }

        segments.Clear();
        startingSegments.Clear();
        segmentSpawnedCount = 0;
    }

    public void EnterDemoMode() {
        isDemoMode = true;
        isPlaying = false;
        canCountDistance = false;
        totalDistance = 0f;

        ClearAllSpeedEffects();
        ClearAllSegments();
        SpawnDemoSegments();
    }

    public void ResetForNewRun() {
        isDemoMode = false;
        isPlaying = true;
        canCountDistance = false;
        totalDistance = 0f;

        ClearAllSpeedEffects();
        ClearAllSegments();
        SpawnStartingSegments();
    }

    void SpawnDemoSegments() {
        while (segments.Count < settings.level.segmentCount) {
            SpawnSingleSegment();
        }
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

    public void ChangeSegmentMoveSpeed(float speedAmount) {
        if (speedAmount < 0f) {
            StartStumble(speedAmount);
            return;
        }

        StartOrStackSpeedUp(speedAmount);
    }

    void StartOrStackSpeedUp(float speedAmount) {
        if (activeSpeedAmount > 0f && speedUpCountdown > 0f) {
            speedUpCountdown += settings.level.buffDuration;
            return;
        }

        if (speedBuffCoroutine != null) {
            StopCoroutine(speedBuffCoroutine);
            speedBuffCoroutine = null;
            EndSpeedBuff();
        }

        ApplySpeedChange(speedAmount);
        speedUpCountdown = settings.level.buffDuration;
        GameEvents.RaisePowerUpStartedSFX();
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
        yield return new WaitForSeconds(settings.level.stumbleDuration);
        EndSpeedBuff();
        speedBuffCoroutine = null;
    }

    void ApplySpeedChange(float speedAmount) {
        float newMoveSpeed = Mathf.Clamp(settings.level.speedDefault + speedAmount, settings.level.minMoveSpeed, settings.level.maxMoveSpeed);
        if (Mathf.Approximately(newMoveSpeed, moveSpeed) && Mathf.Approximately(newMoveSpeed, settings.level.speedDefault)) {
            return;
        }

        moveSpeed = newMoveSpeed;

        if (speedAmount > settings.level.speedDefault) {
            float newGravityZ = Mathf.Clamp(gravityZDefault - speedAmount, settings.level.minGravityZ, settings.level.maxGravityZ);
            Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, newGravityZ);
        }

        activeSpeedAmount = speedAmount;
        UpdateRunAnimSpeed();

        if (cameraController != null) {
            cameraController.ChangeCameraFOV(speedAmount, moveSpeed, settings.level.speedDefault);
        }
    }

    void EndSpeedBuff() {
        ClearAllSpeedEffects();
    }

    void ClearAllSpeedEffects() {
        bool hadSpeedUp = activeSpeedAmount > 0f;

        if (speedBuffCoroutine != null) {
            StopCoroutine(speedBuffCoroutine);
            speedBuffCoroutine = null;
        }

        activeSpeedAmount = 0f;
        speedUpCountdown = 0f;
        ResetSpeedToDefault();
        UpdateRunAnimSpeed();

        if (cameraController != null) {
            cameraController.ResetToDefault();
        }

        if (hadSpeedUp) {
            GameEvents.RaisePowerUpEndedSFX();
        }
    }

    void UpdateRunAnimSpeed() {
        if (playerAnimator == null) return;
        if (!playerAnimator.isActiveAndEnabled) return;
        if (playerAnimator.runtimeAnimatorController == null) return;

        float animSpeed = 1f;
        if (activeSpeedAmount > 0f && settings.level.speedDefault > 0f) {
            animSpeed = Mathf.Clamp(
                moveSpeed / settings.level.speedDefault,
                1f,
                settings.powerUp.runAnimSpeedMax
            );
        }

        playerAnimator.SetFloat(animRunSpeed, animSpeed);
    }

    void ResetSpeedToDefault() {
        moveSpeed = settings.level.speedDefault;
        Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, gravityZDefault);
    }

    void SpawnStartingSegments() {
        for (int i = 0; i < segmentStartingPrefabs.Length; i++) {
            if (segmentStartingPrefabs[i] == null) continue;

            float spawnPositionZ = GetSpawnPositionZ();
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);
            GameObject introSegment = SpawnSegmentFromPool(segmentStartingPrefabs[i], spawnPosition, false);

            Segment introSegmentComponent = introSegment.GetComponent<Segment>();
            if (introSegmentComponent != null) {
                introSegmentComponent.DisableItemSpawn();
            }

            introSegment.SetActive(true);
            segments.Add(introSegment);
            startingSegments.Add(introSegment);
        }

        canCountDistance = startingSegments.Count == 0;

        while (segments.Count < settings.level.segmentCount) {
            SpawnSingleSegment();
        }
    }

    void SpawnSingleSegment() {
        float spawnPositionZ = GetSpawnPositionZ();
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        GameObject prefab;
        if (segmentSpawnedCount % settings.level.segmentGateInterval == 0 && segmentSpawnedCount > 0) {
            prefab = segmentGatePrefab;
        } else {
            prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
        }

        GameObject newSegment = SpawnSegmentFromPool(prefab, spawnPosition, true);
        segments.Add(newSegment);
        segmentSpawnedCount++;
    }

    GameObject SpawnSegmentFromPool(GameObject prefab, Vector3 spawnPosition, bool activate) {
        GameObject segment = poolManager.GetInactive(prefab);
        segment.transform.SetParent(segmentParent, false);
        segment.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        if (activate) {
            segment.SetActive(true);
            SetupSegment(segment);
        }
        return segment;
    }

    void SetupSegment(GameObject segment) {
        Segment segmentComponent = segment.GetComponent<Segment>();
        if (segmentComponent == null) return;

        segmentComponent.PrepareForReuse();
        segmentComponent.Setup();
    }

    void ReturnSegment(GameObject segment) {
        Segment segmentComponent = segment.GetComponent<Segment>();
        if (segmentComponent != null) {
            segmentComponent.ReleaseSpawnedContent();
        }

        poolManager.Return(segment);
    }

    void RecycleSegment(GameObject segment) {
        segments.Remove(segment);

        Segment segmentComponent = segment.GetComponent<Segment>();
        if (segmentComponent != null) {
            segmentComponent.ReleaseSpawnedContent();
        }

        segment.SetActive(false);

        float spawnPositionZ = GetSpawnPositionZ();
        segment.transform.position = new Vector3(transform.position.x, transform.position.y, spawnPositionZ);

        segments.Add(segment);
        segment.SetActive(true);

        if (segmentComponent != null) {
            segmentComponent.PrepareForReuse();
            segmentComponent.Setup();
        }
    }

    void RemoveStartingSegment(GameObject segment) {
        segments.Remove(segment);
        startingSegments.Remove(segment);
        ReturnSegment(segment);

        SpawnSingleSegment();

        if (startingSegments.Count == 0) {
            canCountDistance = true;
        }
    }

    float GetSpawnPositionZ() {
        if (segments.Count == 0) {
            return transform.position.z;
        }

        return segments[segments.Count - 1].transform.position.z + settings.level.segmentLength;
    }

    void MoveSegments() {
        float recycleZ = Camera.main.transform.position.z - settings.level.segmentLength;

        for (int i = 0; i < segments.Count; i++) {
            segments[i].transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));
        }

        while (segments.Count > 0 && segments[0].transform.position.z <= recycleZ) {
            GameObject frontSegment = segments[0];
            if (startingSegments.Contains(frontSegment)) {
                RemoveStartingSegment(frontSegment);
            } else {
                RecycleSegment(frontSegment);
            }
        }
    }
}
