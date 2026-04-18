using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public Sprite weaponSprite;
        public GameObject projectilePrefab; // [추가] 무기별 전용 탄환 프리팹 (필요시)
        public enum WeaponType { Pistol, AssaultRifle, Sniper, Shotgun }

        public WeaponType Type;
        public string WeaponName;
        [Tooltip("탄환에 적용할 피해량 계수")]
        public float damageMultiplier = 1f;
        [Tooltip("Shots per second")]
        public float FireRate = 1f;
        public float ProjectileSpeed = 20f;
        public float BulletDeceleration = 1.0f;
        public bool IsAuto = false;
        
        [Header("Shotgun Only")]
        public int NumberOfPellets = 1;
        public float SpreadAngle = 0f; // in degrees
        
        [Header("Sniper Only")]
        public int PierceCount = 0; // 0 means destroys on first hit
    }
}
