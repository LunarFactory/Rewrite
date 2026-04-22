using System;
using Level;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        public float interactRange = 1.0f;
        public LayerMask interactLayer;
        public Action<InteractableBase> OnInteractableChanged;
        private PlayerInput playerInput;
        private InputAction interactAction;

        private InteractableBase _currentInteractable; // 실제 TMP 컴포넌트
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
                    if (!isMissing)
                        _currentInteractable.ShowOutline(false);

                    _currentInteractable = value;

                    // 새로운 대상 설정 및 이벤트 알림
                    _currentInteractable?.ShowOutline(true);
                    OnInteractableChanged?.Invoke(_currentInteractable);
                }
            }
        }

        void Awake()
        {
            playerInput = gameObject.GetComponent<PlayerInput>();
            interactAction = playerInput.actions.FindActionMap("Player").FindAction("Interact");
        }

        private void OnEnable()
        {
            // 1. 액션 활성화 및 이벤트 구독
            if (interactAction != null)
            {
                interactAction.Enable();
                interactAction.performed += OnInteractPerformed;
            }
        }

        private void OnDisable()
        {
            // 2. 이벤트 구독 해제 및 액션 비활성화
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
                interactAction.Disable();
            }
        } // 3. 인풋 이벤트가 발생했을 때 호출되는 함수

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            // 게임이 일시정지 중이거나 상호작용 대상이 없으면 리턴
            if (Time.timeScale == 0f || currentInteractable == null)
                return;

            currentInteractable.OnInteract(gameObject);
        }

        private void Update()
        {
            FindInteractable();
        }

        private void FindInteractable()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position,
                interactRange,
                interactLayer
            );
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
