using UnityEngine;
using System.Collections.Generic;
using Item;
using Enemy;
using System;
using Entity;

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
        public int activeEnemyCount = 0;

        public static event Action OnBossWaveStart;
        public static event Action<EntityStats> OnBossSummon;

        private void Start()
        {
            // 씬이 시작되자마자 실행됩니다.
            if (RunManager.Instance != null)
            {
                Debug.Log("[WaveManager] RunManager를 발견했습니다. 층 데이터 동기화를 시작합니다.");
                // 현재 RunManager에 저장된 층 번호로 웨이브를 시작합니다.
                StartFloor(RunManager.Instance.CurrentFloor);
            }
            else
            {
                Debug.LogWarning("[WaveManager] RunManager를 찾을 수 없습니다. 테스트용 모드로 작동하거나 대기합니다.");
            }
            GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");

            // 2. 플레이어 객체를 찾아 위치 이동
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && spawnPoint != null)
            {
                player.transform.position = spawnPoint.transform.position;
            }
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
                    NotifyBossWaveStart();
                    SpawnEnemies(bossPrefab, 1, true);
                    break;
            }
        }

        // [개선] 소환 로직을 별도 메서드로 분리하여 중복 제거 및 의존성 고립
        private void SpawnEnemies(GameObject prefab, int count, bool isBoss = false)
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
                EnemyStats stat = enemy.GetComponent<EnemyStats>();
                enemy.SetActive(true);
                stat.isBoss = isBoss;
                if (isBoss) NotifyBossSummon(stat);
                activeEnemyCount++;
            }
        }
        public void OnEnemyDied()
        {
            activeEnemyCount--;
            if (activeEnemyCount <= 0)
            {
                if (GetWaveType(CurrentWave) == WaveType.Boss)
                {
                    if (RunManager.Instance.CurrentFloor == 5)
                    {
                        RunManager.Instance.AdvanceFloor();
                    }
                    else
                    {
                        SpawnBossRewards();
                        SpawnExitPortal(); // 보상을 다 보고 나갈 수 있게 포탈 소환
                    }
                }
                else CompleteCurrentWave();
                // 모든 적 처치 시 다음 웨이브 포탈 소환 또는 즉시 완료
            }
        }
        private void SpawnShop()
        {
            // 1. 중복 없는 아이템 세트 3개를 한 번에 가져옴
            List<PassiveItemData> itemsToSpawn = RunManager.Instance.GetRandomItemSet(CurrentWave, 3);

            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                Vector2 pos = new Vector2(-2f + (i * 2f), 0);
                GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity);

                if (itemObj.TryGetComponent(out Level.FieldItem fieldItem))
                {
                    fieldItem.itemData = itemsToSpawn[i];

                    // 가격 책정 로직...
                    fieldItem.price = GetPriceByRarity(itemsToSpawn[i].tier) * RunManager.Instance.CurrentFloor;
                }
            }
            SpawnExitPortal();
        }
        private int GetPriceByRarity(ItemTier rarity)
        {
            return rarity switch
            {
                ItemTier.Common => 40,   // 커먼: 40, 80, 120...
                ItemTier.Uncommon => 80, // 언커먼: 80, 160, 240...
                ItemTier.Rare => 150,   // 레어: 150, 300, 450...
                _ => 50
            };
        }
        private void SpawnRest()
        {
            Instantiate(healthRestorerPrefab, Vector3.zero, Quaternion.identity);
            SpawnExitPortal();
        }

        private void SpawnExitPortal()
        {
            // 플레이어 근처나 맵 중앙에 포탈 생성
            Instantiate(exitPortalPrefab, new Vector2(0.5f, 3.5f), Quaternion.identity);
        }

        private void NotifyBossWaveStart()
        {
            OnBossWaveStart?.Invoke();
        }

        private void NotifyBossSummon(EntityStats boss)
        {
            OnBossSummon?.Invoke(boss);
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
        private void SpawnBossRewards()
        {
            // 1. 보스 보상용 아이템 3개 가져오기 (보스니까 더 좋은 티어 확률을 높여도 좋습니다)
            List<PassiveItemData> rewards = RunManager.Instance.GetTierItemSet(ItemTier.Boss, 3, CurrentWave);

            for (int i = 0; i < rewards.Count; i++)
            {
                // 보상 아이템들 위치 선정 (가운데 정렬)
                Vector2 pos = new Vector2(-2f + (i * 2f), 1f);
                GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity);

                if (itemObj.TryGetComponent(out Level.FieldItem fieldItem))
                {
                    fieldItem.itemData = rewards[i];
                    fieldItem.price = 0; // 보상은 무료
                    fieldItem.isBossReward = true; // [중요] 하나 먹으면 나머지 사라지는 플래그
                }
            }
            Debug.Log("[WaveManager] 보스 보상 아이템 3개가 소환되었습니다.");
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