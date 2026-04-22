using System;
using Enemy;
using Entity;
using UnityEngine;

public class LightningActiveEffect : ActiveEffect
{
    private static Material _sharedLightningMaterial;
    private LightningEffect _lightningData;
    private EnemyStats enemy;

    public override void OnStart(EntityStats target, EntityStats source)
    {
        _lightningData = Data as LightningEffect;

        // 부착되는 순간 즉시 3000% 피해 (아이템 설명 참고)
        int damage = Mathf.RoundToInt(
            source.DamageIncreased.GetValue(
                source.AttackDamage.GetValue() * _lightningData.damageMultiplier
            )
        );
        enemy = target as EnemyStats;
        enemy.TakeDamage(source, damage, Color.yellow);
    }

    public override void OnEnd(EntityStats target, EntityStats source)
    {
        // 핵심 로직: 시간이 다 돼서 끝난 게 아니라, 적이 죽어서 끝나는 경우 전이 발생
        // (BuffManager에서 RemoveAt 시점에 OnEnd가 호출되는 것을 활용)
        if (target != null && target.isDead)
        {
            ChainToNextTarget(target, source);
        }
    }

    private void ChainToNextTarget(EntityStats currentTarget, EntityStats source)
    {
        // 1. 주변 적 탐색
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            currentTarget.transform.position,
            _lightningData.searchRange,
            LayerMask.GetMask("Enemy")
        );

        EntityStats closest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in cols)
        {
            if (col.gameObject == currentTarget.gameObject)
                continue; // 자기 자신 제외

            if (col.TryGetComponent(out EntityStats enemy) && !enemy.isDead)
            {
                float dist = Vector2.Distance(
                    currentTarget.transform.position,
                    enemy.transform.position
                );
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy;
                }
            }
        }

        // 2. 다음 적이 있다면 번개 전이
        if (closest != null)
        {
            BuffManager nextManager = closest.GetComponent<BuffManager>();
            if (nextManager != null)
            {
                // 번개 시각 효과 생성 (Neural Link 로직 활용 가능)
                CreateLightningVisual(currentTarget.transform.position, closest.transform.position);

                // 새 타겟에게 동일한 효과 적용 (다시 30초 시작)
                nextManager.ApplyEffect(Data, _lightningData.duration, source);
            }
        }
    }

    private void CreateLightningVisual(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new("Lightning_Rod_Line");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        if (_sharedLightningMaterial == null)
        {
            _sharedLightningMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        // 머티리얼 및 색상 설정
        lr.material = _sharedLightningMaterial;
        lr.startColor = Color.yellow; // 신경망 느낌의 민트색
        lr.endColor = Color.lightYellow;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.sortingOrder = 100;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // 0.15초 뒤에 선 제거 (잔상 효과)
        UnityEngine.Object.Destroy(lineObj, 0.15f);
    }
}
