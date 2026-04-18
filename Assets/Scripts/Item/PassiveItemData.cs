// PassiveItemData.cs
using UnityEngine;
using Core;
using Player;

namespace Item
{
    // 이제 이 자체로는 에셋을 만들 수 없고, 상속받아서 사용합니다.
    public abstract class PassiveItemData : ScriptableObject
    {
        public string itemName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemTier tier;

        // 아이템이 획득되었을 때 실행될 추상 함수
        public abstract void OnApply(GameObject player, PlayerStats stats);
    }
}