using Core;
using Entity;
using UnityEngine;

namespace Weapon
{
    public class WeaponBase : MonoBehaviour // [수정] abstract 제거
    {
        public WeaponData weaponData;
        public Transform firePoint;
        private SpriteRenderer weaponRenderer; // [추가] 무기 이미지를 보여줄 렌더러

        protected float nextFireTime;
        private EntityStats _equippedEntityStats;
        private Player.PlayerStats _playerStats;
        public float finalFireRate;

        private void Awake()
        {
            weaponRenderer = GetComponent<SpriteRenderer>();

            if (firePoint == null)
            {
                firePoint = transform.Find("FirePoint");
            }
            _equippedEntityStats = GetComponentInParent<EntityStats>();
            if (_equippedEntityStats is Player.PlayerStats player)
            {
                _playerStats = player;
            }
        }

        // [추가] 외부(PlayerController 등)에서 무기 정보를 주입하는 함수
        public void Initialize(WeaponData newData)
        {
            weaponData = newData;
            if (weaponRenderer == null)
                weaponRenderer = GetComponent<SpriteRenderer>();

            if (newData != null && newData.weaponSprite != null)
            {
                weaponRenderer.sprite = newData.weaponSprite;
                weaponRenderer.color = Color.white; // 투명도 체크
            }
        }

        public virtual void Fire(Vector2 direction)
        {
            if (weaponData == null || Time.time < nextFireTime)
                return;
            // 플레이어 스탯이 반영된 연사 속도
            Log.LogTracker.Instance.RegisterAttackClick();
            finalFireRate = _playerStats.GetWeaponBaseAttackSpeed();
            nextFireTime = Time.time + (1f / Mathf.Max(finalFireRate, 0.1f)); //

            // 데이터의 탄환 수만큼 발사 로직 수행
            for (int i = 0; i < weaponData.numberOfPellets; i++)
            {
                SpawnProjectile(direction);
            }
        }

        protected virtual void SpawnProjectile(Vector2 direction)
        {
            if (weaponData == null || weaponData.projectilePrefab == null || firePoint == null)
                return;

            // 1. [핵심] 탄환 퍼짐(Spread) 계산
            // 설계도에 설정된 SpreadAngle이 10도라면, -5도 ~ +5도 사이의 랜덤한 각도를 생성합니다.
            float randomSpread = Random.Range(
                -weaponData.spreadAngle * 0.5f,
                weaponData.spreadAngle * 0.5f
            );

            // 2. 방향 벡터를 랜덤 각도만큼 회전시킵니다.
            Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);
            Vector2 finalDirection = spreadRotation * direction;

            // 3. 탄환 생성 (회전값에 spreadRotation을 곱해 시각적으로도 퍼지게 만듭니다)
            GameObject obj = Instantiate(
                weaponData.projectilePrefab,
                firePoint.position,
                firePoint.rotation * spreadRotation
            );
            obj.SetActive(true);

            if (obj.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(
                    finalDirection,
                    new ProjectileInfo
                    {
                        damage = Mathf.RoundToInt(
                            _playerStats.DamageIncreased.GetValue(
                                _playerStats.GetWeaponBaseAttackDamage()
                            )
                        ),
                        pierceCount = weaponData.pierceCount + (int)_playerStats.Pierce.GetValue(),
                        ricochetCount = (int)_playerStats.Ricochet.GetValue(),
                        homingRange = (int)_playerStats.HomingRange.GetValue(),
                        homingStrength = (int)_playerStats.HomingStrength.GetValue(),
                        decelerationRate =
                            weaponData.decelerationRate * _playerStats.DecelerationRate.GetValue(),
                        scale = _playerStats.ProjectileScale.GetValue(),
                        speed =
                            weaponData.projectileSpeed * _playerStats.ProjectileSpeed.GetValue(),
                        minSpeed =
                            (weaponData.projectileSpeed * _playerStats.ProjectileSpeed.GetValue())
                            / 10,
                    },
                    _playerStats
                );
            }
        }

        public void ResetFireDelay()
        {
            nextFireTime = Time.time;
        }
    }
}
