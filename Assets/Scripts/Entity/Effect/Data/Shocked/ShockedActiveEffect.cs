using UnityEngine;
using Entity;

public class ShockedActiveEffect : ActiveEffect 
{
    private StatModifier _mod;

    public override void OnStart(EntityStats target, EntityStats source) 
    {
        var stats = target.GetComponent<EntityStats>();
        var data = (ShockedEffect)Data;

        if (stats != null)
        {
            // 1. 수정자 생성 (Source는 이 효과 객체인 this)
            _mod = new StatModifier("ShockedDamageTaken", data.damageTaken, ModifierType.Percent, this);
            
            // 2. 공격력 추가
            stats.DamageTaken.AddModifier(_mod);
            Debug.Log($"<color=orange>[충격파]</color> 받는 피해 +{data.damageTaken * 100}% 디버프 시작!");
        }
    }

    public override void OnEnd(EntityStats target, EntityStats source) 
    {
        var stats = target.GetComponent<EntityStats>();
        if (stats != null)
        {
            // 3. 정확히 이 버프가 추가했던 수정자만 제거
            stats.DamageTaken.RemoveModifiersFromSource(this);
            Debug.Log("[충격파] 디버프 종료.");
        }
    }
}