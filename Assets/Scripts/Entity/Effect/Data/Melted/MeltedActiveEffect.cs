using Enemy;
using Entity;
using Player;
using UnityEngine;

public class MeltedActiveEffect : ActiveEffect
{
    private StatModifier _moveSpeedMod;
    private StatModifier _damageTakenMod;
    private MeltedEffect _data;
    private int _currentStack = 0;
    private EntityStats _ownerTarget;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        _data = (MeltedEffect)Data;
        _ownerTarget = target;

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.yellow;

        if (source is PlayerStats player && target is EnemyStats enemy)
        {
            _moveSpeedMod = new StatModifier(
                "MeltedMoveSpeed",
                _data.moveSpeed,
                ModifierType.Percent,
                this
            );
            enemy.MoveSpeed.AddModifier(_moveSpeedMod);
            _currentStack = 0;
            // 1. 공격 적중 이벤트 구독
            player.OnPlayerAttackHit -= HandleAttackHit;
            player.OnPlayerAttackHit += HandleAttackHit;
        }
    }

    private void HandleAttackHit(PlayerStats player, EntityStats target, int damage)
    {
        if (target != _ownerTarget)
            return;
        _currentStack++;
        UpdateModifiers();
    }

    private void UpdateModifiers()
    {
        // 기존 수정자 제거 (Source 기반)
        _ownerTarget.DamageTaken.RemoveModifiersFromSource(this);

        // 2. 공격 속도 수정자 갱신 (중첩당 4%)
        float damageTaken = _currentStack * _data.damageTaken;
        _damageTakenMod = new StatModifier(
            "MeltedDamageTaken",
            damageTaken,
            ModifierType.Percent,
            this
        );
        _ownerTarget.DamageTaken.AddModifier(_damageTakenMod);
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
        }
    }
}
