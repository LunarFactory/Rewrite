using UnityEngine;
using Player;

namespace Item
{
    // 아이템의 개별 효과를 정의하는 베이스 (데이터가 아닌 '동작' 정의)
    public abstract class ItemEffect : ScriptableObject
    {
        // 아이템을 획득했을 때 실행될 로직
        public abstract void OnApply(GameObject player, PlayerStats stats);
    }
}