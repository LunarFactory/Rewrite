using UnityEngine;
using Item; // ItemData가 있는 네임스페이스
using Level; // IInteractable이 있는 네임스페이스
using Core;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FieldItem : InteractableBase
    {
        public PassiveItemData itemData; // 이 오브젝트가 어떤 아이템인지 저장
        public int price = 0; // 0이면 무료, 0보다 크면 상점 아이템

        private Transform FieldVisual;
        private SpriteRenderer spriteRenderer;

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
        }

        private void Update()
        {
            if (FieldVisual != null)
            {
                float newY = Mathf.Sin(Time.time * 2f) * 0.1f;
                // 부모(본체)는 가만히 있고, 자식(이미지)만 위아래로 움직입니다.
                FieldVisual.localPosition = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        // [IInteractable 구현] 상호작용 시 출력될 텍스트
        public override string GetInteractPrompt()
        {
            if (itemData == null) return "";
            // 가격이 있으면 가격 표시, 없으면 이름만 표시
            return price > 0 ? $"{itemData.itemName} 구매 ({price} 볼트)" : $"{itemData.itemName} 획득";
        }

        // [IInteractable 구현] E키를 눌렀을 때 실행될 로직
        public override void OnInteract(GameObject interactEntity)
        {
            // 상점 아이템일 경우 골드 체크 (RunManager에 Gold가 있다고 가정)
            if (price > 0)
            {
                if (RunManager.Instance != null)
                {
                    if (RunManager.Instance.Bolts >= price)
                    {
                        RunManager.Instance.AddBolts(-price);
                        GetItem();
                    }
                    else
                    {
                        Debug.Log("볼트가 부족합니다!");
                        return;
                    }
                }
            }
            else
            {
                GetItem();
            }
            if (itemData == null) return;

            // 1. 인벤토리에 아이템 추가
            InventoryManager.Instance.AddItem(itemData);

            // 2. 획득 로그 출력 (선택 사항)
            Debug.Log($"아이템 획득: {itemData.itemName}");

            // 3. 필드에서 제거
            Destroy(gameObject);
        }
        private void GetItem()
        {
            InventoryManager.Instance.AddItem(itemData);
            Destroy(gameObject);
        }
    }
}