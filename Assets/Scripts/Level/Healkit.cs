using UnityEngine;
using Player; // PlayerStats가 있는 네임스페이스

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Healkit : InteractableBase
    {
        [Header("Heal Settings")]
        [Range(0, 100)]
        public float healPercentage = 25f;

        [Header("Visuals")]
        public Sprite activeSprite;   // 사용 전 이미지
        public Sprite usedSprite;     // 사용 후 이미지

        private SpriteRenderer _sr;
        private bool _isUsed = false;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();

            // 초기 비주얼 설정
            if (activeSprite != null && _sr != null)
                _sr.sprite = activeSprite;
        }

        public override string GetInteractPrompt()
        {
            if (_isUsed) return "이미 사용된 키트입니다.";
            return $"체력 회복 [{healPercentage}%]";
        }

        public override void OnInteract(GameObject interactEntity)
        {
            if (_isUsed) return;

            // 1. 플레이어에게서 PlayerStats 컴포넌트를 찾습니다.
            if (interactEntity.TryGetComponent(out PlayerStats stats))
            {

                // 2. 최대 체력 기반으로 회복량 계산
                int healAmount = Mathf.RoundToInt((stats.MaxHealth * healPercentage) / 100);

                // 3. 플레이어 치료
                stats.Heal(healAmount);

                // 4. 상태 및 비주얼 업데이트 (1회성)
                _isUsed = true;
                if (usedSprite != null) _sr.sprite = usedSprite;
                GetComponent<BoxCollider2D>().enabled = false;

                Debug.Log($"[Healkit] 플레이어 {healAmount} 회복 완료!");
            }
        }
    }
}