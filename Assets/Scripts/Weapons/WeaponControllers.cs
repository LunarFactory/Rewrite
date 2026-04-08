using UnityEngine;

namespace Weapons
{
    public class WeaponPistol : WeaponBase
    {
        // Default behavior from WeaponBase
    }

    public class WeaponAssaultRifle : WeaponBase
    {
        // Inherits fast fire rate from WeaponData
        // Relies on PlayerController checking 'Hold' if IsAuto is true
    }

    public class WeaponSniper : WeaponBase
    {
        // Inherits Piercing from WeaponData
    }

    public class WeaponShotgun : WeaponBase
    {
        public override void Fire(Vector2 direction)
        {
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + (1f / weaponData.FireRate);
            
            Log.PlayerLogManager.Instance?.RecordShotFired();

            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int pellets = weaponData.NumberOfPellets;
            float spread = weaponData.SpreadAngle;

            float startAngle = baseAngle - (spread / 2f);
            float angleStep = pellets > 1 ? spread / (pellets - 1) : 0;

            for (int i = 0; i < pellets; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 spreadDir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                
                SpawnProjectile(spreadDir);
            }
        }
    }
}
