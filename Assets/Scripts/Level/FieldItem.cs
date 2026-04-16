using UnityEngine;
using Item; 
using Level;
using Core;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FieldItem : MonoBehaviour, IInteractable
    {
        public PassiveItemData itemData; // 이 오브젝트가 어떤 아이템인지 저장
        public int price; // 아이템 구매 가격 (0이면 무료)
        public bool isShopItem; // 상점 아이템 여부
        public bool isBossReward; // 보스 보상 아이템 여부

        public Transform FieldVisual;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        }

        private void Update()
        {
            if (FieldVisual != null)
            {
                float newY = Mathf.Sin(Time.time * 2f) * 0.1f;
                // 부모(본체)는 가만히 있고, 자식(이미지)만 위아래로 움직입니다.
                FieldVisual.localPosition = new Vector3(0, newY, 0);
            }
        }

        // [IInteractable 구현] 상호작용 시 출력될 텍스트
        public string GetInteractPrompt()
        {
            if (itemData == null) return "아이템";
            
            if (price > 0)
            {
                return $"{itemData.itemName} 구매 [{price} 볼트]";
            }
            
            return $"{itemData.itemName} 획득";
        }

        // [IInteractable 구현] E키를 눌렀을 때 실행될 로직
        public void OnInteract(GameObject interactEntity)
        {
            if (itemData == null) return;

            // 0. 가격 체크
            if (price > 0)
            {
                if (interactEntity.TryGetComponent(out Player.PlayerStats stats))
                {
                    if (stats.Bolts >= price)
                    {
                        stats.UseBolts(price);
                    }
                    else
                    {
                        Debug.Log("볼트가 부족합니다!");
                        return;
                    }
                }
            }

            // 1. 인벤토리에 아이템 추가
            InventoryManager.Instance.AddItem(itemData);

            // 2. 획득 로그 출력
            Debug.Log($"아이템 획득: {itemData.itemName}");

            // [추가] 상점/보스 보상 처리 통보
            if (WaveManager.Instance != null)
            {
                if (isBossReward)
                {
                    WaveManager.Instance.OnBossItemPicked(this);
                }
                else if (isShopItem)
                {
                    WaveManager.Instance.OnShopItemPurchased(this);
                }
            }

            // 3. 필드에서 제거
            Destroy(gameObject);
        }
    }
}