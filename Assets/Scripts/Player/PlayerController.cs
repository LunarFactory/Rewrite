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
            if (weaponPivot == null) return;
            
            if (Camera.main != null && Mouse.current != null)
            {
                mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector2 lookDir = mousePos - weaponPivot.position;
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void HandleActions()
        {
            if (Mouse.current == null) return;

            if (isAttacking)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    PlayerLogManager.Instance?.RecordAction();
                }
                
                if (currentWeapon != null && weaponPivot != null)
                {
                    Vector2 aimDir = (mousePos - weaponPivot.position).normalized;
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
