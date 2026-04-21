using UnityEngine;
using Player;
using Drone;

namespace Item
{

    [CreateAssetMenu(fileName = "DroneAttack", menuName = "Items/Uncommon/Drone - Attack")]
    public class DroneAttack : PassiveItemData
    {
        [Header("Drone Settings")]
        public GameObject dronePrefab;     // 소환할 드론 프리팹
        public int spawnCount = 1;         // 한 번에 소환할 드론 수

        public override void OnApply(PlayerStats player)
        {
            // 1. 드론 매니저에게 능력 해제 알림
            if (DroneManager.Instance != null)
            {
                // ability.ToString()을 통해 "Defense", "Attack" 등의 문자열 전달
                DroneManager.Instance.UnlockAbility("Attack");
            }

            // 2. 설정된 수만큼 드론 소환
            for (int i = 0; i < spawnCount; i++)
            {
                var droneObj = Instantiate(dronePrefab);
                var droneBase = droneObj.GetComponent<DroneBase>();

                // 플레이어를 중심으로 공전하도록 설정 (초기 각도는 랜덤)
                droneBase.SetCenter(player.transform, Random.Range(0, 360f));

                // 매니저에 등록 (능력치 실시간 동기화를 위해)
                DroneManager.Instance.RegisterDrone(droneBase);
            }
        }
    }
}