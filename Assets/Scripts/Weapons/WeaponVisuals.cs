using UnityEngine;

namespace Weapons
{
    public class WeaponVisuals : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Offset of the pivot point from the center of the player when aiming right.")]
        [SerializeField] private Vector3 rightPivotOffset = new Vector3(0.2f, 0.3f, 0f);
        
        [Tooltip("Offset of the pivot point from the center of the player when aiming left.")]
        [SerializeField] private Vector3 leftPivotOffset = new Vector3(-0.2f, 0.3f, 0f);

        [Tooltip("Local offset to align the weapon handle (bottom-left of sprite) to the pivot point.")]
        [SerializeField] private Vector3 handleOffset = new Vector3(0.2f, 0.2f, 0f);
        
        private bool wasLeft = false;
        private SpriteRenderer sr;
        
        private void Update()
        {
            sr = transform.parent.transform.parent.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                int parentOrder = sr.sortingOrder;
            }
            HandleOrientation();
        }

        private void HandleOrientation()
        {
            if (transform.parent == null) return;

            // 1. Get angle from parent pivot
            // Transform.parent is expected to be the weaponPivot on the Player, which rotates towards the mouse
            float angle = transform.parent.eulerAngles.z;
            angle %= 360f;
            if (angle < 0) angle += 360f;

            // Hysteresis:
            // Default split is 90 and 270. 
            // If already on Left, wait until angle < 60 or > 300 to flip Right.
            // If already on Right, wait until angle > 120 and < 240 to flip Left.
            bool isLeft = wasLeft;
            if (wasLeft)
            {
                if (angle < 60f || angle > 300f)
                    isLeft = false;
            }
            else
            {
                if (angle > 120f && angle < 240f)
                    isLeft = true;
            }
            wasLeft = isLeft;
            
            // 2. Base point switching: Move the pivot point itself to the left or right side of the player
            // This ensures aiming direction is calculated from the offset position correctly.
            transform.parent.localPosition = isLeft ? leftPivotOffset : rightPivotOffset;

            // 3. Upright flipping & Handle positioning
            // When aiming left, parent pivot rotation makes the sprite upside down, so we flip the Y scale.
            float scaleY = isLeft ? -1f : 1f;

            // Apply handle offset. If we flip the sprite on Y, the handle offset on Y must also flip
            // so the hand remains rigidly attached to the bottom-left handle.
            Vector3 currentHandleOffset = new Vector3(handleOffset.x, handleOffset.y * scaleY, handleOffset.z);

            transform.localPosition = currentHandleOffset;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(1f, scaleY, 1f);
        }
    }
}
