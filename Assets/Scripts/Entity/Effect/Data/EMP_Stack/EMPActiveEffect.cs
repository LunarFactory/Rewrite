using Enemy;
using Entity;
using Player;
using UnityEngine;

public class EMPActiveEffect : ActiveEffect
{
    private StatModifier _moveSpeedMod;
    private EMPEffect _data;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        _data = (EMPEffect)Data;

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.blue;

        if (source is PlayerStats player && target is EnemyStats enemy)
        {
            _moveSpeedMod = new StatModifier(
                "EMPMoveSpeed",
                _data.moveSpeed,
                ModifierType.Percent,
                this
            );
            enemy.MoveSpeed.AddModifier(_moveSpeedMod);
        }
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        if (source is PlayerStats player)
        {
            // 5. 모든 수정자 제거
            target.MoveSpeed.RemoveModifiersFromSource(this);

            SpriteRenderer sr = target.GetRenderer();
            if (sr != null)
                sr.color = target.GetOriginalColor();
        }
    }
}
