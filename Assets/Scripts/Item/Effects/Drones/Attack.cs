using UnityEngine;
using Player;
using Entity;
using Weapon;

namespace Drone
{
    public class Attack : MonoBehaviour
    {
        [Header("Settings")]
        public float range = 10f;
        public float ProjectileSpeed = 10f;
        private LayerMask enemyLayer;

        private float _timer;
        private Transform _currentTarget;

        private void Awake()
        {
            // 레이저와 마찬가지로 레이어 자동 설정 (안전장치)
            enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
            Debug.Log($"{gameObject.name} 사격 모듈 장착!");
        }

        private void Update()
        {
            if (PlayerStats.LocalPlayer == null) return;

            // 1. 가장 가까운 타겟 찾기
            FindClosestTarget();

            // 2. 공격 주기 계산 (플레이어 공속 반영)
            float fireRate = PlayerStats.LocalPlayer.AttackSpeed.GetValue();
            if (fireRate <= 0) fireRate = 1f;

            _timer += Time.deltaTime;
            if (_timer >= 1f / fireRate)
            {
                _timer = 0f;
                if (_currentTarget != null)
                {
                    Shoot();
                }
            }
        }

        private void FindClosestTarget()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
            
            Transform closest = null;
            float minDistanceSqr = float.MaxValue;

            foreach (var enemyCollider in enemies)
            {
                var stats = enemyCollider.GetComponent<EntityStats>();
                if (stats != null && stats.isDead) continue;

                float distSqr = (enemyCollider.transform.position - transform.position).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closest = enemyCollider.transform;
                }
            }
            _currentTarget = closest;
        }

        private void Shoot()
        {
            if (DroneManager.Instance.projectilePrefab == null) return;

            // 발사 방향 계산
            Vector2 direction = (_currentTarget.position - transform.position).normalized;

            // 탄환 생성
            GameObject projObj = Instantiate(DroneManager.Instance.projectilePrefab, transform.position, Quaternion.identity);
            Projectile proj = projObj.GetComponent<Projectile>();

            if (proj != null)
            {
                // Projectile 초기화 (관통 0, PlayerStats 전달)
                proj.Initialize(direction, new ProjectileInfo{
                    damage = Mathf.RoundToInt(PlayerStats.LocalPlayer.DamageIncreased.GetValue(PlayerStats.LocalPlayer.AttackDamage.GetValue() * 0.5f * DroneManager.Instance.globalDroneDamageMultiplier)), 
                    pierceCount = 0, 
                    ricochetCount = 0,
                    homingRange = 0, 
                    homingStrength = 0,
                    decelerationRate = 0,
                    scale = 1,
                    speed = ProjectileSpeed, 
                    minSpeed = ProjectileSpeed / 10, 
                }, PlayerStats.LocalPlayer);
                
            }
        }

        // 사거리 확인용 기즈모
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}