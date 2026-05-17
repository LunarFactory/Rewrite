using System.Collections;
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
        enemy = target as EnemyStats;

        if (target == null || _lightningData == null)
            return;

        // [핵심 해결책] 즉시 데미지를 주지 않고, 코루틴을 통해 미세한 지연을 줍니다.
        // 이렇게 해야 BuffManager에 이 이펙트가 정상 등록된 후 데미지가 들어가서,
        // 적이 죽었을 때 다음 OnEnd(전이)가 정상적으로 트리거됩니다.
        target.StartCoroutine(ApplyDamageRoutine((EnemyStats)target, source));
    }

    private IEnumerator ApplyDamageRoutine(EnemyStats target, EntityStats source)
    {
        // 0.04초(약 2~3프레임) 지연으로 라이프사이클 꼬임 방지 및 체인 연출 극대화
        yield return new WaitForSeconds(0.04f);

        if (target == null || target.isDead || _lightningData == null)
            yield break;

        int damage = Mathf.RoundToInt(
            source.DamageIncreased.GetValue(
                source.AttackDamage.GetValue() * _lightningData.damageMultiplier
            )
        );
        target.TakeDamage(source, damage, Color.yellow);
    }
}
