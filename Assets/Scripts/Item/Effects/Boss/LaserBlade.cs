using UnityEngine;
using System.Collections;
using Entity;
using Player;
using Enemy;
using System;
using Unity.VisualScripting;

namespace Item
{
    [CreateAssetMenu(fileName = "LaserBlade", menuName = "Items/Boss/Laser Blade")]
    public class LaserBladeItem : PassiveItemData
    {
        public float damageMultiplier = 8f;
        public float laserbladeRadius = 3.5f;
        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LaserBladeTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LaserBladeTracker>();
                tracker.Initialize(player, damageMultiplier, laserbladeRadius);
            }
        }
    }

    public class LaserBladeTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private PlayerStealth _playerStealth;
        private float _damageMultiplier;
        private float _laserbladeRadius;

        public void Initialize(PlayerStats player, float damageMultiplier, float laserbladeRadius)
        {
            _player = player;
            _playerStealth = player.GetComponent<PlayerStealth>();
            _damageMultiplier = damageMultiplier;
            _laserbladeRadius = laserbladeRadius;

            _playerStealth.OnStealthEnd += HandleItemEffect;
        }

        private void HandleItemEffect()
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(_player.transform.position, _laserbladeRadius, LayerMask.GetMask("Enemy"));

            // 데미지 계산: 현재 공격력 * 30 (3000%)
            int finalDamage = Mathf.RoundToInt(_player.DamageIncreased.GetValue(_player.AttackDamage.GetValue() * _damageMultiplier));

            foreach (var col in hitEnemies)
            {
                if (col.TryGetComponent<EnemyStats>(out var enemy))
                {
                    // 3. 범위 내 적들에게 피해 입힘 (Color.gold로 크리티컬 느낌 강조)
                    enemy.TakeDamage(_player, finalDamage, Color.softRed);
                    // (선택 사항) 충격파 연출이나 파티클을 여기서 생성하면 좋습니다.
                }
            }

            Debug.Log($"[광선검] 플레이어 주변 {_laserbladeRadius} 범위에 {finalDamage}의 피해!");
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _playerStealth.OnStealthEnd -= HandleItemEffect;
            }
        }
    }
}