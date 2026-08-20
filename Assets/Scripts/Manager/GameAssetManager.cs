using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameAssetManager : MonoBehaviour
{
    public static GameAssetManager Instance { get; private set; }

    public static class PrefabNames
    {
        public const string Player = "Player";
        public const string Player2 = "Player 2";
        public const string Player3 = "Player 3";
        public const string Player4 = "Player 4";

        public const string Rock = "Rock";
        public const string Wheel = "Wheel";
        public const string Car = "Car";
        public const string BaseObstacle = "Base Obstacle";
        public const string Fence = "Fence";

        public const string CoinPickup = "Coin Pickup";
        public const string PowerUpPickup = "PowerUp Pickup";

        public const string Road1 = "Road 1";
        public const string Road2 = "Road 2";
        public const string Gate = "Gate";

        public const string StartText1 = "Start Text 1";
        public const string StartText2 = "Start Text 2";
        public const string StartText3 = "Start Text 3";
        public const string StartTextRun = "Start Text Run";
        public const string StartTextNull = "Start Text Null";
    }

    const string BundleFolder = "AssetBundles";
    const string BundleFileName = "prefabs";

    AssetBundle loadedBundle;
    readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    public bool IsLoaded => prefabs.Count > 0;
    public int PrefabCount => prefabs.Count;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }

        UnloadBundle();
    }

    public GameObject GetPrefab(string prefabName) {
        if (string.IsNullOrEmpty(prefabName)) {
            return null;
        }

        prefabs.TryGetValue(prefabName, out GameObject result);

        return result;
    }

    public bool TryGetPrefab(string prefabName, out GameObject prefab) {
        prefab = GetPrefab(prefabName);
        return prefab != null;
    }

    public GameObject[] GetPrefabs(params string[] prefabNames) {
        if (prefabNames == null || prefabNames.Length == 0) {
            return Array.Empty<GameObject>();
        }

        GameObject[] results = new GameObject[prefabNames.Length];
        for (int i = 0; i < prefabNames.Length; i++) {
            results[i] = GetPrefab(prefabNames[i]);
            if (results[i] == null) {
                Debug.LogWarning("GameAssetManager: prefab not found: " + prefabNames[i]);
            }
        }

        return results;
    }

    public bool TryGetPrefabs(out GameObject[] results, params string[] prefabNames) {
        results = GetPrefabs(prefabNames);
        if (prefabNames == null || prefabNames.Length == 0) {
            return false;
        }

        for (int i = 0; i < results.Length; i++) {
            if (results[i] == null) {
                return false;
            }
        }

        return true;
    }

    public IEnumerator LoadAsync(Action onSuccess = null, Action<string> onError = null) {
        if (IsLoaded) {
            onSuccess?.Invoke();
            yield break;
        }

        string path = Path.Combine(Application.streamingAssetsPath, BundleFolder, BundleFileName);

        if (!File.Exists(path)) {
            onError?.Invoke("AssetBundle not found: " + path
                + "\nBuild on menu: Custom AssetBundle > Build Prefabs AssetBundle");
            yield break;
        }

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        loadedBundle = request.assetBundle;
        if (loadedBundle == null) {
            onError?.Invoke("Failed to load AssetBundle: " + path);
            yield break;
        }

        CachePrefabs();
        Debug.Log("AssetBundle loaded: " + BundleFileName + " (" + prefabs.Count + " prefabs)");
        onSuccess?.Invoke();
    }

    void CachePrefabs() {
        GameObject[] assets = loadedBundle.LoadAllAssets<GameObject>();
        prefabs.Clear();

        for (int i = 0; i < assets.Length; i++) {
            if (assets[i] != null) {
                prefabs[assets[i].name] = assets[i];
            }
        }
    }

    void UnloadBundle() {
        prefabs.Clear();

        if (loadedBundle != null) {
            loadedBundle.Unload(false);
            loadedBundle = null;
        }
    }
}
