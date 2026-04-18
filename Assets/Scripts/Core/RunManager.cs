using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Item;

namespace Core
{
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        public int CurrentFloor { get; private set; } = 1;
        public int CurrentSeed { get; private set; }
        [Header("Item Pool")]
        // 등급별로 미리 분류된 딕셔너리 (검색 속도 O(1))
        private Dictionary<ItemTier, List<PassiveItemData>> _itemPool = new Dictionary<ItemTier, List<PassiveItemData>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeItemDatabase();
        }

        private void InitializeItemDatabase()
        {
            _itemPool.Clear();
            _itemPool[ItemTier.Common] = new List<PassiveItemData>();
            _itemPool[ItemTier.Uncommon] = new List<PassiveItemData>();
            _itemPool[ItemTier.Rare] = new List<PassiveItemData>();
            _itemPool[ItemTier.Boss] = new List<PassiveItemData>();
            // Resources/Items 폴더 아래의 모든 PassiveItemData 로드
            PassiveItemData[] allItems = Resources.LoadAll<PassiveItemData>("Items");
            if (allItems.Length == 0)
            {
                Debug.LogError("[RunManager] Resources/Items 폴더에 아이템 데이터가 없습니다! 폴더 구조를 확인하세요.");
                return;
            }
            foreach (var item in allItems)
            {
                // 보스 아이템은 일반 풀에서 제외
                if (item.tier == ItemTier.Boss) continue;
                _itemPool[item.tier].Add(item);
            }
            Debug.Log($"[RunManager] 아이템 데이터베이스 구축 완료. 총 {allItems.Length}개 로드.");
        }

        // 2. 시드 기반 아이템 결정 로직
        public List<PassiveItemData> GetRandomItemSet(int wave, int count)
        {
            Random.State originalState = Random.state;
            // 웨이브 전체에 대한 고유 시드 설정
            int uniqueSeed = CurrentSeed + (CurrentFloor * 1000) + (wave * 100);
            Random.InitState(uniqueSeed);

            List<PassiveItemData> resultList = new List<PassiveItemData>();
            // 이미 이번 세트에서 뽑힌 아이템을 추적 (이름이나 객체로 비교)
            HashSet<PassiveItemData> pickedInThisSet = new HashSet<PassiveItemData>();

            for (int i = 0; i < count; i++)
            {
                ItemTier selectedTier = RollRarity();

                // 해당 티어의 아이템 풀을 가져옴
                List<PassiveItemData> fullPool = _itemPool[selectedTier];

                // [핵심] 전체 풀에서 이미 뽑힌 것과 인벤토리에 있는 것을 제외한 '진짜 풀' 생성
                List<PassiveItemData> validPool = fullPool.FindAll(item =>
                    !pickedInThisSet.Contains(item) &&
                    !InventoryManager.Instance.HasItem(item) // 이미 가진 아이템 제외
                );

                // 만약 해당 티어에 남은 아이템이 없다면 Common에서 다시 시도
                if (validPool.Count == 0)
                {
                    validPool = _itemPool[ItemTier.Common].FindAll(item =>
                        !pickedInThisSet.Contains(item) &&
                        !InventoryManager.Instance.HasItem(item)
                    );
                }

                if (validPool.Count > 0)
                {
                    PassiveItemData picked = validPool[Random.Range(0, validPool.Count)];
                    resultList.Add(picked);
                    pickedInThisSet.Add(picked); // 중복 방지 목록에 추가
                }
            }

            Random.state = originalState;
            return resultList;
        }

        private ItemTier RollRarity()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < 60f) return ItemTier.Common;
            if (roll < 90f) return ItemTier.Uncommon;
            return ItemTier.Rare;
        }

        public void StartNewRun()
        {
            CurrentSeed = Random.Range(1000, 99999);
            CurrentFloor = 1;
            Random.InitState(CurrentSeed);
            Debug.Log($"[RunManager] New Run Started - Seed: {CurrentSeed}, Floor: {CurrentFloor}");
        }

        public void AdvanceFloor()
        {
            if (CurrentFloor < 5)
            {
                CurrentFloor++;
                Debug.Log($"[RunManager] Advanced to Floor {CurrentFloor}");
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.StartFloor(CurrentFloor);
                }
            }
            else
            {
                Debug.Log("[RunManager] Run Cleared!");
                // Game clear logic
            }
        }
    }
}
