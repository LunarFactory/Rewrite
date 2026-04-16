using UnityEngine;

namespace Core
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public enum WaveType { Mob, Shop, Rest, Boss }
        [field: SerializeField] public int CurrentWave { get; private set; } = 1;

        // [추가] WaveManager가 직접 관리할 적 프리팹
        [Header("Wave Resources")]
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject mobPrefab;
        [SerializeField] private GameObject bossPrefab; // 보스전 대비용 추가
        [Header("Special Prefabs")]
        [SerializeField] private GameObject exitPortalPrefab; // 다음 웨이브로 가는 포탈
        [SerializeField] private GameObject healthRestorerPrefab; // 체력 회복 오브젝트
        private int activeEnemyCount = 0;

        private void Start()
        {
            if (RunManager.Instance != null)
            {
                Debug.Log($"현재 런의 무기: {RunManager.Instance.GetWeapon().WeaponName}");
                // ApplyRunSettings(runData);
            }
            StartWave(CurrentWave);
        }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartFloor(int floorNumber)
        {
            CurrentWave = 1;
            StartWave(CurrentWave);
        }

        public void StartWave(int waveNumber)
        {
            CurrentWave = waveNumber;
            WaveType type = GetWaveType(CurrentWave);

            Debug.Log($"[WaveManager] Wave {CurrentWave} Started - Type: {type}");

            Log.PlayerLogManager.Instance?.OnWaveStarted(CurrentWave);

            switch (type)
            {
                case WaveType.Mob:
                    SpawnEnemies(mobPrefab, 3 + RunManager.Instance.CurrentFloor);
                    break;
                case WaveType.Shop:
                    SpawnShop();
                    break;
                case WaveType.Rest:
                    SpawnRest();
                    break;
                case WaveType.Boss:
                    SpawnEnemies(bossPrefab, 1);
                    break;
            }
        }

        // [개선] 소환 로직을 별도 메서드로 분리하여 중복 제거 및 의존성 고립
        private void SpawnEnemies(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[WaveManager] {prefab?.name} 프리팹이 할당되지 않았습니다!");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector2 randomPos = UnityEngine.Random.insideUnitCircle * 8f;
                GameObject enemy = Instantiate(prefab, randomPos, Quaternion.identity);
                enemy.SetActive(true);
            }
        }
        public void OnEnemyDied()
        {
            activeEnemyCount--;
            if (activeEnemyCount <= 0)
            {
                // 모든 적 처치 시 다음 웨이브 포탈 소환 또는 즉시 완료
                SpawnExitPortal();
            }
        }
        private void SpawnShop()
        {
            // 3개의 랜덤 아이템 생성 (가격 책정)
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = new Vector2(-2f + (i * 2f), 0);
                GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity); // 실제론 아이템 프리팹
                var fieldItem = itemObj.GetComponent<Level.FieldItem>();
                fieldItem.price = 50 * RunManager.Instance.CurrentFloor; // 층별 가격 상승
            }
            SpawnExitPortal();
        }
        private void SpawnRest()
        {
            Instantiate(healthRestorerPrefab, Vector3.zero, Quaternion.identity);
            SpawnExitPortal();
        }

        private void SpawnExitPortal()
        {
            // 플레이어 근처나 맵 중앙에 포탈 생성
            Instantiate(exitPortalPrefab, new Vector2(0, 3f), Quaternion.identity);
        }
        private void TestCompleteWave()
        {
            CompleteCurrentWave();
        }

        public void CompleteCurrentWave()
        {
            Debug.Log($"[WaveManager] Wave {CurrentWave} Completed.");

            Log.PlayerLogManager.Instance?.OnWaveCompleted(CurrentWave);

            if (CurrentWave < 9)
            {
                StartWave(CurrentWave + 1);
            }
            else
            {
                RunManager.Instance.AdvanceFloor();
            }
        }

        private WaveType GetWaveType(int wave)
        {
            if (wave == 4) return WaveType.Shop;
            if (wave == 8) return WaveType.Rest;
            if (wave == 9) return WaveType.Boss;
            return WaveType.Mob;
        }
    }
}