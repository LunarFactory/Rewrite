using UnityEngine;
using Entity;
using Player;

public class AssistedActiveEffect : ActiveEffect
{
    private StatModifier _speedMod;
    private StatModifier _damageMod;
    private AssistedEffect _data;
    private int _currentStack = 0;
    private int maxStack;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        _data = (AssistedEffect)Data;
        maxStack = _data.maxStack;
        if (target is PlayerStats player)
        {
            _currentStack = 0;
            // 1. 공격 적중 이벤트 구독
            player.OnPlayerAttackHit += HandleAttackHit;
            Debug.Log("<color=orange>[조준보조]</color> 효과 시작! 공격 시 속도가 증가합니다.");
        }
    }

    private void HandleAttackHit(PlayerStats player, EntityStats target, int damage)
    {
        if (_currentStack < maxStack)
        {
            _currentStack++;
            UpdateModifiers(player);
        }
    }

    private void UpdateModifiers(PlayerStats player)
    {
        // 기존 수정자 제거 (Source 기반)
        player.AttackSpeed.RemoveModifiersFromSource(this);
        player.DamageIncreased.RemoveModifiersFromSource(this);

        // 2. 공격 속도 수정자 갱신 (중첩당 4%)
        float speedBonus = _currentStack * _data.bonusAttackSpeed;
        _speedMod = new StatModifier("AssistedAttackSpeed", speedBonus, ModifierType.Percent, this);
        player.AttackSpeed.AddModifier(_speedMod);

        // 3. 최대 중첩(100%) 도달 시 모든 피해 50% 증가
        if (_currentStack >= maxStack)
        {
            _damageMod = new StatModifier("AssistedDamageIncreased_MaxBonus", _data.bonusDamageIncreased, ModifierType.Percent, this);
            player.DamageIncreased.AddModifier(_damageMod);
            Debug.Log("<color=red>[조준보조]</color> 최대 중첩 달성! 모든 피해 50% 증가!");
        }

        // 디버그용 (필요 없으면 삭제)
        Debug.Log($"[조준보조] {_currentStack}중첩 (공속 +{speedBonus * 100}%)");
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        if (target is PlayerStats player)
        {
            // 4. 이벤트 구독 해제 (중요: 메모리 누수 방지)
            player.OnPlayerAttackHit -= HandleAttackHit;

            // 5. 모든 수정자 제거
            player.AttackSpeed.RemoveModifiersFromSource(this);
            player.DamageIncreased.RemoveModifiersFromSource(this);

            _currentStack = 0;
            Debug.Log("[조준보조] 효과 종료.");
        }
    }
}