using System.Collections.Generic;
using System.Collections.ObjectModel;

[System.Serializable]
public class CharacterStat
{
    public float BaseValue; // 기본값
    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    // 외부에서 버프 리스트를 함부로 건드리지 못하게 읽기 전용으로 제공
    public void AddModifier(StatModifier mod) => _modifiers.Add(mod);
    
    // 특정 아이템이 준 버프만 골라 제거하기 위함
    public void RemoveModifiersFromSource(object source) => _modifiers.RemoveAll(m => m.Source == source);

    public CharacterStat(float value)
    {
        this.BaseValue = value;
    }

    public float GetValue()
    {
        float finalValue = BaseValue;
        float percentSum = 0;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            StatModifier mod = _modifiers[i];

            if (mod.Type == ModifierType.Flat)
            {
                finalValue += mod.Value; // 고정치 먼저 더하기
            }
            else if (mod.Type == ModifierType.Percent)
            {
                percentSum += mod.Value; // 퍼센트는 따로 합산 (복리 방지용)
            }
        }

        // 최종 합산 퍼센트 적용 (예: 10% + 20% = 30% 증가)
        finalValue *= 1 + percentSum;

        return finalValue;
    }
}