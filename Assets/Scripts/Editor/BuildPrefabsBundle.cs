using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildPrefabsBundle
{
    const string BundleName = "prefabs";
    const string OutputRoot = "Assets/StreamingAssets/AssetBundles";

    static readonly string[] PrefabPaths = {
        "Assets/Prefabs/Players/Player.prefab",
        "Assets/Prefabs/Players/Player 2.prefab",
        "Assets/Prefabs/Players/Player 3.prefab",
        "Assets/Prefabs/Players/Player 4.prefab",
        "Assets/Prefabs/Obstacles/Rock.prefab",
        "Assets/Prefabs/Obstacles/Wheel.prefab",
        "Assets/Prefabs/Obstacles/Car.prefab",
        "Assets/Prefabs/Obstacles/Base Obstacle.prefab",
        "Assets/Prefabs/Obstacles/Fence.prefab",
        "Assets/Prefabs/Pickups/Coin Pickup.prefab",
        "Assets/Prefabs/Pickups/PowerUp Pickup.prefab",
        "Assets/Prefabs/Segments/Road 1.prefab",
        "Assets/Prefabs/Segments/Road 2.prefab",
        "Assets/Prefabs/Segments/Gate.prefab",
        "Assets/Prefabs/Segments/Starting/Start Text 1.prefab",
        "Assets/Prefabs/Segments/Starting/Start Text 2.prefab",
        "Assets/Prefabs/Segments/Starting/Start Text 3.prefab",
        "Assets/Prefabs/Segments/Starting/Start Text Run.prefab",
        "Assets/Prefabs/Segments/Starting/Start Text Null.prefab",
    };

    static string BundleFilePath => Path.Combine(OutputRoot, BundleName);

    [MenuItem("Custom AssetBundle/Build Prefabs AssetBundle")]
    public static void BuildPrefabsAssetBundle() {
        int assignedCount = 0;
        for (int i = 0; i < PrefabPaths.Length; i++) {
            if (AssignBundleName(PrefabPaths[i])) {
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

    static bool AssignBundleName(string assetPath) {
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null) {
            Debug.LogError("Cannot find prefab for bundle: " + assetPath);
            return false;
        }

        importer.assetBundleName = BundleName;
        importer.SaveAndReimport();
        
        return true;
    }
}
