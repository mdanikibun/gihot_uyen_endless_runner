using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildPrefabsBundle
{
    const string BundleName = "prefabs";
    const string OutputRoot = "Assets/StreamingAssets/AssetBundles";
    public const string CatalogPath = "Assets/Settings/PrefabBundleCatalog.asset";

    static string BundleFilePath => Path.Combine(OutputRoot, BundleName);

    [MenuItem("Custom AssetBundle/Build Prefabs AssetBundle")]
    public static void BuildPrefabsAssetBundle() {
        PrefabBundleCatalog catalog = LoadCatalog();
        List<GameObject> prefabs = CollectPrefabs(catalog);
        int assignedCount = 0;
        for (int i = 0; i < prefabs.Count; i++) {
            if (AssignBundleName(prefabs[i])) {
                assignedCount++;
            }
        }

        if (!Directory.Exists(OutputRoot)) {
            Directory.CreateDirectory(OutputRoot);
        }

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        BuildPipeline.BuildAssetBundles(OutputRoot, BuildAssetBundleOptions.None, target);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (File.Exists(BundleFilePath)) {
            Debug.Log(
                "Prefabs AssetBundle built: " + BundleFilePath
                + " (" + assignedCount + " prefabs, platform: " + target + ")"
            );
        } else {
            Debug.LogWarning("Build finished but bundle file was not found at: " + BundleFilePath);
        }
    }

    static bool AssignBundleName(GameObject prefab) {
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null) {
            Debug.LogError("Cannot find prefab for bundle: " + prefab.name);
            return false;
        }

        importer.assetBundleName = BundleName;
        importer.SaveAndReimport();

        return true;
    }

    [MenuItem("Custom AssetBundle/Prefab Bundle Catalog")]
    public static void OpenCatalogWindow() {
        PrefabBundleCatalogWindow.ShowWindow();
    }

    public static PrefabBundleCatalog LoadCatalog() {
        return AssetDatabase.LoadAssetAtPath<PrefabBundleCatalog>(CatalogPath);
    }

    static List<GameObject> CollectPrefabs(PrefabBundleCatalog catalog) {
        List<GameObject> prefabs = new List<GameObject>();
        HashSet<Object> seen = new HashSet<Object>();

        foreach (GameObject prefab in catalog.GetAllPrefabs()) {
            if (prefab == null || !seen.Add(prefab)) continue;
            prefabs.Add(prefab);
        }
        Debug.Log("Collected " + prefabs.Count + " prefabs");
        return prefabs;
    }
}
