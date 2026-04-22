using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrade/Upgrade Data")]
    public class PlayerUpgradeData : ScriptableObject
    {
        public string id; // 저장을 위한 고유 ID
        public string upgradeName;
        public Sprite icon;

        [TextArea]
        public string description;

        [Header("Level Settings (Max 3)")]
        public float[] statOffsets = new float[3]; // 레벨별 증가 수치 (예: 0.1f, 0.2f, 0.3f)
        public int[] costs = new int[3]; // 레벨별 구매 비용

        public StatType statType; // 어떤 스탯을 올릴 것인가?
    }

    public enum StatType
    {
        Damage,
        AttackSpeed,
        MoveSpeed,
    }
}
