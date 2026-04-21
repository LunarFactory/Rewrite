using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Compensated", menuName = "Effects/Buff/Compensated")]
public class CompensatedEffect : StatusEffectData
{
    public int bonusDamage = 5;

    public override ActiveEffect CreateEffect() => new CompensatedActiveEffect();
}