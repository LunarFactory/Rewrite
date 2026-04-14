using UnityEngine;
using Log;

namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] public WeaponData weaponData;
        [SerializeField] public GameObject projectilePrefab;
        [SerializeField] public Transform firePoint;
        public Transform FirePoint => firePoint;

        [Header("Allegiance")]
        [Tooltip("True if this weapon is wielded by the player.")]
        public bool isPlayerWeapon = true;

        protected float nextFireTime;

        public virtual void Fire(Vector2 direction)
        {
            if (Time.time < nextFireTime) return;

            nextFireTime = Time.time + (1f / weaponData.FireRate);
            
            if (isPlayerWeapon)
            {
                PlayerLogManager.Instance?.RecordShotFired();
                // 사격 시 스텔스 해제
                var stealth = GetComponentInParent<Player.PlayerStealth>();
                stealth?.CancelStealth();
            }

            SpawnProjectile(direction);
        }

        protected virtual void SpawnProjectile(Vector2 direction)
        {
            if (projectilePrefab == null || firePoint == null) return;

            GameObject obj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            obj.SetActive(true);
            
            // If the user forgot to add Projectile.cs to their bullet prefab, add it automatically!
            if (!obj.TryGetComponent(out Projectile proj))
            {
                proj = obj.AddComponent<Projectile>();
            }
            
            float spd = weaponData.ProjectileSpeed > 0 ? weaponData.ProjectileSpeed : 20f;
            float dmg = weaponData.Damage > 0 ? weaponData.Damage : 10f;
            proj.Initialize(direction, spd, dmg, weaponData.PierceCount, isPlayerWeapon);
        }
    }
}
