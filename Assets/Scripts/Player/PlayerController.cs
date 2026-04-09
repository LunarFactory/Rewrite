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
        [SerializeField] private Weapons.WeaponBase currentWeapon;

        [Header("Combat")]
        private bool isAttacking;

        private PlayerStealth stealth;

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
            Vector2 lookDir = (Vector2)(mousePos - pivot.position);
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            pivot.rotation = Quaternion.Euler(0, 0, angle);
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
                    Vector2 aimDir = ((Vector2)(mousePos - pivot.position)).normalized;
                    if (aimDir == Vector2.zero) aimDir = Vector2.right;
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
