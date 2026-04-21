using UnityEngine;
using Entity;

[System.Serializable]
public abstract class ActiveEffect
{
    public StatusEffectData Data { get; private set; }
    public float RemainingTime { get; private set; }
    public EntityStats Source { get; private set; }

    public void Initialize(StatusEffectData data, float duration, EntityStats source)
    {
        Data = data;
        RemainingTime = duration;
        Source = source;
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

    public virtual void OnStart(EntityStats target, EntityStats source) { }
    public virtual void OnUpdate(EntityStats target, float deltaTime, EntityStats source) 
    {
        RemainingTime -= deltaTime;
    }
    public virtual void OnEnd(EntityStats target, EntityStats source) { }

    public bool IsFinished => RemainingTime <= 0;
}