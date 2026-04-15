using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item.PassiveItemData> items = new List<Items.PassiveItemData>();
    
    // 아이템이 추가될 때마다 실행될 이벤트 (UI나 플레이어 스탯 갱신용)
    public event Action OnItemAdded;

    private void Awake() => Instance = this;

    public void AddItem(Items.PassiveItemData newItem)
    {
        items.Add(newItem);
        Debug.Log($"{newItem.itemName} 획득!");
        
        // 스탯 재계산 및 UI 업데이트 알림
        OnItemAdded?.Invoke();
    }
}