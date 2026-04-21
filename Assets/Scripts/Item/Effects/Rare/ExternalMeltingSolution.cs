using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "ExternalMeltingSolution", menuName = "Items/Rare/External Melting Solution")]
    public class ExternalMeltingSolutionItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Melted Settings")]
        [SerializeField] private MeltedEffect buffData;
        public float moveSpeed = -0.2f;
        public float damageTaken = 0.01f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ExternalMeltingSolutionTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ExternalMeltingSolutionTracker>();
                tracker.Initialize(player, buffData, moveSpeed, damageTaken);
            }
        }
    }

    public class ExternalMeltingSolutionTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private MeltedEffect _buffData;

        public void Initialize(PlayerStats player, MeltedEffect buffData, float moveSpeed, float damageTaken)
        {
            _player = player;
            _buffData = buffData;
            _buffData.moveSpeed = moveSpeed;
            _buffData.damageTaken = damageTaken;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (target.TryGetComponent(out BuffManager buffManager))
            {
                if (_buffData != null)
                {
                    _buffData.setCurrentStack(_buffData.getCurrentStack() + 1);
                    buffManager.ApplyEffect(_buffData, 3, attacker);
                }
            }
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}