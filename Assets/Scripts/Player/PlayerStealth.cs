using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerStealth : MonoBehaviour
    {
        [Header("Stealth / Dodge Settings")]
        [SerializeField] private float dodgeSpeed = 10f;
        [SerializeField] private float stealthDuration = 3f;
        [SerializeField] private float dodgeDuration = 0.3f;
        [SerializeField] private float cooldown = 5f;

        public bool IsDodging { get; private set; }
        public bool IsStealthActive { get; private set; }

        private float nextAvailableTime;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ActivateStealth(Vector2 direction)
        {
            if (Time.time < nextAvailableTime) return;

            if (direction == Vector2.zero)
                direction = transform.right; // Default direction if standing still

            StartCoroutine(StealthRoutine(direction));
        }

        private IEnumerator StealthRoutine(Vector2 dir)
        {
            // Start Dodge/Roll
            IsDodging = true;
            IsStealthActive = true;
            nextAvailableTime = Time.time + cooldown;
            rb.linearVelocity = dir * dodgeSpeed;

            // Visual feedback
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.3f; // Transparent
                spriteRenderer.color = c;
            }

            yield return new WaitForSeconds(dodgeDuration);
            IsDodging = false;
            rb.linearVelocity = Vector2.zero;

            // Continue Stealth for the rest of the duration
            yield return new WaitForSeconds(stealthDuration - dodgeDuration);

            // End Stealth
            IsStealthActive = false;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f; // Opaque
                spriteRenderer.color = c;
            }
        }
    }
}
