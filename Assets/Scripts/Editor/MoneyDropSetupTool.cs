using UnityEditor;
using UnityEngine;
using Level;
using Enemy;

namespace Editor
{
    public class MoneyDropSetupTool : EditorWindow
    {
        [MenuItem("Tools/Setup Money Drop")]
        public static void Setup()
        {
            // 1. Bolt Sprite 확인
            Sprite boltSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bolt_currency.png");
            if (boltSprite == null)
            {
                Debug.LogError("Bolt sprite not found at Assets/Sprites/bolt_currency.png. Please ensure the sprite exists.");
                return;
            }

            // 2. Prefabs 디렉토리 생성
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Items"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Items");

            string prefabPath = "Assets/Prefabs/Items/Bolt.prefab";

            // 3. Bolt 프리팹 생성/갱신
            GameObject boltGo = new GameObject("Bolt_Temporary");
            try
            {
                // 비주얼 추가
                GameObject visualGo = new GameObject("Visual");
                visualGo.transform.SetParent(boltGo.transform);
                SpriteRenderer sr = visualGo.AddComponent<SpriteRenderer>();
                sr.sprite = boltSprite;
                sr.color = new Color(1f, 0.9f, 0.2f); // 황금빛 노란색

                // 컴포넌트 추가
                boltGo.AddComponent<CircleCollider2D>().isTrigger = true;
                BoltItem boltItem = boltGo.AddComponent<BoltItem>();
                boltItem.value = 1;
                boltItem.pickupRange = 2.0f;
                boltItem.moveSpeed = 8.0f;

                // 프리팹 저장
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(boltGo, prefabPath);
                Debug.Log($"<color=green>SUCCESS:</color> Bolt Prefab created/updated at {prefabPath}");

                // 4. 모든 Enemy 프리팹에 BoltPrefab 할당
                AssignBoltToEnemies(prefab);
            }
            finally
            {
                Object.DestroyImmediate(boltGo);
            }
        }

        private static void AssignBoltToEnemies(GameObject boltPrefab)
        {
            string[] enemyGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            int count = 0;

            foreach (string guid in enemyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (go != null && go.TryGetComponent<EnemyBase>(out EnemyBase enemy))
                {
                    // 프리팹 직접 수정
                    SerializedObject so = new SerializedObject(enemy);
                    so.FindProperty("BoltPrefab").objectReferenceValue = boltPrefab;
                    so.ApplyModifiedProperties();
                    
                    EditorUtility.SetDirty(go);
                    count++;
                }
            }

            Debug.Log($"<color=cyan>INFO:</color> Assigned BoltPrefab to {count} enemies.");
            AssetDatabase.SaveAssets();
        }
    }
}
