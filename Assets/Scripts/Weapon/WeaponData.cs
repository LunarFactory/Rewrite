using UnityEngine;

namespace Weapon
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public Sprite weaponSprite;
        public GameObject projectilePrefab; // [추가] 무기별 전용 탄환 프리팹 (필요시)

        public enum WeaponType
        {
            Pistol,
            AssaultRifle,
            Sniper,
            Shotgun,
        }

        public WeaponType Type;
        public string weaponName;

        [Tooltip("탄환에 적용할 피해량 계수")]
        public float damageMultiplier = 1f;

        [Tooltip("Shots per second")]
        public float fireRate = 1f;
        public float projectileSpeed = 20f;
        public float decelerationRate = 1.0f;

        [Header("Shotgun Only")]
        public int numberOfPellets = 1;
        public float spreadAngle = 0f; // in degrees

        [Header("Sniper Only")]
        public int pierceCount = 0; // 0 means destroys on first hit
    }
}
