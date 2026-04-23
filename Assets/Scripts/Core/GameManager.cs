using System;
using System.Collections.Generic;
using Enemy;
using Entity;
using Level;
using Player;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public enum ItemTier
    {
        Common,
        Uncommon,
        Rare,
        Boss,
    };

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
    };

    public class GameManager : MonoBehaviour // MCP SYNC TEST - IF YOU SEE THIS, IT WORKS!
    {
        public string projectVersion = "REWRITE-2026-v0.5"; // LIVE SYNC TEST - SHOULD APPEAR IN INSPECTOR
        public static GameManager Instance { get; private set; }
        private GameState State;
        private PlayerInput playerInputSystem;

        public bool isGameOver = false;

        [SerializeField]
        private GameObject playerCrosshairPrefab;

        [SerializeField]
        private GameObject enemyBasePrefab;

        [Header("맵 풀")]
        [SerializeField]
        private List<MapData> MapPool;

        [Header("적 개체 풀")]
        [SerializeField]
        private List<EnemyData> enemyPool; // 전체 적 데이터 리스트

        [SerializeField]
        private List<EnemyData> bossPool; // 보스전 대비용 추가

        [Header("FDT 개체")]
        public FDTObject fdtPrefab; // TextMeshPro가 붙은 FDTObject 프리팹
        public float defaultDuration = 0.75f;

        public static event Action<EntityStats> OnBossSummon;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            State = GameState.Playing; // Default for now
            gameObject.AddComponent<RunManager>();
            gameObject.AddComponent<UIManager>();
            var map = gameObject.AddComponent<MapManager>();
            map.LoadMapList(MapPool);
            gameObject.AddComponent<ProjectileManager>();
            gameObject.AddComponent<InventoryManager>();
            var fdt = gameObject.AddComponent<FDTManager>();
            fdt.SetFDTPrefab(fdtPrefab, defaultDuration);

            Instantiate(playerCrosshairPrefab);
        }

        private void Start()
        {
            playerInputSystem = FindAnyObjectByType<PlayerStats>().GetComponent<PlayerInput>();
            var ui = gameObject.GetComponent<UIManager>();
            ui.LoadUI(playerInputSystem);
        }

        public void ChangeState(GameState newState)
        {
            State = newState;
            // Handle state transitions (e.g., time scale for pause)
            Time.timeScale = (State == GameState.Paused) ? 0f : 1f;
        }

        // [통합] 모든 스폰은 이 메서드를 통과하여 Setup을 보장함
        public void ExecuteSpawn(EnemyData data, bool isBoss)
        {
            if (data == null || enemyBasePrefab == null)
                return;

            Vector2 spawnPos = UnityEngine.Random.insideUnitCircle * 8f;
            GameObject enemyObj = Instantiate(enemyBasePrefab, spawnPos, Quaternion.identity);

            if (enemyObj.TryGetComponent(out EnemyStats stats))
            {
                stats.data = data;
                stats.isBoss = isBoss; // 보스 여부 설정

                if (isBoss)
                    NotifyBossSummon(stats);
            }
        }

        private void NotifyBossSummon(EntityStats boss)
        {
            OnBossSummon?.Invoke(boss);
        }

        public List<EnemyData> GetEnemyPool()
        {
            return enemyPool;
        }

        public List<EnemyData> GetBossPool()
        {
            return bossPool;
        }
    }
}
