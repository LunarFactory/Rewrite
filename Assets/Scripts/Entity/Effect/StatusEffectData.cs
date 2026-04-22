using Entity;
using UnityEngine;

public abstract class StatusEffectData : ScriptableObject
{
    public string effectName;
    public Sprite icon;
    public bool isHard;
    public bool stackable = false;

    public virtual void OnStackFull(BuffManager manager, EntityStats source) { }

    // 이 데이터에 대응하는 실제 '실행 객체'를 생성하는 함수
    public virtual ActiveEffect CreateEffect() => null;

    public virtual int GetMaxStack()
    {
        return 0;
    }
}
