using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Shocked", menuName = "Effects/Debuff/Shocked")]
public class ShockedEffect : StatusEffectData
{
    public float damageTaken;

    public override ActiveEffect CreateEffect() => new ShockedActiveEffect();
}