using UnityEngine;
using System.Linq;

namespace Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        [Header("References")]
        public PlayerController controller;
        private SpriteRenderer spriteRenderer;

        [Header("Atlas Settings")]
        [Tooltip("Assign the sliced sprites from player_idle here.")]
        public Sprite[] idleSheet;
        [Tooltip("Assign the sliced sprites from player_run here. Frames 1-4 = Run, 5-8 = Run Upside.")]
        public Sprite[] runSheet;

        [Header("Animation Arrays (Auto-populated if Atlas used)")]
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
                controller = GetComponent<PlayerController>();
                if (controller == null) controller = GetComponentInParent<PlayerController>();
            }

            InitializeAnimations();
        }

        private void InitializeAnimations()
        {
            // IDLE: Use full idleSheet if available
            if (idleSheet != null && idleSheet.Length > 0)
            {
                idleSprites = idleSheet;
            }

            // RUN / RUN UPSIDE: Partition runSheet
            if (runSheet != null && runSheet.Length > 0)
            {
                // Run: frames 1-4 (indices 0-3)
                int runFrameCount = Mathf.Min(4, runSheet.Length);
                runSprites = runSheet.Take(runFrameCount).ToArray();

                // Run Upside: frames 5-8 (indices 4-7)
                if (runSheet.Length >= 5)
                {
                    int upsideCount = Mathf.Min(4, runSheet.Length - 4);
                    runUpsideSprites = runSheet.Skip(4).Take(upsideCount).ToArray();
                }
            }

            // Initialize current animation to Idle and show the first frame
            if (idleSprites != null && idleSprites.Length > 0)
            {
                currentAnim = idleSprites;
                currentFrame = 0;
                UpdateSprite();
                // Debug.Log($"[PlayerAnimator] Initialized with {idleSprites.Length} idle frames.");
            }
        }

        private void Update()
        {
            if (controller == null) return;

            Vector2 moveInput = controller.MoveInput;
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            
            // Mouse Look Logic
            Vector2 lookDir = Vector2.right;
            if (Camera.main != null && UnityEngine.InputSystem.Mouse.current != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
                lookDir = (Vector2)(mousePos - transform.position);
            }

            // Flip logic based on mouse position
            if (Mathf.Abs(lookDir.x) > 0.01f)
            {
                isFacingRight = lookDir.x > 0;
                spriteRenderer.flipX = !isFacingRight;
            }

            // Determine current animation
            Sprite[] targetAnim = idleSprites;
            if (isMoving)
            {
                if (lookDir.y > 0.1f)
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
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            if (currentAnim != null && currentAnim.Length > 0 && currentFrame < currentAnim.Length)
            {
                spriteRenderer.sprite = currentAnim[currentFrame];
               // Debug.Log($"[Animator] Sprite set to: {spriteRenderer.sprite.name} on {gameObject.name}");
            }
        }

        [ContextMenu("Refresh From Sheets")]
        private void RefreshFromSheets()
        {
            InitializeAnimations();
            Debug.Log($"Animations refreshed from sheets. Run: {runSprites?.Length ?? 0}, Upside: {runUpsideSprites?.Length ?? 0}");
        }
    }
}
