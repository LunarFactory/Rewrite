using UnityEngine;
using Entity;

public class EngagedActiveEffect : ActiveEffect
{
    private float _tickTimer;
    private EngagedEffect _engagedData;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        // 데이터를 EngagedEffect로 캐스팅하여 damage 수치에 접근합니다.
        _engagedData = Data as EngagedEffect;
        _tickTimer = 0f;
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.magenta;

        Debug.Log($"{target.name}에게 Engaged 효과 시작! (초당 피해: {_engagedData.damage})");
    }

    public override void OnUpdate(EntityStats target, float deltaTime, EntityStats source)
    {
        // 1. 부모의 OnUpdate를 호출하여 RemainingTime을 감소시킵니다.
        base.OnUpdate(target, deltaTime, source);

        if (_engagedData == null) return;

        // 2. 타이머에 프레임 시간을 더합니다.
        _tickTimer += deltaTime;

        // 3. 타이머가 1초가 넘었을 때마다 데미지를 입힙니다.
        if (_tickTimer >= 1.0f)
        {
            // 정확한 주기 유지를 위해 1.0f를 뺍니다.
            _tickTimer -= 1.0f;

            // 정해진 초당 데미지를 입힙니다.
            if (target is Enemy.EnemyStats enemy) enemy.TakeDamage(source, _engagedData.damage, Color.magenta);
            else target.TakeDamage(source, _engagedData.damage);

            Debug.Log($"{target.name}이 Engaged 피해를 입음: {_engagedData.damage} (남은 시간: {RemainingTime:F1}초)");
        }
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null) 
            sr.color = target.GetOriginalColor();
        Debug.Log($"{target.name}의 Engaged 효과가 만료되었습니다.");
    }
}