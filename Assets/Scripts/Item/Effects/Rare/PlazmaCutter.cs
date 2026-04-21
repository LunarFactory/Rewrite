using UnityEngine;
using Player;
using Entity;
using Weapon;
using Core;

namespace Item
{

    [CreateAssetMenu(fileName = "PlazmaCutter", menuName = "Items/Rare/Plazma Cutter")]
    public class PlazmaCutterItem : PassiveItemData
    {
        public GameObject projectilePrefab; // 플라즈마 커터 탄환 프리팹
        public float damageMultiplier = 5f;
        public int ricochetCount = 10;
        public override void OnApply(PlayerStats player)
        {
            // 플레이어에 트래커를 부착 (중복 획득 시 여러 개가 붙지 않게 체크)
            var tracker = player.GetComponent<PlazmaCutterTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<PlazmaCutterTracker>();
                tracker.Initialize(player, projectilePrefab, damageMultiplier, ricochetCount);
            }
        }
    }
    public class PlazmaCutterTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private GameObject _projectilePrefab;
        private float _damageMultiplier;
        private int _ricochetCount;

        public void Initialize(PlayerStats player, GameObject prefab, float damageMultiplier, int ricochetCount)
        {
            _player = player;
            _projectilePrefab = prefab;
            _damageMultiplier = damageMultiplier;
            _ricochetCount = ricochetCount;

            _player.OnKill += HandleItemEffect;
            _player.OnWallHit += HandleHitTargetReset;
        }

        private void HandleItemEffect(EntityStats entity)
        {
            if (_projectilePrefab == null) return;

            // 1. 무작위 방향 결정
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) randomDir = Vector2.up;

            // 2. 탄환 생성 (플레이어 위치 혹은 필요하다면 적 위치에서 생성)
            GameObject obj = Instantiate(_projectilePrefab, entity.transform.position, Quaternion.identity);
            var proj = obj.GetComponent<Projectile>();

            if (proj != null)
            {
                // 3. 500% 데미지 계산 ($Damage \times 5.0$)
                // 현재 플레이어의 최종 공격력을 가져와서 배율을 곱합니다.
                int finalDamage = Mathf.RoundToInt(_player.DamageIncreased.GetValue(_player.AttackDamage.GetValue() * _damageMultiplier));

                // 4. 구조체를 이용한 데이터 준비
                ProjectileInfo info = new ProjectileInfo
                {
                    damage = finalDamage,
                    pierceCount = -1,       // 관통은 없고 튕기기만 함
                    ricochetCount = _ricochetCount,
                    homingRange = 0,
                    homingStrength = 0,
                    decelerationRate = 0,
                    scale = 1,
                    speed = 20f,           // 플라즈마니까 좀 빨라야겠죠?
                    minSpeed = 0,        // 감속을 막기 위해 MinSpeed를 Speed와 동일하게 설정
                };

                // 6. 초기화
                proj.Initialize(randomDir, info, _player, true);
            }
        }

        private void HandleHitTargetReset(Projectile proj)
        {
            proj.ResetOnWallHit();
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnKill -= HandleItemEffect;
        }
    }
}
