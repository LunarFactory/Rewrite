using UnityEngine;
using System.Collections.Generic;
using Level;
using Enemy;

namespace Core
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public enum WaveType { Mob, Shop, Rest, Boss }
        [field: SerializeField] public int CurrentWave { get; private set; } = 1;

        [Header("Wave Resources")]
        [SerializeField] private GameObject mobPrefab;
        [SerializeField] private GameObject bossPrefab; 
        
        [Header("Shop Settings")]
        [SerializeField] private GameObject fieldItemPrefab;
        [SerializeField] private System.Collections.Generic.List<Item.PassiveItemData> allItems;
        [SerializeField] private GameObject waveTransitionPrefab;
        
        [Header("Spawn Tracking")]
        [SerializeField] private List<EnemyBase> spawnedEnemies = new List<EnemyBase>();
        private System.Collections.Generic.List<FieldItem> spawnedShopItems = new System.Collections.Generic.List<FieldItem>();
        private System.Collections.Generic.List<FieldItem> spawnedBossRewards = new System.Collections.Generic.List<FieldItem>();

        private bool isWaitingForManualTransition = false;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                var runManager = GameManager.Instance.GetComponent<RunManager>();
                if (runManager != null)
                {
                    Debug.Log($"현재 런의 무기: {runManager.GetWeapon().WeaponName}");
                }
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

        private void Update()
        {
            if (isWaitingForManualTransition && Input.GetKeyDown(KeyCode.T))
            {
                isWaitingForManualTransition = false;
                UI.GameHUD.Instance?.HideWavePrompt();
                CompleteCurrentWave();
            }
        }

        public void StartFloor(int floorNumber)
        {
            CurrentWave = 1;
            StartWave(CurrentWave);
        }

        public void StartWave(int waveNumber)
        {
            ClearCurrentWaveObjects();
            isWaitingForManualTransition = false;

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
                    SpawnShopItems();
                    break;
                case WaveType.Rest:
                    HandleRestWave();
                    break;
                case WaveType.Boss:
                    SpawnBoss();
                    break;
            }
        }

        private void ClearCurrentWaveObjects()
        {
            foreach (var enemy in spawnedEnemies) if (enemy != null) Destroy(enemy.gameObject);
            spawnedEnemies.Clear();

            foreach (var item in spawnedShopItems) if (item != null) Destroy(item.gameObject);
            spawnedShopItems.Clear();

            foreach (var reward in spawnedBossRewards) if (reward != null) Destroy(reward.gameObject);
            spawnedBossRewards.Clear();

            var existingTriggers = Object.FindObjectsByType<WaveTransitionTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var t in existingTriggers) if (t != null) Destroy(t.gameObject);

            UI.GameHUD.Instance?.HideWavePrompt();
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null) return;
            GameObject bossGo = Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
            if (bossGo.TryGetComponent(out BossEnemy boss))
            {
                boss.bossDisplayName = GetBossName(RunManager.Instance.CurrentFloor);
            }
            if (bossGo.TryGetComponent(out EnemyBase enemy))
            {
                RegisterEnemy(enemy);
            }
        }

        private string GetBossName(int floor)
        {
            return floor switch
            {
                1 => "M3-K4",
                2 => "Gemini A & B",
                3 => "Maya",
                4 => "Meta",
                5 => "Omega",
                _ => "Unknown Entity"
            };
        }

        private void SpawnEnemies(GameObject prefab, int count)
        {
            if (prefab == null) return;
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = Random.insideUnitCircle * 5f;
                GameObject enemyGo = Instantiate(prefab, spawnPos, Quaternion.identity);
                if (enemyGo.TryGetComponent(out EnemyBase enemy))
                {
                    RegisterEnemy(enemy);
                }
            }
        }

        private void RegisterEnemy(EnemyBase enemy)
        {
            spawnedEnemies.Add(enemy);
            enemy.OnDeath += () => OnEnemyDefeated(enemy);
        }

        private void OnEnemyDefeated(EnemyBase enemy)
        {
            spawnedEnemies.Remove(enemy);
            if (spawnedEnemies.Count == 0)
            {
                Debug.Log($"[WaveManager] Wave {CurrentWave} Cleared!");
                WaveType type = GetWaveType(CurrentWave);
                if (type == WaveType.Boss) SpawnBossRewards();
                else if (type == WaveType.Mob) CompleteCurrentWave();
            }
        }

        public void CompleteCurrentWave()
        {
            Debug.Log($"[WaveManager] Wave {CurrentWave} Completed.");
            Log.PlayerLogManager.Instance?.OnWaveCompleted(CurrentWave);

            if (CurrentWave < 9) StartWave(CurrentWave + 1);
            else RunManager.Instance.AdvanceFloor();
        }

        private void SpawnShopItems()
        {
            if (fieldItemPrefab == null || allItems == null || allItems.Count == 0) return;

            var shopPool = allItems.FindAll(i => i.tier != Item.ItemTier.Boss);
            if (shopPool.Count < 3) return;

            var selectedItems = new List<Item.PassiveItemData>();
            var tempPool = new List<Item.PassiveItemData>(shopPool);
            for (int i = 0; i < 3; i++)
            {
                int r = Random.Range(0, tempPool.Count);
                selectedItems.Add(tempPool[r]);
                tempPool.RemoveAt(r);
            }

            float[] xPos = { -3f, 0f, 3f };
            spawnedShopItems.Clear();
            for (int i = 0; i < 3; i++)
            {
                GameObject itemGo = Instantiate(fieldItemPrefab, new Vector3(xPos[i], 0, 0), Quaternion.identity);
                if (itemGo.TryGetComponent(out FieldItem fi))
                {
                    fi.itemData = selectedItems[i];
                    fi.price = CalculatePrice(selectedItems[i]);
                    fi.isShopItem = true;
                    spawnedShopItems.Add(fi);
                }
            }

            isWaitingForManualTransition = true;
            UI.GameHUD.Instance?.ShowWavePrompt("다음 웨이브로 진행하려면 [T]를 누르세요");
        }

        private void SpawnBossRewards()
        {
            if (fieldItemPrefab == null || allItems == null) return;
            var bossPool = allItems.FindAll(i => i.tier == Item.ItemTier.Boss);
            if (bossPool.Count < 3) return;

            var selectedItems = new List<Item.PassiveItemData>();
            var tempPool = new List<Item.PassiveItemData>(bossPool);
            for (int i = 0; i < 3; i++)
            {
                int r = Random.Range(0, tempPool.Count);
                selectedItems.Add(tempPool[r]);
                tempPool.RemoveAt(r);
            }

            float[] xPos = { -3f, 0f, 3f };
            spawnedBossRewards.Clear();
            for (int i = 0; i < 3; i++)
            {
                GameObject itemGo = Instantiate(fieldItemPrefab, new Vector3(xPos[i], 0, 0), Quaternion.identity);
                if (itemGo.TryGetComponent(out FieldItem fi))
                {
                    fi.itemData = selectedItems[i];
                    fi.price = 0;
                    fi.isBossReward = true;
                    spawnedBossRewards.Add(fi);
                }
            }
        }

        private int CalculatePrice(Item.PassiveItemData item)
        {
            int baseP = item.tier switch
            {
                Item.ItemTier.Common => 25,
                Item.ItemTier.Uncommon => 50,
                Item.ItemTier.Rare => 100,
                _ => 10
            };
            return Mathf.RoundToInt(baseP * Random.Range(0.8f, 1.2f));
        }

        public void OnShopItemPurchased(FieldItem item)
        {
            if (spawnedShopItems.Contains(item)) spawnedShopItems.Remove(item);
        }

        public void OnBossItemPicked(FieldItem pickedItem)
        {
            foreach (var item in spawnedBossRewards)
            {
                if (item != null && item != pickedItem) Destroy(item.gameObject);
            }
            spawnedBossRewards.Clear();
            SpawnTransitionPortal();
        }

        private void HandleRestWave()
        {
            var playerStats = Object.FindAnyObjectByType<Player.PlayerStats>();
            if (playerStats != null) playerStats.Heal(playerStats.MaxHealth * 0.5f);

            isWaitingForManualTransition = true;
            UI.GameHUD.Instance?.ShowWavePrompt("다음 웨이브로 진행하려면 [T]를 누르세요");
        }

        private void SpawnTransitionPortal()
        {
            if (waveTransitionPrefab != null) Instantiate(waveTransitionPrefab, Vector3.zero, Quaternion.identity);
            else CompleteCurrentWave();
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