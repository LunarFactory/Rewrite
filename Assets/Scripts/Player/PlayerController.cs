using UnityEngine;
using UnityEngine.InputSystem;
using Log;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 moveInput;
        public Vector2 MoveInput => moveInput;
        private Rigidbody2D rb;

        [Header("Aiming & Weapon")]
        private Vector3 mousePos;
        public Transform weaponPivot;
        [SerializeField] public Weapons.WeaponBase currentWeapon;

        [Tooltip("캐릭터 스프라이트의 시각적 중심. 스프라이트 Pivot이 발치에 있을 경우 Y값을 올려서 맞추세요.")]
        [SerializeField] private Vector2 aimCenterOffset = new Vector2(0f, 0.25f);

        [Header("Combat")]
        private bool isAttacking;

        private PlayerStealth stealth;

        /// <summary>조준 기준점 (스프라이트 시각적 중심)</summary>
        private Vector3 AimOrigin => transform.position + (Vector3)aimCenterOffset;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stealth = GetComponent<PlayerStealth>();
            
            // Set gravity scale to 0 for Top-Down
            rb.gravityScale = 0f;
            currentWeapon = GetComponentInChildren<Weapons.WeaponBase>();
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

        private void HandleMovement()
        {
            if (stealth != null && stealth.IsDodging) 
            {
                return; // Maintain dodging velocity
            }
            // Compatibility for 2023+ (velocity or linearVelocity)
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }

        private void HandleAiming()
        {
            if (Camera.main == null || Mouse.current == null) return;

            mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0f;

            Transform pivot = weaponPivot != null ? weaponPivot : transform;

            // 항상 스프라이트 시각적 중심(AimOrigin)을 기준으로 각도 계산
            // → 거리에 따라 기준점이 바뀌는 스위칭 없이 안정적
            Vector2 lookDir = (Vector2)(mousePos - AimOrigin);
            if (lookDir != Vector2.zero)
            {
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                pivot.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void HandleActions()
        {
            if (Mouse.current == null) return;

            bool leftHeld = Mouse.current.leftButton.isPressed;

            if (leftHeld && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlayerLogManager.Instance?.RecordAction();
            }

            if (leftHeld)
            {
                Transform pivot = weaponPivot != null ? weaponPivot : transform;
                if (currentWeapon != null)
                {
                    // 항상 총구(무기 피벗)가 실제로 가리키는 방향으로 발사
                    // 무기 회전 자체가 이미 크로스헤어를 조준하고 있으므로 가장 자연스럽다
                    Vector2 aimDir = pivot.right;
                    currentWeapon.Fire(aimDir);
                }
            }

            // Right click for stealth / dodge
            if (Mouse.current.rightButton.wasPressedThisFrame && stealth != null)
            {
                PlayerLogManager.Instance?.RecordAction();
                stealth.ActivateStealth(moveInput.normalized);
            }
        }
    }
}
