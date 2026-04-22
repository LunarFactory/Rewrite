using Core;
using Item; // ItemData가 있는 네임스페이스
using Level; // IInteractable이 있는 네임스페이스
using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FieldItem : InteractableBase
    {
        public PassiveItemData itemData; // 이 오브젝트가 어떤 아이템인지 저장
        public int price = 0; // 0이면 무료, 0보다 크면 상점 아이템

        private Transform FieldVisual;
        private SpriteRenderer spriteRenderer;
        private Vector3 _basePosition;

        public bool isBossReward = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            FieldVisual = spriteRenderer.transform;
            // 콜라이더를 트리거로 설정 (물리 충돌 방지, 감지만 수행)
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Start()
        {
            // 데이터에 설정된 아이콘으로 자동으로 외형 설정
            if (itemData != null)
            {
                spriteRenderer.sprite = itemData.icon;
            }
            _basePosition = transform.position;
        }

        private void Update()
        {
            if (FieldVisual != null)
            {
                float newY = Mathf.Sin(Time.time * 2f) * 0.1f;
                FieldVisual.localPosition = _basePosition + new Vector3(0, newY, 0);
            }
        }

        // [IInteractable 구현] 상호작용 시 출력될 텍스트
        public override string GetInteractPrompt()
        {
            if (itemData == null)
                return "";
            // 가격이 있으면 가격 표시, 없으면 이름만 표시
            return price > 0
                ? $"{itemData.itemName} 구매 ({price} 볼트)"
                : $"{itemData.itemName} 획득";
        }

        // [IInteractable 구현] E키를 눌렀을 때 실행될 로직
        public override void OnInteract(GameObject interactEntity)
        {
            // 상점 아이템일 경우 골드 체크 (RunManager에 Gold가 있다고 가정)
            if (price > 0)
            {
                if (interactEntity.TryGetComponent(out Player.PlayerStats stats))
                {
                    if (stats.GetBolts() >= price)
                    {
                        stats.AddBolts(-price);
                        GetItem();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                // 2. 무료 아이템(일반 보상)일 때
                if (isBossReward)
                    DestroyOtherBossRewards();
                GetItem();
            }
            if (itemData == null)
                return;

            // 3. 필드에서 제거
            Destroy(gameObject);
        }

        private void GetItem()
        {
            InventoryManager.Instance.AddItem(itemData);
            Destroy(gameObject);
        }

        private void DestroyOtherBossRewards()
        {
            // 현재 씬에 있는 모든 FieldItem을 찾습니다.
            // Unity 2023 이상이라면 FindObjectsByType을 사용하고, 이전 버전이라면 FindObjectsOfType을 사용하세요.
            FieldItem[] allItems = Object.FindObjectsByType<FieldItem>();

            foreach (var item in allItems)
            {
                // 자기 자신은 제외하고, 다른 보스 보상 아이템들만 파괴합니다.
                if (item != this && item.isBossReward)
                {
                    // 화려한 연출을 원한다면 여기에 파티클 생성을 추가할 수 있습니다.
                    Destroy(item.gameObject);
                }
            }
        }
    }
}
