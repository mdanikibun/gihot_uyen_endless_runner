using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameAssetManager : MonoBehaviour
{
    public static GameAssetManager Instance { get; private set; }

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
