using Entity;
using UnityEngine;

public class RadioactiveActiveEffect : ActiveEffect
{
    private float _tickTimer = 0f;
    private RadioactiveEffect _radioactiveEffectData;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        _radioactiveEffectData = Data as RadioactiveEffect;
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = Color.green;
    }

    public override void OnUpdate(EntityStats target, float deltaTime, EntityStats source)
    {
        base.OnUpdate(target, deltaTime, source);

        _tickTimer += deltaTime;
        if (_tickTimer >= 1f) // 1초마다 틱 데미지
        {
            _tickTimer -= 1.0f;
            int totalDamage = Mathf.RoundToInt(
                source.AttackDamage.GetValue() * _radioactiveEffectData.damagePerStack * stacks
            );
            target.TakeDamage(source, totalDamage);
        }
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null)
            sr.color = target.GetOriginalColor();
    }
}
