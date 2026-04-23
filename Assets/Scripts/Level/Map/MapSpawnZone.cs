using UnityEngine;

namespace Level
{
    public class MapSpawnZone : MonoBehaviour
    {
        private Collider2D _spawnArea;

        [SerializeField]
        private LayerMask obstacleLayer; // 장애물 레이어 (벽, 기둥 등)

        private void Awake()
        {
            _spawnArea = GetComponent<Collider2D>();
        }

        // 영역 내에서 유효한 랜덤 위치를 찾아 반환
        public Vector2 GetRandomLocation()
        {
            Bounds bounds = _spawnArea.bounds;
            Vector2 randomPos = Vector2.zero;
            bool isValid = false;
            int safetyNet = 0;

            while (!isValid && safetyNet < 50)
            {
                safetyNet++;

                // 1. 바운드 내에서 무작위 점 생성
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float y = Random.Range(bounds.min.y, bounds.max.y);
                randomPos = new Vector2(x, y);

                // 2. 점이 실제 콜라이더(영역) 안에 있는지 확인 (다각형일 경우 대비)
                if (_spawnArea.OverlapPoint(randomPos))
                {
                    // 3. 해당 위치에 장애물 레이어가 없는지 확인
                    if (Physics2D.OverlapCircle(randomPos, 0.5f, obstacleLayer) == null)
                    {
                        isValid = true;
                    }
                }
            }

            return randomPos;
        }
    }
}
