using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        [Header("References")]
        public PlayerController controller;
        private SpriteRenderer spriteRenderer;

        [Header("Sprites")]
        public Sprite[] idleSprites;
        public Sprite[] runSprites;
        public Sprite[] runUpsideSprites;

        [Header("Animation Settings")]
        public float fps = 10f;
        
        private float frameTimer;
        private int currentFrame;
        private Sprite[] currentAnim;
        private bool isFacingRight = true;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (controller == null)
            {
                controller = GetComponentInParent<PlayerController>();
            }
        }

        private void Update()
        {
            if (controller == null) return;

            Vector2 moveInput = controller.MoveInput;
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            
            // Flip logic
            if (isMoving && Mathf.Abs(moveInput.x) > 0.01f)
            {
                isFacingRight = moveInput.x > 0;
                spriteRenderer.flipX = !isFacingRight;
            }

            // Determine current animation
            Sprite[] targetAnim = idleSprites;
            if (isMoving)
            {
                if (moveInput.y > 0.1f)
                {
                    targetAnim = runUpsideSprites;
                }
                else
                {
                    targetAnim = runSprites;
                }
            }

            if (targetAnim != currentAnim)
            {
                currentAnim = targetAnim;
                currentFrame = 0;
                frameTimer = 0f;
                UpdateSprite();
            }

            // Animate
            if (currentAnim != null && currentAnim.Length > 0 && isMoving)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= 1f / fps)
                {
                    frameTimer = 0f;
                    currentFrame = (currentFrame + 1) % currentAnim.Length;
                    UpdateSprite();
                }
            }
            else if (!isMoving && currentAnim != null && currentAnim.Length > 0)
            {
                // Idle animation
                frameTimer += Time.deltaTime;
                if (frameTimer >= 1f / (fps / 2f)) // Idle might be slower
                {
                    frameTimer = 0f;
                    currentFrame = (currentFrame + 1) % currentAnim.Length;
                    UpdateSprite();
                }
            }
        }

        private void UpdateSprite()
        {
            if (currentAnim != null && currentAnim.Length > 0 && currentFrame < currentAnim.Length)
            {
                spriteRenderer.sprite = currentAnim[currentFrame];
            }
        }
    }
}
