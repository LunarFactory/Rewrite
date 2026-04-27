using System.Collections.Generic;
using Item;
using Log;
using UnityEngine;

namespace Core
{
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        public int CurrentFloor { get; private set; } = 1;
        public int CurrentSeed { get; private set; }

        [Header("Item Pool")]
        // 등급별로 미리 분류된 딕셔너리 (검색 속도 O(1))
        private Dictionary<ItemTier, List<PassiveItemData>> _itemPool =
            new Dictionary<ItemTier, List<PassiveItemData>>();

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

        public int GetCalculatedSeed(string subKey, bool useAlpha = false, float currentAlpha = 0)
        {
            string combinedKey;

            if (useAlpha)
            {
                // 몹 생성처럼 알파에 영향을 받아야 하는 경우
                combinedKey = $"{CurrentSeed}_{CurrentFloor}_{subKey}_{currentAlpha:F2}";
            }
            else
            {
                // 맵이나 아이템처럼 알파가 변해도 결과가 같아야 하는 경우
                combinedKey = $"{CurrentSeed}_{CurrentFloor}_{subKey}";
            }

            return combinedKey.GetHashCode();
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
                return;
            }
            foreach (var item in allItems)
            {
                _itemPool[item.tier].Add(item);
            }
        }

        // 2. 시드 기반 아이템 결정 로직
        public List<PassiveItemData> GetRandomItemSet(int wave, int count)
        {
            Random.State originalState = Random.state;
            // 웨이브 전체에 대한 고유 시드 설정
            Random.InitState(GetCalculatedSeed("ItemSet_" + wave));

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
                    !pickedInThisSet.Contains(item) && !InventoryManager.Instance.HasItem(item) // 이미 가진 아이템 제외
                );

                // 만약 해당 티어에 남은 아이템이 없다면 Common에서 다시 시도
                if (validPool.Count == 0)
                {
                    validPool = _itemPool[ItemTier.Common]
                        .FindAll(item =>
                            !pickedInThisSet.Contains(item)
                            && !InventoryManager.Instance.HasItem(item)
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

        public List<PassiveItemData> GetTierItemSet(ItemTier targetTier, int count, int wave)
        {
            Random.State originalState = Random.state;
            // 웨이브 전체에 대한 고유 시드 설정
            Random.InitState(GetCalculatedSeed("ItemTierSet_" + wave));

            List<PassiveItemData> resultList = new List<PassiveItemData>();
            HashSet<PassiveItemData> pickedInThisSet = new HashSet<PassiveItemData>();

            // 해당 티어의 전체 풀 가져오기
            List<PassiveItemData> fullPool = _itemPool[targetTier];

            for (int i = 0; i < count; i++)
            {
                // 1. 현재 풀에서 유효한 아이템 필터링 (중복 제외 + 미보유)
                List<PassiveItemData> validPool = fullPool.FindAll(item =>
                    !pickedInThisSet.Contains(item) && !InventoryManager.Instance.HasItem(item)
                );

                // 2. 만약 해당 티어의 아이템이 다 떨어졌다면? (하위 티어에서 보충)
                if (validPool.Count == 0)
                {
                    // 한 단계 아래 티어 혹은 Common에서 남은 거라도 찾아옵니다.
                    ItemTier fallbackTier =
                        (targetTier > ItemTier.Common) ? targetTier - 1 : ItemTier.Common;
                    validPool = _itemPool[fallbackTier]
                        .FindAll(item =>
                            !pickedInThisSet.Contains(item)
                            && !InventoryManager.Instance.HasItem(item)
                        );
                }

                if (validPool.Count > 0)
                {
                    PassiveItemData picked = validPool[Random.Range(0, validPool.Count)];
                    resultList.Add(picked);
                    pickedInThisSet.Add(picked);
                }
            }

            Random.state = originalState;
            return resultList;
        }

        private ItemTier RollRarity()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < 60f)
                return ItemTier.Common;
            if (roll < 90f)
                return ItemTier.Uncommon;
            return ItemTier.Rare;
        }

        public void StartNewRun()
        {
            var rand = new System.Random();
            CurrentSeed = rand.Next(10000, 99999);
            CurrentFloor = 5;
            Random.InitState(CurrentSeed);

            LogTracker.Instance.GenerateRunId();

            UI.UIManager.Instance.LoadScene("GameScene");
        }

        public void AdvanceFloor()
        {
            if (CurrentFloor < 5)
            {
                CurrentFloor++;
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.StartFloor(CurrentFloor);
                }
            }
            else
            {
                LogTracker.Instance.EndWaveAndSend(0.5f, 0.5f, 0.5f);
                LogTracker.Instance.OnRunEnded("CLEAR");
            }
        }
    }
}
