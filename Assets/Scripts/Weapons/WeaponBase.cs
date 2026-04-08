using UnityEngine;
using Log;

namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected WeaponData weaponData;
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform firePoint;

        protected float nextFireTime;

        public virtual void Fire(Vector2 direction)
        {
            if (Time.time < nextFireTime) return;

            nextFireTime = Time.time + (1f / weaponData.FireRate);
            
            PlayerLogManager.Instance?.RecordShotFired();

            SpawnProjectile(direction);
        }

        protected virtual void SpawnProjectile(Vector2 direction)
        {
            if (projectilePrefab == null || firePoint == null) return;

            GameObject obj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            obj.SetActive(true);
            if (obj.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(direction, weaponData.ProjectileSpeed, weaponData.Damage, weaponData.PierceCount);
            }
        }
    }
}
