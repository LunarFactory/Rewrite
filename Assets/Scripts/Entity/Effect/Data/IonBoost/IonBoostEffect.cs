using UnityEngine;

[CreateAssetMenu(fileName = "Effect_IonBoost", menuName = "Effects/Buff/Ion Boost")]
public class IonBoostEffect : StatusEffectData
{
    public float bonusAttackSpeed = 0.5f;

    public override ActiveEffect CreateEffect() => new IonBoostActiveEffect();
}