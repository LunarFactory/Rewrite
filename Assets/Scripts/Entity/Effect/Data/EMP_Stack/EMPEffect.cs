using Entity;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect_EMP", menuName = "Effects/Stack/EMP")]
public class EMPEffect : StatusEffectData
{
    [Header("Result Effect")]
    public float moveSpeed;
    public float duration;

    public override ActiveEffect CreateEffect() => new EMPActiveEffect();
}
