using UnityEngine;

[CreateAssetMenu(fileName = "Effect_DelayedBomb", menuName = "Effects/Debuff/Delayed Bomb")]
public class DelayedBombEffect : StatusEffectData
{
    public float damageMultiplier;
    public float explosionRadius;
    public StunEffect stunEffect;
    public float stunTime;

    public override ActiveEffect CreateEffect() => new DelayedBombActiveEffect();
}