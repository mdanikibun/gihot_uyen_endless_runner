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
    readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

    public bool IsLoaded => prefabs.Count > 0;

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
        return prefabs[prefabName];
    }

    public GameObject[] GetPrefabs(params string[] prefabNames) {
        GameObject[] results = new GameObject[prefabNames.Length];
        for (int i = 0; i < prefabNames.Length; i++) {
            results[i] = GetPrefab(prefabNames[i]);
        }

        return results;
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
        prefabs.Clear();

        string[] assetPaths = loadedBundle.GetAllAssetNames();
        for (int i = 0; i < assetPaths.Length; i++) {
            GameObject go = loadedBundle.LoadAsset<GameObject>(assetPaths[i]);
            if (go == null) continue;

            prefabs[go.name] = go;
            prefabs[Path.GetFileNameWithoutExtension(assetPaths[i])] = go;
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
