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
            private set
            {
                if (_currentInteractable != value)
                {
                    _currentInteractable?.ShowOutline(false);
                    _currentInteractable = value;
                    _currentInteractable?.ShowOutline(true);
                    OnInteractableChanged?.Invoke(_currentInteractable); // 값이 바뀔 때만 알림!
                }
            }
        }

        private void Update()
        {
            FindInteractable();

            if (currentInteractable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentInteractable.OnInteract(gameObject);
                currentInteractable = null;
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