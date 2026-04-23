using Core;
using Log;
using Unity.Cinemachine; // 시네머신 3.0 네임스페이스
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Weapon;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input Action References")]
        [Header("Movement")]
        private Vector2 moveInput;
        public Vector2 MoveInput => moveInput;
        private Rigidbody2D rb;

        private PlayerStats stats;

        [Header("Aiming & Weapon")]
        private Vector3 mousePos;
        public Transform weaponPivot;
        private WeaponBase currentWeapon;

        [Tooltip(
            "캐릭터 스프라이트의 시각적 중심. 스프라이트 Pivot이 발치에 있을 경우 Y값을 올려서 맞추세요."
        )]
        [SerializeField]
        private Vector2 aimCenterOffset = new Vector2(0f, 0.25f);

        [Header("Combat")]
        private bool isAttacking;
        private bool isStealth;

        [Header("Input Action References")]
        [SerializeField]
        private InputActionReference attackAction;

        [SerializeField]
        private InputActionReference stealthAction;

        private PlayerStealth stealth;

        /// <summary>조준 기준점 (스프라이트 시각적 중심)</summary>
        private Vector3 AimOrigin => transform.position + (Vector3)aimCenterOffset;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stealth = GetComponent<PlayerStealth>();
            stats = GetComponent<PlayerStats>();

            var objs = FindObjectsByType<PlayerStats>(FindObjectsInactive.Exclude);
            if (objs.Length > 1)
            {
                Destroy(gameObject); // 이미 존재하면 새로 생긴 녀석을 파괴
                return;
            }

            // Set gravity scale to 0 for Top-Down
            rb.gravityScale = 0f;
            if (currentWeapon == null)
            {
                currentWeapon = GetComponentInChildren<Weapon.WeaponBase>();
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 2. 무기 컴포넌트 확인
            if (currentWeapon == null)
            {
                currentWeapon = GetComponentInChildren<Weapon.WeaponBase>();
            }
            currentWeapon.Initialize(currentWeapon.weaponData);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var vcam = FindAnyObjectByType<CinemachineCamera>();
            if (vcam == null)
                return;

            var targetGroup = FindAnyObjectByType<CinemachineTargetGroup>();

            if (targetGroup == null)
            {
                GameObject tgObj = new GameObject("DynamicTargetGroup");
                targetGroup = tgObj.AddComponent<CinemachineTargetGroup>();
            }

            // 1. 기존 리스트 비우기
            targetGroup.Targets.Clear();

            // 2. [수정] 리스트에 직접 Target 데이터 추가하기
            // 플레이어 추가
            targetGroup.Targets.Add(
                new CinemachineTargetGroup.Target
                {
                    Object = this.transform,
                    Weight = 0.7f,
                    Radius = 1f,
                }
            );

            // 크로스헤어 추가
            if (Crosshair.Instance != null)
            {
                targetGroup.Targets.Add(
                    new CinemachineTargetGroup.Target
                    {
                        Object = Crosshair.Instance.transform,
                        Weight = 0.3f,
                        Radius = 1f,
                    }
                );
            }

            vcam.Target.TrackingTarget = targetGroup.transform;
            vcam.ForceCameraPosition(transform.position, Quaternion.identity);
        }

        private void Update()
        {
            HandleAiming();
            HandleActions();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnAttack(InputValue value)
        {
            isAttacking = value.isPressed;
        }

        public void OnStealth(InputValue value)
        {
            isStealth = value.isPressed;
        }

        private void HandleMovement()
        {
            if (stats.isStunned)
                return;
            // Compatibility for 2023+ (velocity or linearVelocity)
            rb.linearVelocity = moveInput.normalized * stats.MoveSpeed.GetValue();
        }

        private void HandleAiming()
        {
            // 1. 크로스헤어 위치 가져오기 (싱글톤 활용)
            if (Crosshair.Instance == null)
                return;
            Vector3 targetPos = Crosshair.Instance.transform.position;

            // 2. 방향 계산 (목표 지점 - 내 위치)
            Vector2 lookDir = (Vector2)(targetPos - weaponPivot.position);

            // 3. 무기 회전
            if (lookDir != Vector2.zero)
            {
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void HandleActions()
        {
            if (stats.isStunned)
                return;
            // 1. 공격 (연사형): 버튼을 '누르고 있는 동안' 계속 실행
            // InputActionReference의 .IsPressed()를 쓰면 떼는 순간 즉시 정지합니다.
            if (attackAction.action.IsPressed())
            {
                // 처음 눌렀을 때만 로그 기록 (중복 로그 방지)
                if (attackAction.action.WasPressedThisFrame())
                {
                    PlayerLogManager.Instance?.RecordAction();
                }

                if (currentWeapon != null)
                {
                    Vector2 aimDir = weaponPivot != null ? weaponPivot.right : transform.right;
                    currentWeapon.Fire(aimDir);
                    stealth.CancelStealth();
                }
            }

            // 2. 은신 (단발형): 버튼을 '누른 그 순간' 딱 한 번만 실행
            // WasPressedThisFrame()을 쓰면 꾹 누르고 있어도 한 번만 터집니다.
            if (stealthAction.action.WasPressedThisFrame() && stealth != null)
            {
                PlayerLogManager.Instance?.RecordAction();
                stealth.ActivateStealth(); // 이제 굴러가는 동안 한 번만 호출됨
            }
        }

        public WeaponData GetCurrentWeapon()
        {
            return currentWeapon.weaponData;
        }

        public void SetCurrentWeapon(WeaponData newData)
        {
            currentWeapon.Initialize(newData);
            currentWeapon.ResetFireDelay();
        }
    }
}
