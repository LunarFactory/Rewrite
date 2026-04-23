using UnityEngine;

namespace Level
{
    [CreateAssetMenu(fileName = "NewMapData", menuName = "Map/MapData")]
    public class MapData : ScriptableObject
    {
        [Header("Basic Info")]
        public string mapName;
        public GameObject mapPrefab;

        [Header("Appearance Conditions")]
        public int minFloor = 1; // 등장 시작 층
        public int maxFloor = 5; // 등장 종료 층 (필요 시)
        public int weight = 10; // 등장 확률 (가중치)

        [Header("Environment")]
        public Color environmentColor = Color.white; // 층별 분위기를 위한 색상

        [Header("Player Settings")]
        [Tooltip("이 맵에서 플레이어가 시작할 위치")]
        public Vector2 playerSpawnPoint; // [추가] 플레이어 스폰 위치
    }
}
