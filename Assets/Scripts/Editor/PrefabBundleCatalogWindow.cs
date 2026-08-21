using UnityEditor;
using UnityEngine;

public class PrefabBundleCatalogWindow : EditorWindow
{
    const string WindowTitle = "Prefab Bundle Catalog";

    PrefabBundleCatalog catalog;
    SerializedObject serializedCatalog;
    Vector2 scrollPosition;

    public static void ShowWindow() {
        PrefabBundleCatalogWindow window = GetWindow<PrefabBundleCatalogWindow>(true, WindowTitle, true);
        window.minSize = new Vector2(420f, 560f);
        window.LoadCatalog();
        window.Show();
        window.Focus();
    }

    void OnEnable() {
        LoadCatalog();
    }

    void OnFocus() {
        LoadCatalog();
    }

    void LoadCatalog() {
        catalog = BuildPrefabsBundle.LoadCatalog();
        serializedCatalog = catalog != null ? new SerializedObject(catalog) : null;
    }

    void OnGUI() {
        serializedCatalog.Update();
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
        EditorGUILayout.ObjectField("Asset", catalog, typeof(PrefabBundleCatalog), false);
        EditorGUILayout.Space(4f);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawProperties();
        EditorGUILayout.EndScrollView();

        if (serializedCatalog.ApplyModifiedProperties()) {
            EditorUtility.SetDirty(catalog);
        }
    }

    void DrawProperties() {
        SerializedProperty property = serializedCatalog.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren)) {
            enterChildren = false;
            if (property.name == "m_Script") continue;
            EditorGUILayout.PropertyField(property, true);
        }
    }
}
