using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Stun", menuName = "Effects/Debuff/Stun")]
public class StunEffect : StatusEffectData
{
    public override ActiveEffect CreateEffect() => new StunActiveEffect();
}