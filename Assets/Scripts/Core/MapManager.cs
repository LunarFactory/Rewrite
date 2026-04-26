using System.Collections.Generic;
using System.Linq;
using Level;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        private void Start() { }

        public void LoadMapList(List<MapData> maps)
        {
            totalMapPool = maps;
        }

        public void LoadMap(int currentFloor)
        {
            Random.State originalState = Random.state;
            int mapSeed = (
                RunManager.Instance.CurrentSeed.ToString() + "_" + currentFloor + "_MapSelection"
            ).GetHashCode();
            Random.InitState(mapSeed);
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
            Tilemap floormap = _currentMapInstance
                .GetComponentsInChildren<Tilemap>()
                .FirstOrDefault(t => t.name == "FloorMap");
            if (floormap != null && AstarPath.active != null)
            {
                var gg = AstarPath.active.data.gridGraph;

                // Floormap에 깔린 타일들의 실제 영역(Bounds)을 가져옵니다.
                // cellBounds는 타일이 있는 칸수만큼의 크기를 알려줍니다.
                var bounds = floormap.cellBounds;

                // 3. 에이스타 격자의 위치와 크기를 맵에 딱 맞게 조절
                // 중심점: 타일맵의 중심 좌표
                gg.center = floormap.transform.TransformPoint(bounds.center);

                // 크기: 타일맵의 가로/세로 칸수 (여유분 +2)
                int width = bounds.size.x + 2;
                int depth = bounds.size.y + 2;

                // 격자 크기 업데이트 (가로, 세로, 한 칸의 크기)
                gg.SetDimensions(width, depth, gg.nodeSize);

                // 4. 이 영역을 기준으로 다시 스캔!
                AstarPath.active.Scan();

                Debug.Log(
                    $"<color=green>[MapManager]</color> Floormap 크기({width}x{depth})에 맞춰 길찾기 영역 갱신!"
                );
            }
            else
            {
                Debug.LogError("맵 프리팹에서 'Floormap'을 찾을 수 없거나 Pathfinder가 없습니다!");
            }

            // 5. 현재 맵의 스폰 영역 참조 (맵 프리팹 내부에 SpawnZone이 있다고 가정)
            CurrentSpawnZone = _currentMapInstance.GetComponentInChildren<MapSpawnZone>();

            // 6. 플레이어 객체를 찾아 위치 이동
            if (Player.PlayerStats.LocalPlayer != null)
            {
                Player.PlayerStats.LocalPlayer.transform.position = selectedMap.playerSpawnPoint;
            }
            WaveManager.Instance.bossSpawnPoint = selectedMap.bossSpawnPoint;
            WaveManager.Instance.rewardSpawnPoint = selectedMap.rewardSpawnPoint;

            Debug.Log($"<color=yellow>[MapManager]</color> {selectedMap.mapName} 로드 완료");
        }
    }
}
