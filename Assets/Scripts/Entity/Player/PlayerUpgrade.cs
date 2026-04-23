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

        public void ApplyUpgradeTo(PlayerStats stats, int level)
        {
            if (level <= 0)
                return;

            int index = Mathf.Clamp(level - 1, 0, statOffsets.Length - 1);
            float offsetValue = statOffsets[index] / 100f; // 퍼센트 수치 적용

            StatModifier modifier = new StatModifier(
                $"Meta_{id}",
                offsetValue,
                ModifierType.Percent,
                this
            );

            switch (statType)
            {
                case StatType.Damage:
                    stats.AttackDamage.AddModifier(modifier);
                    break;
                case StatType.AttackSpeed:
                    stats.AttackSpeed.AddModifier(modifier);
                    break;
                case StatType.MoveSpeed:
                    stats.MoveSpeed.AddModifier(modifier);
                    break;
            }
        }
    }

    public enum StatType
    {
        Damage,
        AttackSpeed,
        MoveSpeed,
    }
}
