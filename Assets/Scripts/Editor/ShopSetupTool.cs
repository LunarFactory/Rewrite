using UnityEditor;
using UnityEngine;
using Core;
using Item;
using System.Collections.Generic;
using Level;

namespace Editor
{
    public class ShopSetupTool : EditorWindow
    {
        [MenuItem("Tools/Setup Shop (Waves 4 & 8)")]
        public static void Setup()
        {
            // 1. 모든 PassiveItemData 찾기 및 등급별 가격 설정
            string[] guids = AssetDatabase.FindAssets("t:PassiveItemData");
            List<PassiveItemData> allItems = new List<PassiveItemData>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PassiveItemData item = AssetDatabase.LoadAssetAtPath<PassiveItemData>(path);
                
                if (item != null)
                {
                    // 가격이 0인 경우에만 기본 등급별 가격 설정
                    if (item.basePrice <= 0)
                    {
                        item.basePrice = GetDefaultBasePrice(item.tier);
                        EditorUtility.SetDirty(item);
                    }
                    
                    // 보스 등급 제외하고 리스트에 추가
                    if (item.tier != ItemTier.Boss)
                    {
                        allItems.Add(item);
                    }
                }
            }

            // 2. WaveManager 찾기 및 설정
            WaveManager waveManager = FindAnyObjectByType<WaveManager>();
            if (waveManager != null)
            {
                SerializedObject so = new SerializedObject(waveManager);
                
                // allItems 리스트 설정
                SerializedProperty allItemsProp = so.FindProperty("allItems");
                allItemsProp.ClearArray();
                for (int i = 0; i < allItems.Count; i++)
                {
                    allItemsProp.InsertArrayElementAtIndex(i);
                    allItemsProp.GetArrayElementAtIndex(i).objectReferenceValue = allItems[i];
                }

                // fieldItemPrefab 설정 (없을 경우 검색)
                SerializedProperty prefabProp = so.FindProperty("fieldItemPrefab");
                if (prefabProp.objectReferenceValue == null)
                {
                    string[] prefabGuids = AssetDatabase.FindAssets("FieldItem t:Prefab");
                    if (prefabGuids.Length > 0)
                    {
                        string pPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                        prefabProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                    }
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(waveManager);
                Debug.Log($"<color=green>SUCCESS:</color> WaveManager shop pool populated with {allItems.Count} items.");
            }
            else
            {
                Debug.LogError("WaveManager not found in the current scene.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("<color=cyan>INFO:</color> Tier-based prices assigned to item assets.");
        }

        private static int GetDefaultBasePrice(ItemTier tier)
        {
            switch (tier)
            {
                case ItemTier.Common:   return 25;
                case ItemTier.Uncommon: return 50;
                case ItemTier.Rare:     return 100;
                default:                return 0;
            }
        }
    }
}
