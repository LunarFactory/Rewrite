using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Heated", menuName = "Effects/Debuff/Heated")]
public class HeatedEffect : StatusEffectData
{
    public float duration;
    public int damage;

    public override ActiveEffect CreateEffect() => new HeatedActiveEffect();
}