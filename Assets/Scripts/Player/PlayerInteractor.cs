using UnityEngine;
using UnityEngine.InputSystem;
using Level;

namespace Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        public float interactRange = 1.0f;
        private InteractableBase currentInteractable;

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
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);
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
            if (closest != currentInteractable)
            {
                // 1. 이전 대상의 외곽선을 끕니다.
                currentInteractable?.ShowOutline(false);

                // 2. 새로운 대상으로 교체합니다.
                currentInteractable = closest;

                // 3. 새로운 대상의 외곽선을 켭니다.
                currentInteractable?.ShowOutline(true);
            }
        }

        private void OnGUI()
        {
            if (currentInteractable != null && Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1f);
                if (screenPos.z > 0)
                {
                    string promptText = "[E] " + currentInteractable.GetInteractPrompt();

                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 20;
                    style.fontStyle = FontStyle.Bold;

                    Vector2 size = style.CalcSize(new GUIContent(promptText));
                    Rect textRect = new Rect(screenPos.x - size.x / 2, Screen.height - screenPos.y - 40, size.x, size.y);

                    // Shadow
                    style.normal.textColor = Color.black;
                    style.hover.textColor = Color.black;
                    GUI.Label(new Rect(textRect.x + 2, textRect.y + 2, textRect.width, textRect.height), promptText, style);

                    // Text
                    style.normal.textColor = Color.yellow;
                    style.hover.textColor = Color.yellow;
                    GUI.Label(textRect, promptText, style);
                }
            }
        }
    }
}
