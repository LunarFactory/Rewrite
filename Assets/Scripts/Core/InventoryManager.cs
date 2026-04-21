using System.Collections.Generic;
using UnityEngine;
using System;
using Item;   // PassiveItemData와 ItemEffect가 있는 네임스페이스
using Player; // PlayerStats가 있는 네임스페이스

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item.PassiveItemData> items = new List<Item.PassiveItemData>();

    // 아이템이 추가될 때마다 실행될 이벤트 (UI나 플레이어 스탯 갱신용)
    public event Action OnItemAdded;
    private PlayerStats _playerStats;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 플레이어 스탯 참조를 미리 가져옵니다.
        _playerStats = FindAnyObjectByType<PlayerStats>();
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(Item.PassiveItemData newItem)
    {
        if (HasItem(newItem))
        {
            Debug.Log($"{newItem.itemName}은(는) 이미 인벤토리에 있습니다.");
            return;
        }
        items.Add(newItem);
        Debug.Log($"{newItem.itemName} 획득!");

        newItem.OnApply(_playerStats);

        // 스탯 재계산 및 UI 업데이트 알림
        OnItemAdded?.Invoke();
    }
    public bool HasItem(Item.PassiveItemData targetItem)
    {
        if (targetItem == null) return false;

        // List.Contains는 리스트 내에 동일한 참조가 있는지 T/F로 반환합니다.
        return items.Contains(targetItem);
    }

    // 이름으로 찾고 싶을 때를 위한 오버로드 (선택 사항)
    public bool HasItem(string itemName)
    {
        return items.Exists(x => x.itemName == itemName);
    }
}