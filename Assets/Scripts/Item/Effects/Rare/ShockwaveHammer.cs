using UnityEngine;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "ShockwaveHammer", menuName = "Items/Rare/Shockwave Hammer")]
    public class ShockwaveHammerItem : PassiveItemData
    {
        [Header("Shockwave Settings")]
        public ShockedEffect data;
        public float damageMultiplier = 30f; // 3000%
        public float shockwaveRadius = 3.5f; // 충격파 범위
        public float damageTaken = 0.5f; // 받는 피해 50% 증가 (예시)
        public float duration = 5f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ShockwaveHammerTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ShockwaveHammerTracker>();
                tracker.Initialize(player, data, damageMultiplier, shockwaveRadius, damageTaken, duration);
            }
        }
    }

    public class ShockwaveHammerTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private ShockedEffect _data;
        private float _damageMultiplier; // 3000%
        private float _shockwaveRadius; // 충격파 범위
        private float _damageTaken; // 받는 피해 50% 증가 (예시)
        private float _duration;

        public void Initialize(PlayerStats player, ShockedEffect data, float damageMultiplier, float shockwaveRadius, float damageTaken, float duration)
        {
            _player = player;
            _data = data;
            _damageMultiplier = damageMultiplier;
            _shockwaveRadius = shockwaveRadius;
            _damageTaken = damageTaken;
            _duration = duration;

            _player.OnPlayerApplyHardCC += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(target.transform.position, _shockwaveRadius, LayerMask.GetMask("Enemy"));

            // 데미지 계산: 현재 공격력 * 30 (3000%)
            int finalDamage = Mathf.RoundToInt(attacker.DamageIncreased.GetValue(attacker.AttackDamage.GetValue() * _damageMultiplier));

            foreach (var col in hitEnemies)
            {
                if (col.TryGetComponent<EnemyStats>(out var enemy))
                {
                    // 3. 범위 내 적들에게 피해 입힘 (Color.gold로 크리티컬 느낌 강조)
                    enemy.TakeDamage(attacker, finalDamage, Color.gold);
                    if (enemy.TryGetComponent<BuffManager>(out var buff))
                    {
                        buff.ApplyEffect(_data, _duration, attacker);
                    }
                    // (선택 사항) 충격파 연출이나 파티클을 여기서 생성하면 좋습니다.
                }
            }

            Debug.Log($"[충격파 해머] {target.name} 주변 {_shockwaveRadius} 범위에 {finalDamage}의 피해!");
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnPlayerApplyHardCC -= HandleItemEffect;
        }

        // 에디터에서 범위를 시각적으로 확인하기 위함
        private void OnDrawGizmosSelected()
        {
            if (_data != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, _shockwaveRadius);
            }
        }
    }
}