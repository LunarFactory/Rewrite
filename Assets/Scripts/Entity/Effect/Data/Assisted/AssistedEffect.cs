using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Assisted", menuName = "Effects/Buff/Assisted")]
public class AssistedEffect : StatusEffectData
{
    private int stack = 0;
    public float bonusAttackSpeed;

    public override ActiveEffect CreateEffect() => new AssistedActiveEffect();

    public int getCurrentStack()
    {
        return stack;
    }
    public void setCurrentStack(int amount)
    {
        stack = amount;
    }
}