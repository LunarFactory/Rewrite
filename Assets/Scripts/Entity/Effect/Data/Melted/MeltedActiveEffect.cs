using UnityEngine;
using Entity;
using Player;
using Enemy;

public class MeltedActiveEffect : ActiveEffect
{
    private StatModifier _moveSpeedMod;
    private StatModifier _damageTakenMod;
    private MeltedEffect _data;
    private int _currentStack = 0;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        _data = (MeltedEffect)Data;

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.yellow;

        if (source is PlayerStats player && target is EnemyStats enemy)
        {
            _moveSpeedMod = new StatModifier("MeltedMoveSpeed", _data.moveSpeed, ModifierType.Percent, this);
            enemy.MoveSpeed.AddModifier(_moveSpeedMod);
            _currentStack = 0;
            // 1. 공격 적중 이벤트 구독
            player.OnPlayerAttackHit += HandleAttackHit;
            Debug.Log("<color=orange>[융해]</color> 효과 시작! 이동 속도가 감소하고 받는 피해가 증가합니다.");
        }
    }

    private void HandleAttackHit(PlayerStats player, EntityStats target, int damage)
    {
        _currentStack++;
        UpdateModifiers((EnemyStats)target);
    }

    private void UpdateModifiers(EnemyStats enemy)
    {
        // 기존 수정자 제거 (Source 기반)
        enemy.DamageTaken.RemoveModifiersFromSource(this);

        // 2. 공격 속도 수정자 갱신 (중첩당 4%)
        float damageTaken = _currentStack * _data.damageTaken;
        _damageTakenMod = new StatModifier("MeltedDamageTaken", damageTaken, ModifierType.Percent, this);
        enemy.DamageTaken.AddModifier(_damageTakenMod);

        // 디버그용 (필요 없으면 삭제)
        Debug.Log($"[융해] {_currentStack}중첩 (받는 피해량 +{damageTaken * 100}%)");
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        if (source is PlayerStats player)
        {
            // 4. 이벤트 구독 해제 (중요: 메모리 누수 방지)
            player.OnPlayerAttackHit -= HandleAttackHit;

            // 5. 모든 수정자 제거
            target.MoveSpeed.RemoveModifiersFromSource(this);
            target.DamageTaken.RemoveModifiersFromSource(this);

            _currentStack = 0;

            SpriteRenderer sr = target.GetRenderer();
            if (sr != null)
                sr.color = target.GetOriginalColor();

            Debug.Log("[융해] 효과 종료.");
        }
    }
}