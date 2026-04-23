using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem; // 인풋 시스템 추가

namespace Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        private PlayerController controller;
        private SpriteRenderer spriteRenderer;
        private Camera _mainCam;

        [Header("Input Settings")]
        // 인스펙터에서 Player Input Actions의 'Point' 액션(마우스 위치)을 연결하세요.
        [SerializeField]
        private InputActionReference mousePosAction;

        [Header("Atlas Settings")]
        public Sprite[] idleSheet;
        public Sprite[] runSheet;

        [Header("Animation Arrays (Auto-populated)")]
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
            _mainCam = Camera.main;
            if (_mainCam == null)
            {
                _mainCam = Camera.main;
                if (_mainCam == null)
                    return;
            }

            if (controller == null)
            {
                controller = GetComponent<PlayerController>();
                if (controller == null)
                    controller = GetComponentInParent<PlayerController>();
            }

            InitializeAnimations();
        }

        private void OnEnable()
        {
            // 액션 활성화
            if (mousePosAction != null)
                mousePosAction.action.Enable();
        }

        private void OnDisable()
        {
            // 액션 비활성화
            if (mousePosAction != null)
                mousePosAction.action.Disable();
        }

        private void InitializeAnimations()
        {
            if (idleSheet != null && idleSheet.Length > 0)
                idleSprites = idleSheet;

            if (runSheet != null && runSheet.Length > 0)
            {
                int runFrameCount = Mathf.Min(4, runSheet.Length);
                runSprites = runSheet.Take(runFrameCount).ToArray();

                if (runSheet.Length >= 5)
                {
                    int upsideCount = Mathf.Min(4, runSheet.Length - 4);
                    runUpsideSprites = runSheet.Skip(4).Take(upsideCount).ToArray();
                }
            }

            if (idleSprites != null && idleSprites.Length > 0)
            {
                currentAnim = idleSprites;
                currentFrame = 0;
                UpdateSprite();
            }
        }

        private void Update()
        {
            // 1. 참조 및 카메라 체크 (이전 로직 유지)
            if (_mainCam == null)
                _mainCam = Camera.main;
            if (controller == null)
                return;

            // 2. 이동 및 조준 방향 데이터 확보
            Vector2 moveInput = controller.MoveInput;
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            // 조준점 좌표 가져오기 (싱글톤 활용)
            Vector3 targetWorldPos =
                (Crosshair.Instance != null)
                    ? Crosshair.Instance.transform.position
                    : transform.position;
            Vector2 lookDir = (Vector2)(targetWorldPos - transform.position);

            // 3. 좌우 반전(Flip) 로직
            if (Mathf.Abs(lookDir.x) > 0.05f)
            {
                isFacingRight = lookDir.x > 0;
                spriteRenderer.flipX = !isFacingRight;
            }

            // 4. [중요] 애니메이션 결정 로직 (Null 체크 강화)
            Sprite[] targetAnim = GetTargetAnimation(isMoving, lookDir.y);

            // 5. 애니메이션 교체 시 초기화
            if (targetAnim != currentAnim)
            {
                if (targetAnim != null && targetAnim.Length > 0)
                {
                    currentAnim = targetAnim;
                    currentFrame = 0;
                    frameTimer = 0f;
                    UpdateSprite();
                }
                else
                {
                    // 만약 바꿀 애니메이션이 비어있다면 Idle로 강제 복구
                    currentAnim = idleSprites;
                }
            }

            // 6. 프레임 재생 로직 (fps가 0이면 재생 안 됨)
            if (fps <= 0)
                fps = 10f; // 최소 속도 보장
            float currentFps = isMoving ? fps : (fps / 2f);

            if (currentAnim != null && currentAnim.Length > 0)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= 1f / currentFps)
                {
                    frameTimer = 0f;
                    currentFrame = (currentFrame + 1) % currentAnim.Length;
                    UpdateSprite();
                }
            }
        }

        // 애니메이션 선택 로직 분리 (가독성)
        private Sprite[] GetTargetAnimation(bool isMoving, float lookY)
        {
            if (!isMoving)
                return idleSprites;

            // 달리는 중일 때 방향에 따라 애니메이션 선택
            // Upside 애니메이션이 비어있다면 일반 Run으로 대체하는 안전장치 포함
            if (lookY > 0.5f && runUpsideSprites != null && runUpsideSprites.Length > 0)
            {
                return runUpsideSprites;
            }

            return (runSprites != null && runSprites.Length > 0) ? runSprites : idleSprites;
        }

        private void UpdateSprite()
        {
            if (spriteRenderer == null)
                return;
            if (currentAnim != null && currentAnim.Length > 0 && currentFrame < currentAnim.Length)
            {
                spriteRenderer.sprite = currentAnim[currentFrame];
            }
        }

        [ContextMenu("Refresh From Sheets")]
        private void RefreshFromSheets()
        {
            InitializeAnimations();
        }
    }
}
