using UnityEngine;
using Player;
using Core;

namespace Weapons
{
    public class WeaponBase : MonoBehaviour // [수정] abstract 제거
    {
        public WeaponData weaponData;
        public Transform firePoint;
        private SpriteRenderer weaponRenderer; // [추가] 무기 이미지를 보여줄 렌더러

        protected float nextFireTime;
        public bool isPlayerWeapon = true; //
        private PlayerStats _playerStats;

        private void Awake()
        {
            weaponRenderer = GetComponent<SpriteRenderer>();

            if (firePoint == null)
            {
                firePoint = transform.Find("FirePoint");
            }
            if (isPlayerWeapon)
            {
                _playerStats = GetComponentInParent<PlayerStats>();
                Debug.Log(_playerStats.baseAttackSpeed);
            }
        }
        // [추가] 외부(PlayerController 등)에서 무기 정보를 주입하는 함수
        public void Initialize(WeaponData newData)
        {
            weaponData = newData;
            if (weaponRenderer == null) weaponRenderer = GetComponent<SpriteRenderer>();

            if (newData != null && newData.weaponSprite != null)
            {
                weaponRenderer.sprite = newData.weaponSprite;
                weaponRenderer.color = Color.white; // 투명도 체크

                // 콘솔창에 이게 뜨는지 확인하세요!
                Debug.Log($"[비주얼 확인] 현재 무기: {newData.WeaponName}, 스프라이트 이름: {weaponRenderer.sprite.name}");
            }
        }

        public virtual void Fire(Vector2 direction)
        {
            if (weaponData == null || Time.time < nextFireTime) return;
            float finalFireRate;
            if (isPlayerWeapon && _playerStats != null)
            {
                // 플레이어 스탯이 반영된 연사 속도
                finalFireRate = _playerStats.GetCalculatedFireRate(weaponData.FireRate);
            }
            else
            {
                // 적이거나 스탯이 없으면 무기 기본 연사 속도 사용
                finalFireRate = weaponData.FireRate;
            }
            nextFireTime = Time.time + (1f / Mathf.Max(finalFireRate, 0.1f)); //

            // 데이터의 탄환 수만큼 발사 로직 수행
            for (int i = 0; i < weaponData.NumberOfPellets; i++)
            {
                SpawnProjectile(direction);
            }
        }

        protected virtual void SpawnProjectile(Vector2 direction)
        {
            if (weaponData == null || weaponData.projectilePrefab == null || firePoint == null) return;

            // 1. [핵심] 탄환 퍼짐(Spread) 계산
            // 설계도에 설정된 SpreadAngle이 10도라면, -5도 ~ +5도 사이의 랜덤한 각도를 생성합니다.
            float randomSpread = Random.Range(-weaponData.SpreadAngle * 0.5f, weaponData.SpreadAngle * 0.5f);

            // 2. 방향 벡터를 랜덤 각도만큼 회전시킵니다.
            Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);
            Vector2 finalDirection = spreadRotation * direction;

            // 3. 탄환 생성 (회전값에 spreadRotation을 곱해 시각적으로도 퍼지게 만듭니다)
            GameObject obj = Instantiate(weaponData.projectilePrefab, firePoint.position, firePoint.rotation * spreadRotation);
            obj.SetActive(true);

            if (obj.TryGetComponent(out Projectile proj))
            {
                float spd = Mathf.Max(weaponData.ProjectileSpeed, 5f);
                float mspd = spd / 10;
                int finalDamage;

                if (isPlayerWeapon && _playerStats != null)
                {
                    // PlayerStats의 GetCalculatedDamage 호출 (weaponData.Damage를 배수로 사용)
                    finalDamage = _playerStats.GetCalculatedDamage(weaponData.damageMultiplier);
                }
                else
                {
                    // 플레이어 무기가 아니거나 스탯을 못 찾았다면 기존 데이터 값 사용
                    finalDamage = Mathf.RoundToInt(weaponData.damageMultiplier * 10);
                }

                // 4. [중요] 계산된 finalDirection을 탄환에게 넘겨줍니다.
                proj.decelerationRate = weaponData.BulletDeceleration;
                proj.ownerStats = _playerStats;
                proj.Initialize(finalDirection, spd, mspd, finalDamage, weaponData.PierceCount, isPlayerWeapon, _playerStats);
            }
        }

        public void ResetFireDelay()
        {
            nextFireTime = Time.time;
        }
    }
}