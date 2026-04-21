using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "RecurrentNeuralNetwork", menuName = "Items/Uncommon/Recurrent Neural Network")]
    public class RecurrentNeuralNetworkItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Ricochet Settings")]
        public int ricochetCount = 2;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<RecurrentNeuralNetworkTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<RecurrentNeuralNetworkTracker>();
                tracker.Initialize(player, ricochetCount);
            }
        }
    }

    public class RecurrentNeuralNetworkTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private int _ricochetCount;
        private StatModifier _mod;
        public void Initialize(PlayerStats player, int ricochetCount)
        {
            _player = player;
            _ricochetCount = ricochetCount;

            _mod = new StatModifier("RecurrentNeuralNetworkRicochet", ricochetCount, ModifierType.Flat, this);
            _player.Ricochet.AddModifier(_mod);
        }


        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.Ricochet.RemoveModifiersFromSource(this);
            }
        }
    }
}