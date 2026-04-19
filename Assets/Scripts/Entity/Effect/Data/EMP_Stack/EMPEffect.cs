using UnityEngine;

[CreateAssetMenu(fileName = "EMP_EffectData", menuName = "Effects/EMP")]
public class EMPEffect : StatusEffectData
{
    [Header("Result Effect")]
    public float duration = 2f;
    public int maxStack = 10;
    public StatusEffectData actualStunEffect;
    // EMP만의 특화 수치가 필요하다면 여기에 추가 (예: 기절 시 추가 데미지 등)
    public override void OnStackFull(BuffManager manager)
    {
        // EMP 스택이 다 차면, 실제 기절 효과를 발동시킴
        if (actualStunEffect != null)
        {
            manager.ApplyEffect(actualStunEffect, duration);
        }
    }
    public override int GetMaxStack()
    {
        return maxStack;
    }
}