using UnityEngine;
using Entity;

public class StunActiveEffect : ActiveEffect 
{
    public override void OnStart(EntityStatus target) 
    {
        // 기존 StunRoutine의 시작 부분
        target.isStunned = true;
        
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null) 
            sr.color = Color.yellow;
            
        Debug.Log($"{target.gameObject.name} 기절 시작 (버프 시스템)");
    }

    public override void OnEnd(EntityStatus target) 
    {
        // 기존 StunRoutine의 종료 부분
        target.isStunned = false;

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null) 
            sr.color = target.GetOriginalColor();

        Debug.Log($"{target.gameObject.name} 기절 해제 (버프 시스템)");
    }
}