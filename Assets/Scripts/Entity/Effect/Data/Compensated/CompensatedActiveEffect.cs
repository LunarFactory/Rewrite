using Entity;
using UnityEngine;

public class CompensatedActiveEffect : ActiveEffect
{
    private StatModifier _mod;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        var stats = target.GetComponent<EntityStats>();
        var data = (CompensatedEffect)Data;

        if (stats != null)
        {
            // 1. 수정자 생성 (Source는 이 효과 객체인 this)
            _mod = new StatModifier(
                "CompensatedAttackDamage",
                data.bonusDamage,
                ModifierType.Flat,
                this
            );

            // 2. 공격력 추가
            stats.AttackDamage.AddModifier(_mod);
        }
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        var stats = target.GetComponent<EntityStats>();
        if (stats != null)
        {
            // 3. 정확히 이 버프가 추가했던 수정자만 제거
            stats.AttackDamage.RemoveModifiersFromSource(this);
        }
    }
}
