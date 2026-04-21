using UnityEngine;
using Entity;

public abstract class StatusEffectData : ScriptableObject
{
    public string effectName;
    public Sprite icon;
    public virtual void OnStackFull(BuffManager manager, EntityStats source) {}
    // 이 데이터에 대응하는 실제 '실행 객체'를 생성하는 함수
    public virtual ActiveEffect CreateEffect() => null;
    public virtual int GetMaxStack()
    {
        return 0;
    }
}