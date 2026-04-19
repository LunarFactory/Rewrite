using UnityEngine;
using UnityEngine.InputSystem;
using Level;
using TMPro;

namespace Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        public float interactRange = 1.0f;
        public LayerMask interactLayer;
        public System.Action<InteractableBase> OnInteractableChanged;
        private InteractableBase _currentInteractable;// 실제 TMP 컴포넌트

        public InteractableBase currentInteractable
        {
            get => _currentInteractable;
            set
            {
                // [수정] 
                // 1. 기존 참조가 '진짜 null'이거나 '파괴(Missing)'된 상태인지 체크
                // 2. 혹은 새로운 값(value)과 실제로 다른지 체크
                bool isMissing = _currentInteractable == null || _currentInteractable.Equals(null);

                if (isMissing || _currentInteractable != value)
                {
                    // 안전하게 이전 대상의 외곽선을 끕니다 (이미 파괴됐다면 실행 안 됨)
                    if (!isMissing) _currentInteractable.ShowOutline(false);

                    _currentInteractable = value;

                    // 새로운 대상 설정 및 이벤트 알림
                    _currentInteractable?.ShowOutline(true);
                    OnInteractableChanged?.Invoke(_currentInteractable);
                }
            }
        }

        private void Update()
        {
            FindInteractable();

            if (currentInteractable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentInteractable.OnInteract(gameObject);
            }
        }

        private void FindInteractable()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange, interactLayer);
            float closestDist = float.MaxValue;
            InteractableBase closest = null;

            foreach (var col in colliders)
            {
                var interactable = col.GetComponentInParent<InteractableBase>();
                if (interactable != null)
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }
            currentInteractable = closest;
        }
    }
}