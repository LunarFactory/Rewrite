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
        [SerializeField] private GameObject mobPrefab;
        [SerializeField] private GameObject bossPrefab; // 보스전 대비용 추가

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
                    Invoke(nameof(TestCompleteWave), 10f); // 테스트용 10초 후 완료
                    break;
                case WaveType.Shop:
                case WaveType.Rest:
                    Invoke(nameof(TestCompleteWave), 1f);
                    break;
                case WaveType.Boss:
                    SpawnEnemies(bossPrefab, 1);
                    Invoke(nameof(TestCompleteWave), 5f);
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
                // [수정] TestSetup.EnemyPrefab이 아닌 로컬 변수 사용
                GameObject enemy = Instantiate(prefab, randomPos, Quaternion.identity);
                enemy.SetActive(true);
            }
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