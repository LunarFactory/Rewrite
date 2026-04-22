using UnityEngine;
using System.Collections;
using Entity;
using Player;
using System;
using Unity.VisualScripting;

namespace Item
{
    [CreateAssetMenu(fileName = "SuperalloyExoskeleton", menuName = "Items/Boss/Superalloy Exoskeleton")]
    public class SuperalloyExoskeletonItem : PassiveItemData
    {
        public float cooldown = 20f;
        public float damageTaken = -0.2f;
        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<SuperalloyExoskeletonTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<SuperalloyExoskeletonTracker>();
                tracker.Initialize(player, damageTaken, cooldown);
            }
        }
    }

    public class SuperalloyExoskeletonTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _damageTaken;
        private float _cooldown;
        private StatModifier _damageTakenMod;
        private bool _shield = false; // 발동 여부 (비활성화 플래그)
        private bool _isCooldown = false; // 발동 여부 (비활성화 플래그)

        public void Initialize(PlayerStats player, float damageTaken, float cooldown)
        {
            _player = player;
            _damageTaken = damageTaken;
            _cooldown = cooldown;

            _damageTakenMod = new StatModifier("SuperalloyExoskeletonDamageTaken", _damageTaken, ModifierType.Percent, this);
            _player.DamageTaken.AddModifier(_damageTakenMod);

            _player.OnOverHeal += HandleItemEffect;
            _player.OnPreDamage += TriggerShield;
        }

        private void HandleItemEffect(EntityStats entity, int amount)
        {
            if (!_isCooldown)
            {
                _shield = true;
            }
        }

        private void TriggerShield(ref int damage)
        {
            if (_shield)
            {
                _shield = false; // 아이템 비활성화
                damage = 0;
                if (FDTManager.Instance != null)
                {
                    FDTManager.Instance.SpawnText(transform.position + Vector3.up * 0.5f, "방어!", Color.gold);
                }
                StartCoroutine(CooldownRoutine());
                CreateShieldVisual(_player.transform);
            }
        }
        private void CreateShieldVisual(Transform playerTransform)
        {
            // 여기에 실드가 터지는 파티클이나 효과음을 넣으면 좋습니다.
        }
        private IEnumerator CooldownRoutine()
        {
            _isCooldown = true;
            yield return new WaitForSeconds(_cooldown);
            _isCooldown = false;
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.DamageTaken.RemoveModifiersFromSource(this);
                _player.OnOverHeal -= HandleItemEffect;
                _player.OnPreDamage -= TriggerShield;
            }
        }
    }
}