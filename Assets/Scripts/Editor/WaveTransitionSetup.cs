using UnityEngine;
using UnityEditor;
using Core;
using Level;
using System.IO;

namespace Editor
{
    public class WaveTransitionSetup : EditorWindow
    {
        [MenuItem("Tools/Setup Wave Transition")]
        public static void CreateTransitionPrefab()
        {
            // 1. Find the base prefab
            string[] guids = AssetDatabase.FindAssets("Station_SupplyPort t:Prefab");
            if (guids.Length == 0)
            {
                Debug.LogError("Station_SupplyPort prefab not found!");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // 2. Create directory if not exists
            string folderPath = "Assets/Prefabs/Interactables";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 3. Instantiate and modify
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            go.name = "WaveTransitionPortal";

            // Cleanup old components if any
            var oldComp = go.GetComponent<SupplyPortStation>();
            if (oldComp != null) DestroyImmediate(oldComp);

            // Add new component
            if (go.GetComponent<WaveTransitionTrigger>() == null)
            {
                go.AddComponent<WaveTransitionTrigger>();
            }

            // 4. Save as new prefab
            string newPath = folderPath + "/WaveTransitionPortal.prefab";
            GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(go, newPath);
            DestroyImmediate(go);

            Debug.Log($"<color=green>SUCCESS:</color> Created {newPath}");

            // 5. Assign to WaveManager in current scene
            WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
            if (waveManager != null)
            {
                SerializedObject so = new SerializedObject(waveManager);
                SerializedProperty prop = so.FindProperty("waveTransitionPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = newPrefab;
                    so.ApplyModifiedProperties();
                    Debug.Log("<color=cyan>INFO:</color> Assigned to WaveManager in scene.");
                }
                else
                {
                    Debug.LogError("Could not find waveTransitionPrefab field in WaveManager! Check if you updated the script.");
                }
            }
            else
            {
                Debug.LogWarning("WaveManager not found in scene. Please assign the prefab manually to WaveManager.");
            }
        }
    }
}
