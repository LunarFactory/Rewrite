using UnityEngine;

[CreateAssetMenu(fileName = "CompensatedBuffData", menuName = "Effects/Compensated")]
public class CompensatedEffect : StatusEffectData
{
    public int bonusDamage = 5;

    public override ActiveEffect CreateEffect() => new CompensatedActiveEffect();
}