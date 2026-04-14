using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemySpriteAnimator : MonoBehaviour
    {
        public Sprite[] frames;
        public float fps = 10f;
        
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private float frameTimer;
        private int currentFrame;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Flip the sprite based on velocity
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            {
                // Assuming the original sprite faces right
                spriteRenderer.flipX = rb.linearVelocity.x < 0;
            }
            
            #if !UNITY_2023_1_OR_NEWER
            if (rb != null && Mathf.Abs(rb.velocity.x) > 0.01f)
            {
                spriteRenderer.flipX = rb.velocity.x < 0;
            }
            #endif

            // Animation Loop
            if (frames == null || frames.Length == 0) return;

            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / fps)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % frames.Length;
                spriteRenderer.sprite = frames[currentFrame];
            }
        }
    }
}
