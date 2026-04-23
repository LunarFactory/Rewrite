using UnityEngine;

namespace Level
{
    public class MapSpawnZone : MonoBehaviour
    {
        private Collider2D _spawnArea;

        [SerializeField]
        private LayerMask obstacleLayer; // 장애물 레이어 (벽, 기둥 등)

        [SerializeField]
        private float minPlayerDistance = 7f; // 플레이어와 떨어질 최소 거리

        [SerializeField]
        private float checkRadius = 0.5f; // 소환 지점 장애물 체크 반경

        private void Awake()
        {
            _spawnArea = GetComponent<Collider2D>(); // 트리거가 아니더라도 로직은 작동하지만, 물리 충돌 방지를 위해 이 콜라이더는 보통 IsTrigger가 좋습니다.
            _spawnArea.isTrigger = true;
        }

        // 영역 내에서 유효한 랜덤 위치를 찾아 반환
        public Vector2 GetRandomLocation()
        {
            // 1. 플레이어 위치 확인
            Transform playerTrm = null;
            if (Player.PlayerStats.LocalPlayer != null)
            {
                playerTrm = Player.PlayerStats.LocalPlayer.transform;
            }
            else
            {
                // LocalPlayer 캐싱이 안 되어 있을 경우를 대비한 폴백
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    playerTrm = playerObj.transform;
            }

            Bounds bounds = _spawnArea.bounds;
            Vector2 randomPos = Vector2.zero;
            bool isValid = false;
            int safetyNet = 0; // 무한 루프 방지용

            while (!isValid && safetyNet < 50)
            {
                safetyNet++;

                // 2. 바운드 박스 내 무작위 점 생성
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float y = Random.Range(bounds.min.y, bounds.max.y);
                randomPos = new Vector2(x, y);

                // 3. 조건 검사 시작

                // A. 해당 점이 실제 콜라이더 영역 내부에 있는가? (다각형/원형 콜라이더 대응)
                if (!_spawnArea.OverlapPoint(randomPos))
                    continue;

                // B. 해당 위치에 장애물(벽 등)이 있는가?
                if (Physics2D.OverlapCircle(randomPos, checkRadius, obstacleLayer) != null)
                    continue;

                // C. 플레이어와 충분히 떨어져 있는가?
                if (playerTrm != null)
                {
                    float dist = Vector2.Distance(randomPos, playerTrm.position);
                    if (dist < minPlayerDistance)
                        continue;
                }

                // 모든 검사를 통과하면 유효!
                isValid = true;
            }

            // 만약 50번 시도해도 자리를 못 찾았다면, 그나마 마지막에 뽑힌 좌표라도 반환 (혹은 에러 방지)
            return randomPos;
        }
    }
}
