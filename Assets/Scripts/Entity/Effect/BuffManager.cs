using System.Collections.Generic;
using UnityEngine;
using Entity;

public class BuffManager : MonoBehaviour
{
    private EntityStatus _stats;

    // 1. 현재 쌓이고 있는 스택들
    public Dictionary<StatusEffectData, int> _stackCounters = new Dictionary<StatusEffectData, int>();

    // 2. 현재 활성화된 버프/디버프들
    public List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

    private void Awake()
    {
        _stats = gameObject.GetComponent<EntityStatus>();
    }

    public void AddStack(StatusEffectData data, int amount)
    {
        if (data == null) return;

        if (!_stackCounters.ContainsKey(data)) _stackCounters[data] = 0;

        _stackCounters[data] += amount;

        if (_stackCounters[data] >= data.GetMaxStack())
        {
            _stackCounters[data] = 0;
            data.OnStackFull(this);
        }
    }

    public void ApplyEffect(StatusEffectData data, float duration)
    {
        // 중복 체크 및 시간 갱신 로직
        ActiveEffect existingEffect = _activeEffects.Find(e =>
        e.Data == data || (e.Data != null && e.Data.effectName == data.effectName));

        if (existingEffect != null)
        {
            existingEffect.ResetTime(duration);
        }
        else
        {
            ActiveEffect newEffect = data.CreateEffect();
            if (newEffect != null)
            {
                newEffect.Initialize(data, duration);
                newEffect.OnStart(_stats);
                _activeEffects.Add(newEffect);
            }
        }
    }

    private void Update()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.OnUpdate(_stats, Time.deltaTime);

            if (effect.IsFinished)
            {
                effect.OnEnd(_stats);
                _activeEffects.RemoveAt(i);
            }
        }
    }
}