using System;
using System.Collections.Generic;
using Drone;
using Enemy;
using Entity;
using Level;
using Log;
using Pathfinding;
using Player;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        Playing,
        Paused,
        GameOver,
        GameClear,
    };

    public class GameManager : MonoBehaviour // MCP SYNC TEST - IF YOU SEE THIS, IT WORKS!
    {
        public string projectVersion = "REWRITE-2026-v0.5"; // LIVE SYNC TEST - SHOULD APPEAR IN INSPECTOR
        public static GameManager Instance { get; private set; }
        private GameState State;
        private PlayerInput playerInputSystem;

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
            Cursor.lockState = CursorLockMode.Confined;
            State = GameState.Playing; // Default for now
            gameObject.AddComponent<RunManager>();
            gameObject.AddComponent<UIManager>();
            var map = gameObject.AddComponent<MapManager>();
            map.LoadMapList(MapPool);
            gameObject.AddComponent<ProjectileManager>();
            gameObject.AddComponent<InventoryManager>();
            var fdt = gameObject.AddComponent<FDTManager>();
            fdt.SetFDTPrefab(fdtPrefab, defaultDuration);
            gameObject.AddComponent<LogTracker>();
            gameObject.AddComponent<DDAInferenceManager>();

            Instantiate(playerCrosshairPrefab);
            PlayerPrefs.SetInt("LobbyCredits", 10000);
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
            Time.timeScale =
                (
                    State == GameState.Paused
                    || State == GameState.GameOver
                    || State == GameState.GameClear
                )
                    ? 0f
                    : 1f;
        }

        // [통합] 모든 스폰은 이 메서드를 통과하여 Setup을 보장함
        public GameObject ExecuteSpawn(EnemyData data, bool isBoss, Vector2 spawnPos)
        {
            if (data == null || enemyBasePrefab == null)
                return null;
            if (AstarPath.active != null)
            {
                // 0.5f 이내에서 가장 가까운 walkable 노드를 찾습니다.
                var nnConstraint = NNConstraint.Default;
                nnConstraint.constrainWalkability = true; // 이동 가능한 노드만 찾기

                var info = AstarPath.active.GetNearest(spawnPos, nnConstraint);
                if (info.node != null)
                {
                    spawnPos = (Vector2)info.position; // 찾은 좌표로 스폰 위치 보정
                }
            }
            GameObject enemyObj = Instantiate(enemyBasePrefab, spawnPos, Quaternion.identity);

            if (enemyObj.TryGetComponent(out EnemyStats stats))
            {
                stats.data = data;
                stats.isBoss = isBoss; // 보스 여부 설정

                if (isBoss)
                    NotifyBossSummon(stats);
            }
            return enemyObj;
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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "LobbyScene") // 로비 씬 이름 확인
            {
                // 1. 드론 청소
                DroneManager.Instance.ClearAllDrones();
                if (MapManager.Instance != null)
                {
                    MapManager.Instance.LoadMapList(MapPool);
                }
            }
        }
    }
}
