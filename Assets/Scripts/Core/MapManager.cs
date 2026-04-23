using System.Collections.Generic;
using System.Linq;
using Level;
using UnityEngine;

namespace Core
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [SerializeField]
        private List<MapData> totalMapPool; // 전체 맵 데이터 리스트

        private GameObject _currentMapInstance;
        public MapSpawnZone CurrentSpawnZone { get; private set; } // 현재 맵의 스폰 영역

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
                Instance = this;
                return;
            }
            Instance = this;
        }

        public void LoadMapList(List<MapData> maps)
        {
            totalMapPool = maps;
        }

        public void LoadMap(int currentFloor)
        {
            // 1. 기존 맵 제거
            if (_currentMapInstance != null)
                Destroy(_currentMapInstance);

            // 2. 조건에 맞는 맵 후보군 필터링 (층수 & 타입)
            List<MapData> candidates = totalMapPool
                .Where(m => currentFloor >= m.minFloor && currentFloor <= m.maxFloor)
                .ToList();

            // 3. 가중치 기반 무작위 선택 (가장 간단하게는 Random.Range)
            MapData selectedMap = candidates[Random.Range(0, candidates.Count)];

            // 4. 맵 생성
            _currentMapInstance = Instantiate(
                selectedMap.mapPrefab,
                Vector3.zero,
                Quaternion.identity
            );

            // 5. 현재 맵의 스폰 영역 참조 (맵 프리팹 내부에 SpawnZone이 있다고 가정)
            CurrentSpawnZone = _currentMapInstance.GetComponentInChildren<MapSpawnZone>();

            // 6. 플레이어 객체를 찾아 위치 이동
            if (Player.PlayerStats.LocalPlayer != null)
            {
                Player.PlayerStats.LocalPlayer.transform.position = selectedMap.playerSpawnPoint;
            }

            Debug.Log($"<color=yellow>[MapManager]</color> {selectedMap.mapName} 로드 완료");
        }
    }
}
