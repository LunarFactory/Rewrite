using UnityEngine;
using System.Collections.Generic;

namespace Drone
{
    public class DroneManager : MonoBehaviour
    {
        public static DroneManager Instance { get; private set; }
        [Header("Global Orbit")]
        public float orbitSpeed = 60f;
        public float orbitDistance = 2.5f;
        private float _masterRotation; // 모든 드론이 공유하는 마스터 회전 값        public GameObject projectilePrefab; // DroneAttack이 쏠 탄환 프리팹
        public float globalDroneDamageMultiplier = 1f;

        public bool hasDefense, hasAttack, hasLaser;
        private List<DroneBase> _activeDrones = new List<DroneBase>();

        public GameObject projectilePrefab;

        private void Awake() => Instance = this;
        private void Update()
        {
            // 1. 매니저가 마스터 각도를 하나만 계산합니다.
            _masterRotation += orbitSpeed * Time.deltaTime;
        }
        public float GetMasterRotation() => _masterRotation; public void RegisterDrone(DroneBase drone)
        {
            if (!_activeDrones.Contains(drone))
            {
                _activeDrones.Add(drone);
                UpdateDroneIndices(); // 드론이 들어오면 번호표 다시 배분
                RefreshAbilities(drone);
            }
        }

        public void UnregisterDrone(DroneBase drone)
        {
            if (_activeDrones.Contains(drone))
            {
                _activeDrones.Remove(drone);
                UpdateDroneIndices(); // 드론이 나가면 번호표 다시 배분
                RefreshAbilities(drone);
            }
        }

        private void UpdateDroneIndices()
        {
            int total = _activeDrones.Count;
            for (int i = 0; i < total; i++)
            {
                // 각 드론에게 "너는 몇 번째 자리에 서라"고 알려줌
                // i = 0, 1, 2... / total = 3 이면 각도는 0, 120, 240이 됨
                float targetAngle = i * (360f / total);
                _activeDrones[i].SetTargetOffset(targetAngle);
            }
        }
        public void UnlockAbility(string type)
        {
            if (type == "Defense") hasDefense = true;
            if (type == "Attack") hasAttack = true;
            if (type == "Laser") hasLaser = true;

            foreach (var d in _activeDrones) RefreshAbilities(d);
        }

        private void RefreshAbilities(DroneBase drone)
        {
            if (hasDefense) drone.AddAbility<Defense>();
            if (hasAttack) drone.AddAbility<Attack>();
            if (hasLaser) drone.AddAbility<Laser>();
        }
    }
}