using Drone;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "MutualEmpathyCore", menuName = "Items/Boss/Mutual Empathy Core")]
    public class MutualEmpathyCore : PassiveItemData
    {
        public float damageMultiplier = 2f;
        public int bonusDrones = 2;
        public GameObject baseDronePrefab;

        public override void OnApply(PlayerStats player)
        {
            // 1. 글로벌 데미지 배율 증가
            DroneManager.Instance.globalDroneDamageMultiplier *= damageMultiplier;

            // 2. 보너스 드론 소환
            for (int i = 0; i < bonusDrones; i++)
            {
                var drone = Instantiate(baseDronePrefab).GetComponent<DroneBase>();
                drone.SetCenter(player.transform, Random.Range(0, 360f));
                DroneManager.Instance.RegisterDrone(drone);
            }
        }
    }
}
