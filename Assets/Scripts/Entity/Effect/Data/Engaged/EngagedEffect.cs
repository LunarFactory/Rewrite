using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Engaged", menuName = "Effects/Debuff/Engaged")]
public class EngagedEffect : StatusEffectData
{
    public float duration;
    public int damage;

    public override ActiveEffect CreateEffect() => new EngagedActiveEffect();
}