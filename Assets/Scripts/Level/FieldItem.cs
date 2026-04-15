using UnityEngine;
using Item; // ItemData가 있는 네임스페이스
using Level; // IInteractable이 있는 네임스페이스

namespace Level
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class FieldItem : MonoBehaviour, IInteractable
    {
        public PassiveItemData itemData; // 이 오브젝트가 어떤 아이템인지 저장

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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
            // 사인파를 이용해 위아래로 부드럽게 움직임 (둥둥 뜨는 효과)
            float newY = Mathf.Sin(Time.time * 2f) * 0.1f;
            spriteRenderer.transform.localPosition = new Vector3(0, newY, 0);
        }
        
        // [IInteractable 구현] 상호작용 시 출력될 텍스트
        public string GetInteractPrompt()
        {
            if (itemData == null) return "아이템";
            return $"{itemData.itemName} 획득";
        }

        // [IInteractable 구현] E키를 눌렀을 때 실행될 로직
        public void OnInteract(GameObject interactEntity)
        {
            if (itemData == null) return;

            // 1. 인벤토리에 아이템 추가
            InventoryManager.Instance.AddItem(itemData);

            // 2. 획득 로그 출력 (선택 사항)
            Debug.Log($"아이템 획득: {itemData.itemName}");

            // 3. 필드에서 제거
            Destroy(gameObject);
        }
    }
}