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

    public GameObject Resolve(GameObject catalogEntry) {
        return GetPrefab(catalogEntry.name);
    }

    public GameObject[] ResolveMany(GameObject[] catalogEntries) {
        GameObject[] results = new GameObject[catalogEntries.Length];
        for (int i = 0; i < catalogEntries.Length; i++) {
            results[i] = Resolve(catalogEntries[i]);
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
