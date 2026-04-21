using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "SurgeProtector", menuName = "Items/Common/Surge Protector")]
    public class SurgeProtectorItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Shield Settings")]
        public float cooldown = 20f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<SurgeProtectorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<SurgeProtectorTracker>();
                tracker.Initialize(player, cooldown);
            }
        }
    }

    public class SurgeProtectorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, float cooldown)
        {
            _player = player;
            _cooldown = cooldown;

            _player.OnPreDamage += HandleItemEffect;
        }

        private void HandleItemEffect(ref int damage)
        {
            if (!_isCooldown)
            {
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
                _player.OnPreDamage -= HandleItemEffect;
            }
        }
    }
}