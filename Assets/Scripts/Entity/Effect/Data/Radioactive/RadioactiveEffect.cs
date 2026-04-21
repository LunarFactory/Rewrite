using UnityEngine;
using Entity;

[CreateAssetMenu(fileName = "Effect_Radioactive", menuName = "Effects/Debuff/Radioactive")]
public class RadioactiveEffect : StatusEffectData
{
    public float damagePerStack = 1f;
    public int maxStack = 4;
    public override ActiveEffect CreateEffect() => new RadioactiveActiveEffect();
    public override void OnStackFull(BuffManager manager, EntityStats source)
    {
        manager.ApplyEffect(this, 0, source, true);
    }
    public override int GetMaxStack()
    {
        return maxStack;
    }
}