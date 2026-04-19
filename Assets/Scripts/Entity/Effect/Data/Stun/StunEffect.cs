using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Stun")]
public class StunEffect : StatusEffectData
{
    public override ActiveEffect CreateEffect() => new StunActiveEffect();
}