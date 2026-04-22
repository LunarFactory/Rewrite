using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Lightning", menuName = "Effects/Debuff/Lightning")]
public class LightningEffect : StatusEffectData
{
    public float searchRange;
    public float damageMultiplier; // 3000%
    public float duration;

    public override ActiveEffect CreateEffect() => new LightningActiveEffect();
}