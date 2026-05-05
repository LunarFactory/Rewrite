using System;
using System.Collections.Generic;
using Enemy;
using Item;
using Level;
using Log;
using UI;
using UnityEngine;

namespace Core
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public enum WaveType
        {
            Mob,
            Shop,
            Rest,
            Boss,
        }

        [field: SerializeField]
        public int CurrentWave { get; private set; } = 1;

        public int BaseBudget { get; private set; }
        public int FinalBudget { get; private set; }
        public static event System.Action<float, float, float, int, int, float, float, float> OnDDACalculated;

        // [추가] WaveManager가 직접 관리할 적 프리팹
        [Header("Wave Resources")]
        [SerializeField]
        private GameObject itemPrefab;

        [SerializeField]
        private List<EnemyData> bossPool; // 보스전 대비용 추가

        [SerializeField]
        private List<EnemyData> enemyPool; // 전체 적 데이터 리스트

        private Dictionary<EnemyData, int> _spawnTracker = new Dictionary<EnemyData, int>();

        [SerializeField]
        private int baseWaveBudget = 10; // 1층 1웨이브 기본 예산

        private float difficultyAlpha;

        [SerializeField]
        private int budgetIncreasePerWave = 2; // 웨이브당 증가치

        [SerializeField]
        private int budgetIncreasePerFloor = 20; // 층당 증가치

        [Header("Special Prefabs")]
        [SerializeField]
        private GameObject exitPortalPrefab; // 다음 웨이브로 가는 포탈

        [SerializeField]
        private GameObject healthRestorerPrefab; // 체력 회복 오브젝트
        public int activeEnemyCount = 0;

        public Vector2 bossSpawnPoint;
        public Vector2 rewardSpawnPoint;

        private GameManager gameManager;

        public static event Action OnBossWaveStart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance;
                enemyPool = gameManager.GetEnemyPool();
                bossPool = gameManager.GetBossPool();
            }
        }

        private void Start()
        {
            // 씬이 시작되자마자 실행됩니다.
            if (RunManager.Instance != null)
            {
                // 현재 RunManager에 저장된 층 번호로 웨이브를 시작합니다.
                StartFloor(RunManager.Instance.CurrentFloor);
            }
        }

        public void StartFloor(int floorNumber)
        {
            if (MapManager.Instance != null)
            {
                MapManager.Instance.LoadMap(floorNumber);
            }
            CurrentWave = 1;
            StartWave(CurrentWave);
        }

        public void StartWave(int waveNumber)
        {
            CurrentWave = waveNumber;
            switch (GetWaveType(CurrentWave))
            {
                case WaveType.Mob:
                case WaveType.Boss:
                    LogTracker.Instance.StartLogging(
                        RunManager.Instance.CurrentFloor,
                        CurrentWave,
                        RunManager.Instance.CurrentSeed.ToString()
                    );
                    break;
                case WaveType.Shop:
                case WaveType.Rest:
                default:
                    break;
            }
            WaveType type = GetWaveType(CurrentWave);

            switch (type)
            {
                case WaveType.Mob:
                    activeEnemyCount = 0;
                    float currentAlpha = DDAInferenceManager.Instance != null ? DDAInferenceManager.Instance.currentAlpha : 1.0f;
                    BaseBudget = baseWaveBudget + (waveNumber * budgetIncreasePerWave) + ((RunManager.Instance.CurrentFloor - 1) * budgetIncreasePerFloor);
                    FinalBudget = Mathf.RoundToInt(BaseBudget * currentAlpha);
                    SpawnEnemiesWithRules(FinalBudget); // budget 대신 FinalBudget 대입
                    break;
                case WaveType.Shop:
                    SpawnShop();
                    break;
                case WaveType.Rest:
                    SpawnRest();
                    break;
                case WaveType.Boss:
                    NotifyBossWaveStart();
                    gameManager.ExecuteSpawn(
                        bossPool[RunManager.Instance.CurrentFloor - 1],
                        true,
                        bossSpawnPoint
                    );
                    break;
            }
        }

        private void SpawnEnemiesWithRules(int totalBudget)
        {
            UnityEngine.Random.InitState(
                RunManager.Instance.GetCalculatedSeed("ItemSet_" + CurrentWave, true, 0.5f)
            );
            Debug.Log($"Total Budget: {totalBudget}, Pool Count: {enemyPool.Count}");
            _spawnTracker.Clear();

            // [규칙 1] 황금 비율 분배
            int specialBudget = Mathf.FloorToInt(totalBudget * 0.15f);
            int eliteBudget = Mathf.FloorToInt(totalBudget * 0.35f);
            int normalBudget = totalBudget - specialBudget - eliteBudget;

            // 상위 티어에서 남은 예산(leftover)을 하위 티어로 넘겨주는 구조
            int leftover = 0;
            leftover += SpawnTier(EnemyTier.Special, specialBudget);
            leftover += SpawnTier(EnemyTier.Elite, eliteBudget + leftover);
            SpawnTier(EnemyTier.Normal, normalBudget + leftover);
        }

        private int SpawnTier(EnemyTier tier, int budget)
        {
            int remainingBudget = budget;
            List<EnemyData> candidates = enemyPool.FindAll(e =>
                e.tier == tier
                && e.minFloor <= RunManager.Instance.CurrentFloor
                && e.maxFloor >= RunManager.Instance.CurrentFloor
            );

            if (candidates.Count == 0)
                return remainingBudget;

            int safetyNet = 0;
            while (remainingBudget > 0 && safetyNet < 100)
            {
                safetyNet++;
                ShuffleList(candidates);

                bool spawnedInThisLoop = false;
                foreach (var data in candidates)
                {
                    // [규칙 2] 맥스 카운트 체크 (SO 데이터 기준)
                    if (data.maxCountInWave > 0)
                    {
                        _spawnTracker.TryGetValue(data, out int currentCount);
                        if (currentCount >= data.maxCountInWave)
                            continue;
                    }

                    if (remainingBudget >= data.cost)
                    {
                        ExecuteSpawn(data, false);
                        remainingBudget -= data.cost;

                        if (!_spawnTracker.ContainsKey(data))
                            _spawnTracker[data] = 0;
                        _spawnTracker[data]++;
                        spawnedInThisLoop = true;
                    }
                }
                if (!spawnedInThisLoop)
                    break;
            }
            return remainingBudget;
        }

        // WaveManager.cs 내부
        private void ExecuteSpawn(EnemyData data, bool isBoss)
        {
            // 현재 맵에 있는 SpawnZone을 찾아 위치 요청
            MapSpawnZone zone = MapManager.Instance.CurrentSpawnZone;
            Vector2 spawnPos = (zone != null) ? zone.GetRandomLocation() : Vector2.zero;

            var enemy = gameManager.ExecuteSpawn(data, isBoss, spawnPos);
            activeEnemyCount++;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int rnd = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[rnd];
                list[rnd] = temp;
            }
        }

        public void OnEnemyDied()
        {
            if (GetWaveType(CurrentWave) == WaveType.Mob)
            {
                activeEnemyCount--;
                if (activeEnemyCount <= 0)
                {
                    if (ProjectileManager.Instance != null)
                    {
                        ProjectileManager.Instance.ClearAllProjectiles();
                    }
                    CompleteCurrentWave();
                }
            }
        }

        public void OnBossEnemyDied()
        {
            if (ProjectileManager.Instance != null)
            {
                ProjectileManager.Instance.ClearAllProjectiles();
            }
            EnemyStats[] activeMobs = FindObjectsByType<EnemyStats>();

            foreach (EnemyStats mob in activeMobs)
            {
                // 각 투사체 내부의 반납 로직(Deactivate 등)을 호출합니다.
                // 이 메서드는 우리가 앞서 작성했던 'Release' 로직을 포함해야 합니다.
                Destroy(mob.gameObject);
            }
            if (RunManager.Instance != null)
            {
                if (RunManager.Instance.CurrentFloor < 5)
                {
                    SpawnExitPortal();
                    SpawnBossRewards();
                }
                else
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.RequestStateChange(UIState.GameClear);
                    }
                }
            }
        }

        private void SpawnShop()
        {
            // 1. 중복 없는 아이템 세트 3개를 한 번에 가져옴
            List<PassiveItemData> itemsToSpawn = RunManager.Instance.GetRandomItemSet(
                CurrentWave,
                3
            );

            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                Vector2 pos = new Vector2(-2f + (i * 2f), rewardSpawnPoint.y - 3);
                GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity);

                if (itemObj.TryGetComponent(out FieldItem fieldItem))
                {
                    fieldItem.itemData = itemsToSpawn[i];

                    // 가격 책정 로직...
                    fieldItem.price =
                        GetPriceByRarity(itemsToSpawn[i].tier)
                        * (int)Math.Pow(2, RunManager.Instance.CurrentFloor - 1);
                }
            }
            SpawnExitPortal();
        }

        private int GetPriceByRarity(ItemTier rarity)
        {
            return rarity switch
            {
                ItemTier.Common => 40, // 커먼: 40, 80, 120...
                ItemTier.Uncommon => 80, // 언커먼: 80, 160, 240...
                ItemTier.Rare => 150, // 레어: 150, 300, 450...
                _ => 50,
            };
        }

        private void SpawnRest()
        {
            Instantiate(
                healthRestorerPrefab,
                new Vector2(rewardSpawnPoint.x, rewardSpawnPoint.y - 3),
                Quaternion.identity
            );
            SpawnExitPortal();
        }

        private void SpawnExitPortal()
        {
            // 플레이어 근처나 맵 중앙에 포탈 생성
            Instantiate(exitPortalPrefab, rewardSpawnPoint, Quaternion.identity);
        }

        private void NotifyBossWaveStart()
        {
            OnBossWaveStart?.Invoke();
        }

        public void CompleteCurrentWave()
        { // 1. [추가] 필드의 모든 총알 청소
            activeEnemyCount = 0;
            switch (GetWaveType(CurrentWave))
            {
                case WaveType.Boss:
                case WaveType.Mob:
                    if (DDAInferenceManager.Instance != null)
                    {
                        WaveLogData rawLog = LogTracker.Instance.CompleteLogging();
                        var (s, c, alpha) = DDAInferenceManager.Instance.InferDifficulty(rawLog);
                        ApplyDifficultyToGame(alpha);
                        LogTracker.Instance.EndWaveAndSend(alpha, s, c);
                        
                        // UI 송출용 데이터 정규화
                        float rawApm = rawLog.dashboard_summary.apm;
                        float accuracy = Mathf.Clamp01(rawLog.dashboard_summary.accuracy_rate);
                        float evasion = Mathf.Clamp01(1.0f - (rawLog.dashboard_summary.hits_taken / 20f));

                        int nextBaseBudget = baseWaveBudget + ((CurrentWave + 1) * budgetIncreasePerWave) + ((RunManager.Instance.CurrentFloor - 1) * budgetIncreasePerFloor);
                        int nextFinalBudget = Mathf.RoundToInt(nextBaseBudget * alpha);

                        OnDDACalculated?.Invoke(s, c, alpha, nextBaseBudget, nextFinalBudget, rawApm, accuracy, evasion);
                    }
                    else
                    {
                        LogTracker.Instance.EndWaveAndSend(0.5f, 0.5f, 0.5f);
                    }
                    break;
                case WaveType.Rest:
                case WaveType.Shop:
                default:
                    break;
            }

            if (CurrentWave < 9)
            {
                Player.PlayerStats.LocalPlayer.AddBolts(30 * RunManager.Instance.CurrentFloor);
                StartWave(CurrentWave + 1);
            }
            else
            {
                RunManager.Instance.AdvanceFloor();
            }
        }

        private void ApplyDifficultyToGame(float alpha)
        {
            difficultyAlpha = alpha;
        }

        private void SpawnBossRewards()
        {
            // 1. 보스 보상용 아이템 3개 가져오기 (보스니까 더 좋은 티어 확률을 높여도 좋습니다)
            List<PassiveItemData> rewards = RunManager.Instance.GetTierItemSet(
                ItemTier.Boss,
                3,
                CurrentWave
            );

            for (int i = 0; i < rewards.Count; i++)
            {
                // 보상 아이템들 위치 선정 (가운데 정렬)
                Vector2 pos = new Vector2(-2f + (i * 2f), rewardSpawnPoint.y - 3);
                GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity);

                if (itemObj.TryGetComponent(out Level.FieldItem fieldItem))
                {
                    fieldItem.itemData = rewards[i];
                    fieldItem.price = 0; // 보상은 무료
                    fieldItem.isBossReward = true; // [중요] 하나 먹으면 나머지 사라지는 플래그
                }
            }
        }

        private WaveType GetWaveType(int wave)
        {
            if (wave == 4)
                return WaveType.Shop;
            if (wave == 8)
                return WaveType.Rest;
            if (wave == 9)
                return WaveType.Boss;
            return WaveType.Mob;
        }
    }
}
