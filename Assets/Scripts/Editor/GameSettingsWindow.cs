using UnityEditor;
using UnityEngine;

public class GameSettingsWindow : EditorWindow
{
    const string AssetPath = "Assets/Settings/GameSettings.asset";
    const string WindowTitle = "Game Settings";

    GameSettings settings;
    SerializedObject serializedSettings;
    Vector2 scrollPosition;

    public static void ShowWindow() {
        GameSettingsWindow window = GetWindow<GameSettingsWindow>(true, WindowTitle, true);
        window.minSize = new Vector2(420f, 520f);
        window.LoadSettings();
        window.Show();
        window.Focus();
    }

    void OnEnable() {
        LoadSettings();
    }

    void OnFocus() {
        LoadSettings();
    }

    void LoadSettings() {
        settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetPath);
        serializedSettings = settings != null ? new SerializedObject(settings) : null;
    }

    void OnGUI() {
        if (settings == null || serializedSettings == null) {
            EditorGUILayout.HelpBox("Cannot find GameSettings at:\n" + AssetPath, MessageType.Error);
            if (GUILayout.Button("Refresh")) {
                LoadSettings();
            }
            return;
        }

        serializedSettings.Update();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
        EditorGUILayout.ObjectField("Asset", settings, typeof(GameSettings), false);
        EditorGUILayout.Space(4f);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawProperties();
        EditorGUILayout.EndScrollView();

        if (serializedSettings.ApplyModifiedProperties()) {
            EditorUtility.SetDirty(settings);
        }
    }

    void DrawProperties() {
        SerializedProperty property = serializedSettings.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren)) {
            enterChildren = false;
            if (property.name == "m_Script") continue;
            EditorGUILayout.PropertyField(property, true);
        }
    }
}
