using UnityEngine;
using Entity;

public class HeatedActiveEffect : ActiveEffect
{
    private float _tickTimer = 0f;
    private HeatedEffect _heatedData;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        // 데이터를 EngagedEffect로 캐스팅하여 damage 수치에 접근합니다.
        _heatedData = Data as HeatedEffect;
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.red;

        Debug.Log($"{target.name}에게 Heated 효과 시작! (초당 피해: {_heatedData.damage})");
    }

    public override void OnUpdate(EntityStats target, float deltaTime, EntityStats source)
    {
        // 1. 부모의 OnUpdate를 호출하여 RemainingTime을 감소시킵니다.
        base.OnUpdate(target, deltaTime, source);

        if (_heatedData == null) return;

        // 2. 타이머에 프레임 시간을 더합니다.
        _tickTimer += deltaTime;

        // 3. 타이머가 1초가 넘었을 때마다 데미지를 입힙니다.
        if (_tickTimer >= 1.0f)
        {
            // 정확한 주기 유지를 위해 1.0f를 뺍니다.
            _tickTimer -= 1.0f;

            // 정해진 초당 데미지를 입힙니다.
            if (target is Enemy.EnemyStats enemy) enemy.TakeDamage(source, _heatedData.damage, Color.red);
            else target.TakeDamage(source, _heatedData.damage);

            Debug.Log($"{target.name}이 Heated 피해를 입음: {_heatedData.damage} (남은 시간: {RemainingTime:F1}초)");
        }
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {

        SpriteRenderer sr = target.GetRenderer();
        if (sr != null) 
            sr.color = target.GetOriginalColor();
        Debug.Log($"{target.name}의 Heated 효과가 만료되었습니다.");
    }
}