using UnityEngine;
using UnityEditor;

public class PrefabPreviewTool : EditorWindow
{
    private GameObject[] selectedPrefabs = new GameObject[3];
    private bool[] prefabActive = new bool[3];
    private Quaternion previewRotation = Quaternion.identity;
    private GameObject[] previewObjects = new GameObject[3];
    private Vector3 spawnPosition;

    [MenuItem("Tools/Prefab Preview Tool")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPreviewTool>("Prefab Preview Tool");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
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

    private void DuringSceneGUI(SceneView sceneView)
    {
        for (int i = 0; i < selectedPrefabs.Length; i++)
        {
            if (selectedPrefabs[i] != null && prefabActive[i])
            {
                Event e = Event.current;

                if (e.type == EventType.MouseMove || e.type == EventType.Repaint)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        spawnPosition = hit.point;
                        spawnPosition.y = 0; // Forza la componente Y a zero

                        if (previewObjects[i] == null)
                        {
                            previewObjects[i] = Instantiate(selectedPrefabs[i]);
                        }

                        previewObjects[i].transform.position = SnapToGrid(spawnPosition);
                        previewObjects[i].transform.rotation = previewRotation;

                        sceneView.Repaint();
                    }
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    CreatePrefabInstance();
                    e.Use();
                }
            }
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

            previewObjects[index].transform.position = SnapToGrid(spawnPosition);
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
                Vector3 snappedPosition = SnapToGrid(spawnPosition);
                GameObject newObject = Instantiate(selectedPrefabs[i], snappedPosition, Quaternion.Euler(previewRotation.eulerAngles));
                Selection.activeGameObject = newObject;

                // Register the object for Undo
                Undo.RegisterCreatedObjectUndo(newObject, "Prefab Creation");
                break; // Create only one prefab instance
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(Mathf.Round(position.x), 0, Mathf.Round(position.z));
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