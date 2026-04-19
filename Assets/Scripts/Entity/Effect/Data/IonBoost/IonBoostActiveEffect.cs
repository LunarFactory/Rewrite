using UnityEngine;
using Entity;

public class IonBoostActiveEffect : ActiveEffect 
{
    private StatModifier _mod;

    public override void OnStart(EntityStatus target) 
    {
        var stats = (Player.PlayerStats)target.GetComponent<EntityStatus>();
        var data = (IonBoostEffect)Data;

        if (stats != null)
        {
            // 1. 수정자 생성 (Source는 이 효과 객체인 this)
            _mod = new StatModifier(data.bonusAttackSpeed, ModifierType.Percent, this);
            
            // 2. 공격력 추가
            stats.AttackSpeed.AddModifier(_mod);
            Debug.Log($"<color=orange>[이온부스트]</color> 공격 속도 +{100 * data.bonusAttackSpeed}% 버프 시작!");
        }
    }

    public override void OnEnd(EntityStatus target) 
    {
        var stats = (Player.PlayerStats)target.GetComponent<EntityStatus>();
        if (stats != null)
        {
            // 3. 정확히 이 버프가 추가했던 수정자만 제거
            stats.AttackSpeed.RemoveModifiersFromSource(this);
            Debug.Log("[이온부스트] 버프 종료.");
        }
    }
}