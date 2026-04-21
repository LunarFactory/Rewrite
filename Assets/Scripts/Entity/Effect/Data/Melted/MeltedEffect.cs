using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Melted", menuName = "Effects/Debuff/Melted")]
public class MeltedEffect : StatusEffectData
{
    private int stack = 0;
    public float damageTaken;
    public float moveSpeed;

    public override ActiveEffect CreateEffect() => new MeltedActiveEffect();

    public int getCurrentStack()
    {
        return stack;
    }
    public void setCurrentStack(int amount)
    {
        stack = amount;
    }
}