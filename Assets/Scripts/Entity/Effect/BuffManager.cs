using System.Collections.Generic;
using Enemy;
using Entity;
using UnityEditor.Rendering;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    private EntityStats _stats;

    // 1. 현재 쌓이고 있는 스택들
    public Dictionary<StatusEffectData, int> _stackCounters =
        new Dictionary<StatusEffectData, int>();

    // 2. 현재 활성화된 버프/디버프들
    public List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

    private void Awake()
    {
        _stats = gameObject.GetComponent<EntityStats>();
    }

    public void AddStack(StatusEffectData data, int amount, EntityStats source)
    {
        if (data == null)
            return;

        if (!_stackCounters.ContainsKey(data))
            _stackCounters[data] = 0;

        _stackCounters[data] += amount;

        if (_stackCounters[data] >= data.GetMaxStack())
        {
            _stackCounters[data] = 0;
            data.OnStackFull(this, source);
        }
    }

    public void ApplyEffect(
        StatusEffectData data,
        float duration,
        EntityStats source,
        bool infinity = false
    )
    {
        var stats = GetComponent<EntityStats>();
        if (stats is EnemyStats enemy)
        {
            if (enemy.isBoss && data.isHard)
            {
                return;
            }
        }
        // 중복 체크 및 시간 갱신 로직
        ActiveEffect existingEffect = HasEffect(data);

        if (existingEffect != null)
        {
            if (data.stackable)
            {
                existingEffect.AddStack();
            }
            existingEffect.ResetTime(duration);
        }
        else
        {
            ActiveEffect newEffect = data.CreateEffect();
            if (newEffect != null)
            {
                newEffect.Initialize(data, duration, source, infinity);
                newEffect.OnStart(_stats, source);
                _activeEffects.Add(newEffect);
                if (data.isHard)
                {
                    source.NotifyHardCC(source, _stats);
                }
            }
        }
    }

    public ActiveEffect HasEffect(StatusEffectData data)
    {
        // 중복 체크 및 시간 갱신 로직
        ActiveEffect existingEffect = _activeEffects.Find(e =>
            e.Data == data || (e.Data != null && e.Data.effectName == data.effectName)
        );

        if (existingEffect != null)
        {
            Debug.Log("너 버프 있대");
            return existingEffect;
        }
        else
        {
            Debug.Log("너 버프 없대");
            return null;
        }
    }

    private void Update()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.OnUpdate(_stats, Time.deltaTime, effect.Source);

            if (effect.IsFinished)
            {
                effect.OnEnd(_stats, effect.Source);
                _activeEffects.RemoveAt(i);
            }
        }
    }
}
