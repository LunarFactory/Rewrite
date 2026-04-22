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
            if (_mainCam == null)
            {
                _mainCam = Camera.main;
                if (_mainCam == null)
                    return;
            }

            // 2. 컨트롤러가 없으면 다시 찾기
            if (controller == null)
            {
                controller = GetComponentInParent<PlayerController>();
                if (controller == null)
                    return;
            }
            // 1. 이동 상태 확인 (기존 controller 활용)
            Vector2 moveInput = controller.MoveInput;
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            // 2. [핵심] Input System 액션에서 마우스 좌표 읽기
            Vector2 mouseScreenPos = Vector2.zero;
            if (mousePosAction != null)
            {
                mouseScreenPos = mousePosAction.action.ReadValue<Vector2>();
            }

            // 3. 월드 좌표 변환 및 방향 계산
            Vector3 worldMousePos = _mainCam.ScreenToWorldPoint(
                new Vector3(
                    mouseScreenPos.x,
                    mouseScreenPos.y,
                    Mathf.Abs(_mainCam.transform.position.z)
                )
            );

            Vector2 lookDir = (Vector2)(worldMousePos - transform.position);

            // Flip 로직 (마우스 위치 기준)
            if (Mathf.Abs(lookDir.x) > 0.01f)
            {
                isFacingRight = lookDir.x > 0;
                spriteRenderer.flipX = !isFacingRight;
            }

            // 애니메이션 결정 로직
            Sprite[] targetAnim = idleSprites;
            if (isMoving)
            {
                // 마우스가 캐릭터보다 위에 있으면 위쪽 달리기 애니메이션 재생
                targetAnim = (lookDir.y > 0.1f) ? runUpsideSprites : runSprites;
            }

            if (targetAnim != currentAnim)
            {
                currentAnim = targetAnim;
                currentFrame = 0;
                frameTimer = 0f;
                UpdateSprite();
            }

            // 프레임 재생 로직
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
