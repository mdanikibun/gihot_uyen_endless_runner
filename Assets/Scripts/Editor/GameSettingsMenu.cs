using UnityEditor;
using UnityEngine;

public static class GameSettingsMenu
{
    const string MenuRoot = "GameSettings";
    const string AssetPath = "Assets/Settings/GameSettings.asset";

    [MenuItem(MenuRoot + "/Open Settings", priority = 0)]
    public static void OpenSettings() {
        GameSettings settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetPath);
        if (settings == null) {
            Debug.LogError("Cannot find GameSettings at: " + AssetPath);
            return;
        }

        GameSettingsWindow.ShowWindow();
    }
}
