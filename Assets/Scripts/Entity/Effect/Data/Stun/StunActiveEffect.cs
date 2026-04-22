using Entity;
using UnityEngine;

public class StunActiveEffect : ActiveEffect
{
    public override void OnStart(EntityStats target, EntityStats source)
    {
        // 기존 StunRoutine의 시작 부분
        target.isStunned = true;

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.yellow;
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        // 기존 StunRoutine의 종료 부분
        target.isStunned = false;

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = target.GetOriginalColor();
    }
}
