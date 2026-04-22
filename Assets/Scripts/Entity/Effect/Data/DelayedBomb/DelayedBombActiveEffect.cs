using UnityEngine;
using Unity;
using Entity;
using Enemy;
using System.Collections.Generic;

public class DelayedBombActiveEffect : ActiveEffect
{
    private float _explosionRadius;
    private float _damageMultiplier; // 500%
    private float _stunTime; // 500%
    private StunEffect _stunData;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        var stats = target.GetComponent<EntityStats>();
        var data = (DelayedBombEffect)Data;
        _explosionRadius = data.explosionRadius;
        _damageMultiplier = data.damageMultiplier;
        _stunData = data.stunEffect;
        _stunTime = data.stunTime;

        // 부착 시 시각적 효과 (예: 적 몸에 붉은 점 생성 또는 깜빡임)
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null) sr.color = Color.orange;
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        // 1. 시간이 다 되어 끝난 경우(폭발)만 실행
        if (IsFinished)
        {
            Explode(target, source);
        }

        // 색상 복구
        SpriteRenderer sr = target.GetRenderer();
        if (sr != null) sr.color = target.GetOriginalColor();
    }

    private void Explode(EntityStats target, EntityStats source)
    {
        // 2. 주변 적 탐색
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(target.transform.position, _explosionRadius, LayerMask.GetMask("Enemy"));

        int finalDamage = Mathf.RoundToInt(source.DamageIncreased.GetValue(source.AttackDamage.GetValue() * _damageMultiplier));

        foreach (var col in hitEnemies)
        {
            if (col.TryGetComponent<EnemyStats>(out var enemy))
            {
                // 3. 500% 피해 입힘 ($Damage \times 5.0$)
                enemy.TakeDamage(source, finalDamage, Color.red);
                if (enemy.TryGetComponent<BuffManager>(out BuffManager buff))
                {
                    buff.ApplyEffect(_stunData, _stunTime, source);
                }
            }
        }

        // 5. 폭발 이펙트 생성 (VFX)
        // VFXManager.Instance.Play("Explosion_VFX", target.transform.position);
        Debug.Log($"<color=red>[폭발]</color> {target.name} 주변 적들에게 {finalDamage} 피해 및 기절!");
    }
}