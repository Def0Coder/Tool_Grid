using UnityEngine;
using UnityEditor;

public class PrefabPreviewTool : EditorWindow
{
    private GameObject[] selectedPrefabs = new GameObject[3];
    private bool[] prefabActive = new bool[3];
    private Quaternion previewRotation = Quaternion.identity;
    private GameObject[] previewObjects = new GameObject[3];
    private Vector3 spawnPosition;
    private bool createOnXAxis = true; // Se true, il nuovo prefab sarà creato lungo l'asse x, altrimenti lungo l'asse z

    [MenuItem("Tools/Prefab Preview Tool")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPreviewTool>("Prefab Preview Tool");
    }

    private void OnGUI()
    {
        for (int i = 0; i < selectedPrefabs.Length; i++)
        {
            GUILayout.Label($"Select Prefab {i + 1}:", EditorStyles.boldLabel);
            selectedPrefabs[i] = EditorGUILayout.ObjectField(selectedPrefabs[i], typeof(GameObject), false) as GameObject;

            GUILayout.Space(10);

            GUILayout.Label($"Preview Settings {i + 1}:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            prefabActive[i] = EditorGUILayout.Toggle($"Prefab {i + 1} Active", prefabActive[i]);
            if (EditorGUI.EndChangeCheck() && prefabActive[i]) // Disattiva le altre checkbox se questa è attiva
            {
                for (int j = 0; j < prefabActive.Length; j++)
                {
                    if (j != i)
                    {
                        prefabActive[j] = false;
                    }
                }
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Preview Settings:", EditorStyles.boldLabel);
        previewRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Preview Rotation", previewRotation.eulerAngles));

        GUILayout.Space(10);

        if (GUILayout.Button("Rotate Preview 90°"))
        {
            RotatePreview(90f);
        }

        GUILayout.Space(10);

        GUILayout.Label("Create Prefab Along Axis:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        createOnXAxis = GUILayout.Toggle(createOnXAxis, "X Axis");
        GUILayout.Space(10);
        createOnXAxis = !GUILayout.Toggle(!createOnXAxis, "Z Axis");
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        for (int i = 0; i < selectedPrefabs.Length; i++)
        {
            if (selectedPrefabs[i] != null && prefabActive[i])
            {
                GUILayout.Label($"Prefab {i + 1} Preview:", EditorStyles.boldLabel);
                DrawPrefabPreview(i);
            }
            else
            {
                if (previewObjects[i] != null)
                {
                    DestroyImmediate(previewObjects[i]);
                    previewObjects[i] = null;
                }
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Create Prefab Instance"))
        {
            CreatePrefabInstance();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Undo"))
        {
            Undo.PerformUndo();
        }
    }

    private void DrawPrefabPreview(int index)
    {
        if (selectedPrefabs[index] != null)
        {
            if (previewObjects[index] == null)
            {
                previewObjects[index] = Instantiate(selectedPrefabs[index]);
            }

            previewObjects[index].transform.position = spawnPosition; // Set the preview position
            previewObjects[index].transform.rotation = Quaternion.Euler(previewRotation.eulerAngles);
            EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(100, 100), AssetPreview.GetAssetPreview(previewObjects[index]));
        }
    }

    private void RotatePreview(float angle)
    {
        previewRotation *= Quaternion.Euler(0, angle, 0);
        Repaint();
    }

    private void CreatePrefabInstance()
    {
        for (int i = 0; i < selectedPrefabs.Length; i++)
        {
            if (selectedPrefabs[i] != null && prefabActive[i])
            {
                // Get the position to create the prefab instance
                spawnPosition = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 5f;
                spawnPosition.y = 0; // Ensure objects are placed on the same plane

                // Apply offset based on user choice (along x or z axis)
                float offset = createOnXAxis ? 10f : 0f;
                spawnPosition += createOnXAxis ? new Vector3(offset, 0f, 0f) : new Vector3(0f, 0f, offset);

                // Check if there is no other object nearby
                Collider[] colliders = Physics.OverlapSphere(spawnPosition, 0.5f);
                if (colliders.Length > 0)
                {
                    // Find a free position on the same row (x or z)
                    offset = createOnXAxis ? 10f : 0f;
                    Vector3 originalPosition = spawnPosition;
                    while (colliders.Length > 0)
                    {
                        spawnPosition.x = originalPosition.x + (createOnXAxis ? offset : 0f);
                        spawnPosition.z = originalPosition.z + (createOnXAxis ? 0f : offset);

                        colliders = Physics.OverlapSphere(spawnPosition, 0.5f);
                        offset += 1.0f;
                    }
                }

                GameObject newObject = Instantiate(selectedPrefabs[i], spawnPosition, Quaternion.Euler(previewRotation.eulerAngles));
                Selection.activeGameObject = newObject;

                // Register the object for Undo
                Undo.RegisterCreatedObjectUndo(newObject, "Prefab Creation");
                break; // Create only one prefab instance
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < previewObjects.Length; i++)
        {
            if (previewObjects[i] != null)
            {
                DestroyImmediate(previewObjects[i]);
            }
        }
    }
}