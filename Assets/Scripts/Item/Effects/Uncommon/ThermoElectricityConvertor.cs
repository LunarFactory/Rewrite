using UnityEngine;
using System.Collections;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "ThermoElectricityConvertor", menuName = "Items/Uncommon/Thermo Electricity Convertor")]
    public class ThermoElectricityConvertorItem : PassiveItemData
    {
        public float range = 6f;
        public float healPercent = 0.02f;

        // 이 효과가 이미 발동했는지 체크하는 변수
        public float cooldown = 30f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ThermoElectricityConvertorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ThermoElectricityConvertorTracker>();
                tracker.Initialize(player, range, healPercent, cooldown);
            }
        }
    }

    public class ThermoElectricityConvertorTracker : MonoBehaviour
    {
        private PlayerStealth _playerStealth;
        private PlayerStats _player;
        private float _range;
        private float _healPercent;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, float range, float healPercent, float cooldown)
        {
            _player = player;
            _playerStealth = _player.GetComponent<PlayerStealth>();
            _range = range;
            _healPercent = healPercent;
            _cooldown = cooldown;

            _playerStealth.OnStealthStart += HandleItemEffect;
        }

        private void HandleItemEffect()
        {
            if (!_isCooldown)
            {
                AbsorbBullet(_player.transform.position);
                StartCoroutine(CooldownRoutine());
            }
        }

        private void AbsorbBullet(Vector2 center)
        {
            int mask = LayerMask.GetMask("EnemyProjectile");
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, _range, mask);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("EnemyProjectile"))
                {
                    Destroy(hit.gameObject);
                    _player.Heal(Mathf.RoundToInt(_player.maxHealth * _healPercent));
                }
            }

            // 시각적 피드백 (예: 쾅! 하는 파티클)을 여기서 생성하면 좋습니다.
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
                _playerStealth.OnStealthStart -= HandleItemEffect;
            }
        }
    }
}