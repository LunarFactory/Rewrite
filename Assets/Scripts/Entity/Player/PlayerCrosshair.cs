using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Crosshair : MonoBehaviour
    {
        public static Crosshair Instance { get; private set; }
        [Header("Input Settings")]
        // 인스펙터에서 Player Input Actions의 'Point' 액션을 연결하세요.
        [SerializeField] private InputActionReference mousePosAction;

        private Camera _mainCam;
        private PlayerController _targetPlayer;
        private SpriteRenderer _dot;
        private LineRenderer[] _lines;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                _dot = GetComponentInChildren<SpriteRenderer>();
                _lines = GetComponentsInChildren<LineRenderer>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // 액션 활성화
            mousePosAction.action.Enable();
        }

        private void OnDisable()
        {
            // 액션 비활성화
            mousePosAction.action.Disable();
        }

        private void LateUpdate()
        {
            // 타임스케일이 0일 때 (UI 활성화/일시정지 중)에는 시스템 커서를 보여줌
            if (Time.timeScale == 0f)
            {
                SetVisibility(false);
                return;
            }

            // 1. 플레이어 존재 확인 (없으면 숨기기)
            if (_targetPlayer == null)
            {
                _targetPlayer = Object.FindAnyObjectByType<PlayerController>();
                if (_targetPlayer == null)
                {
                    SetVisibility(false);
                    return;
                }
            }

            SetVisibility(true);

            // 2. 카메라 캐싱 최적화
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            // 3. [핵심] Input System의 액션에서 직접 값 읽기
            Vector2 screenPos = mousePosAction.action.ReadValue<Vector2>();
            if (IsMouseOutOfBounds(screenPos)) return;

            // 4. 월드 좌표 변환 (Z값은 카메라와의 거리만큼)
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(
                screenPos.x,
                screenPos.y,
                Mathf.Abs(_mainCam.transform.position.z)
            ));

            worldPos.z = 0f;
            transform.position = worldPos;
        }
        private bool IsMouseOutOfBounds(Vector2 pos)
        {
            // 포커스를 잃었을 때 (0,0)으로 튀는 현상 방지
            if (pos == Vector2.zero) return true;

            // 실제 화면 해상도 밖으로 나갔는지 체크
            return pos.x < 0 || pos.x > Screen.width || pos.y < 0 || pos.y > Screen.height;
        }

        private void SetVisibility(bool visible)
        {
            Cursor.visible = !visible;
            if (_dot != null) _dot.enabled = visible;
            foreach (var line in _lines) if (line != null) line.enabled = visible;
        }
    }
}