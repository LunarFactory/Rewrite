using System.Collections;
using Enemy;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "OpticalCamouflage", menuName = "Items/Boss/Optical Camouflage")]
    public class OpticalCamouflageItem : PassiveItemData
    {
        public float damageMultiplier = 5f;
        public float moveSpeed = 0.3f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<OpticalCamouflageTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<OpticalCamouflageTracker>();
                tracker.Initialize(player, damageMultiplier, moveSpeed);
            }
        }
    }

    public class OpticalCamouflageTracker : MonoBehaviour
    {
        private PlayerStealth _playerStealth;
        private PlayerStats _player;
        private StatModifier _moveSpeedMod;
        private float _damageMultiplier;
        private float _moveSpeed;
        private bool _isNextAttackEmpowered = false;

        public void Initialize(PlayerStats player, float damageMultiplier, float moveSpeed)
        {
            _player = player;
            _playerStealth = _player.GetComponent<PlayerStealth>();
            _damageMultiplier = damageMultiplier;
            _moveSpeed = moveSpeed;

            _playerStealth.OnStealthStart += HandleItemEffect;
            _playerStealth.OnStealthEnd += HandleItemEffectEnd;
            _player.OnPlayerAttackHit += HandleAttackHit;
        }

        private void HandleItemEffect()
        {
            _moveSpeedMod = new StatModifier(
                "OpticalCamouflageMoveSpeed",
                _moveSpeed,
                ModifierType.Percent,
                this
            );
            _player.MoveSpeed.AddModifier(_moveSpeedMod);
            _isNextAttackEmpowered = true;
        }

        private void HandleItemEffectEnd()
        {
            _player.MoveSpeed.RemoveModifiersFromSource(this);
        }

        private void HandleAttackHit(EntityStats attacker, EntityStats target, int damage)
        {
            if (_isNextAttackEmpowered)
            {
                if (attacker is PlayerStats player && target is EnemyStats enemy)
                {
                    // 1. 추가 피해 입히기 (이미 들어간 데미지 외에 추가로 가함)
                    int extraDamage = Mathf.RoundToInt(
                        player.DamageIncreased.GetValue(
                            player.GetWeaponBaseAttackDamage() * _damageMultiplier
                        )
                    );
                    enemy.TakeDamage(player, extraDamage, Color.gold);

                    // 2. 효과 소모
                    _isNextAttackEmpowered = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _playerStealth.OnStealthStart -= HandleItemEffect;
                _playerStealth.OnStealthEnd -= HandleItemEffectEnd;
            }
        }
    }
}
