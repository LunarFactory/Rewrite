using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class ItemCleanupTool : EditorWindow
    {
        [MenuItem("Tools/Adjust Bolt Size")]
        public static void AdjustBoltSize()
        {
            // Bolt 프리팹 찾기
            string guid = "b9d51bf2030b15f40b62865d7cca59ee";
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Bolt prefab not found! GUID may have changed.");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            
            // 크기 대폭 축소 (사용자 요청: 0.02)
            prefabRoot.transform.localScale = new Vector3(0.02f, 0.02f, 1f);
            
            Debug.Log($"<color=green>SUCCESS:</color> Bolt prefab scale adjusted to {prefabRoot.transform.localScale}");

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            
            AssetDatabase.Refresh();
        }
    }
}
