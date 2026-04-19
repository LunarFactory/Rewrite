using UnityEngine;
using Entity;

[System.Serializable]
public abstract class ActiveEffect
{
    public StatusEffectData Data { get; private set; }
    public float RemainingTime { get; private set; }

    public void Initialize(StatusEffectData data, float duration)
    {
        Data = data;
        RemainingTime = duration;
    }
    public void ResetTime(float duration)
    {
        if (Data != null)
        {
            RemainingTime = duration;
            Debug.Log($"{Data.effectName}의 지속시간이 갱신되었습니다.");
            Debug.Log($"남은 시간 : {RemainingTime}.");
        }
    }

    public virtual void OnStart(EntityStatus target) { }
    public virtual void OnUpdate(EntityStatus target, float deltaTime) 
    {
        RemainingTime -= deltaTime;
    }
    public virtual void OnEnd(EntityStatus target) { }

    public bool IsFinished => RemainingTime <= 0;
}