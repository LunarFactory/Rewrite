using UnityEngine;
using Entity;

[System.Serializable]
public abstract class ActiveEffect
{
    public StatusEffectData Data { get; private set; }
    public float RemainingTime { get; private set; }
    public EntityStats Source { get; private set; }
    public bool Infinite;
    public int stacks = 1;

    public void Initialize(StatusEffectData data, float duration, EntityStats source, bool infinity = false)
    {
        Data = data;
        if (infinity) {
            Infinite = true;
            RemainingTime = 0;
        }
        else RemainingTime = duration;
        Source = source;
    }

    public virtual void AddStack()
    {
        stacks++;
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
        if (Infinite) return;
        else RemainingTime -= deltaTime;
    }
    public virtual void OnEnd(EntityStats target, EntityStats source) { }

    public bool IsFinished => !Infinite && RemainingTime <= 0;
}