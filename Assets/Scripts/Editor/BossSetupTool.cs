using UnityEngine;
using UnityEditor;
using Core;
using Enemy;

namespace Editor
{
    public class BossSetupTool : EditorWindow
    {
        [MenuItem("Tools/Setup Boss Component")]
        public static void SetupBoss()
        {
            WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogError("WaveManager not found in scene!");
                return;
            }

            // Get the bossPrefab from WaveManager using SerializedObject
            SerializedObject so = new SerializedObject(waveManager);
            SerializedProperty bossProp = so.FindProperty("bossPrefab");
            
            if (bossProp.objectReferenceValue == null)
            {
                Debug.LogError("WaveManager's bossPrefab is not assigned!");
                return;
            }

            GameObject bossPrefab = (GameObject)bossProp.objectReferenceValue;
            string path = AssetDatabase.GetAssetPath(bossPrefab);

            // Open prefab, add/update component, and save
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            
            if (prefabRoot.GetComponent<BossEnemy>() == null)
            {
                // Remove old EnemyBase if it exists (since BossEnemy inherently has it)
                EnemyBase oldBase = prefabRoot.GetComponent<EnemyBase>();
                if (oldBase != null && oldBase.GetType() == typeof(EnemyBase))
                {
                    DestroyImmediate(oldBase);
                }
                
                prefabRoot.AddComponent<BossEnemy>();
                Debug.Log($"<color=green>SUCCESS:</color> Added BossEnemy to {bossPrefab.name}");
            }
            else
            {
                Debug.Log("<color=cyan>INFO:</color> BossEnemy already present on prefab.");
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            
            Debug.Log("<color=green>Boss Prefab Setup Completed!</color>");
        }
    }
}
