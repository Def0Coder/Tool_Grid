using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridTool : EditorWindow
{
    private GameObject[] prefabs = new GameObject[4]; // Array to hold prefabs
    private int selectedPrefabIndex = -1; // Index of the selected prefab

    [MenuItem("Tools/GridSnapper")]
    public static void OpenWindow() => GetWindow<GridTool>("GridSnapper");

    private void OnGUI()
    {
        GUILayout.Label("Grid Snapper", EditorStyles.boldLabel);

        // Display fields for selecting prefabs with checkboxes
        for (int i = 0; i < prefabs.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Toggle logic
            EditorGUI.BeginChangeCheck();
            bool isSelected = EditorGUILayout.Toggle("Select", i == selectedPrefabIndex);
            if (EditorGUI.EndChangeCheck() && isSelected)
            {
                selectedPrefabIndex = i;
            }

            // Show the preview image of the prefab
            Texture2D preview = AssetPreview.GetAssetPreview(prefabs[i]);
            if (preview != null)
            {
                GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
            }
            else
            {
                GUILayout.Label("Preview Not Available", GUILayout.Width(120));
            }

            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);

            EditorGUILayout.EndHorizontal();
        }

        // Add button to create selected prefab
        if (GUILayout.Button("Create Selected Prefab"))
        {
            CreateSelectedPrefab();
        }
    }

    private void CreateSelectedPrefab()
    {
        // Check if a prefab is selected
        if (selectedPrefabIndex != -1)
        {
            // Instantiate the selected prefab at the center of the scene
            GameObject newPrefab = Instantiate(prefabs[selectedPrefabIndex], Vector3.zero, Quaternion.identity);
            // Ensure the GameObject is properly registered in the scene
            Undo.RegisterCreatedObjectUndo(newPrefab, "Create " + prefabs[selectedPrefabIndex].name);
        }
        else
        {
            Debug.Log("Nessun prefab selezionato.");
        }
    }
}