using UnityEngine;

[CreateAssetMenu(fileName = "IonBoostBuffData", menuName = "Effects/Ion Boost")]
public class IonBoostEffect : StatusEffectData
{
    public float bonusAttackSpeed = 0.5f;

    public override ActiveEffect CreateEffect() => new IonBoostActiveEffect();
}