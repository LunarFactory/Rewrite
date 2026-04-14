using UnityEngine;
using Weapons;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ArmoryStation : MonoBehaviour, IInteractable
    {
        public string GetInteractPrompt()
        {
            return "무기 변경 (Armory)";
        }

        public void OnInteract(GameObject interactEntity)
        {
            var ctrl = interactEntity.GetComponent<Player.PlayerController>();
            if (ctrl == null) return;

            WeaponBase currentWeapon = ctrl.currentWeapon;
            if (currentWeapon != null && currentWeapon.weaponData != null)
            {
                GameObject weaponObj = currentWeapon.gameObject;
                WeaponData oldData = currentWeapon.weaponData;
                GameObject proj = currentWeapon.projectilePrefab;
                Transform fp = currentWeapon.firePoint;

                int typeInt = (int)oldData.Type;
                typeInt = (typeInt + 1) % 4; // 0,1,2,3 -> Pistol, AssaultRifle, Sniper, Shotgun
                
                // We create a fresh WeaponData to avoid persisting changes to the asset if any
                WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                data.Type = (WeaponData.WeaponType)typeInt;
                
                int spriteIndex = 0;
                WeaponBase newComp = null;

                // Destroy old component
                DestroyImmediate(currentWeapon);

                switch (data.Type)
                {
                    case WeaponData.WeaponType.Pistol:
                        data.WeaponName = "Pistol";
                        data.Damage = 10f;
                        data.FireRate = 5f;
                        data.ProjectileSpeed = 20f;
                        data.IsAuto = true;
                        data.NumberOfPellets = 1;
                        data.SpreadAngle = 0f;
                        data.PierceCount = 0;
                        newComp = weaponObj.AddComponent<WeaponPistol>();
                        spriteIndex = 0;
                        break;
                    case WeaponData.WeaponType.AssaultRifle:
                        data.WeaponName = "SMG";
                        data.Damage = 5f;
                        data.FireRate = 12f;
                        data.ProjectileSpeed = 25f;
                        data.IsAuto = true;
                        data.NumberOfPellets = 1;
                        data.SpreadAngle = 5f;
                        data.PierceCount = 0;
                        newComp = weaponObj.AddComponent<WeaponAssaultRifle>();
                        spriteIndex = 1; // Assuming SMG is index 1
                        break;
                    case WeaponData.WeaponType.Sniper:
                        data.WeaponName = "Sniper";
                        data.Damage = 40f;
                        data.FireRate = 1f;
                        data.ProjectileSpeed = 40f;
                        data.IsAuto = false;
                        data.NumberOfPellets = 1;
                        data.SpreadAngle = 0f;
                        data.PierceCount = 3;
                        newComp = weaponObj.AddComponent<WeaponSniper>();
                        spriteIndex = 2; // Assuming Sniper is index 2
                        break;
                    case WeaponData.WeaponType.Shotgun:
                        data.WeaponName = "Shotgun";
                        data.Damage = 8f; 
                        data.FireRate = 1.5f;
                        data.ProjectileSpeed = 30f;
                        data.IsAuto = false;
                        data.NumberOfPellets = 5;
                        data.SpreadAngle = 30f;
                        data.PierceCount = 0;
                        newComp = weaponObj.AddComponent<WeaponShotgun>();
                        spriteIndex = 3; // Assuming Shotgun is 3
                        break;
                }

                // Inject restored elements
                newComp.weaponData = data;
                newComp.projectilePrefab = proj;
                newComp.firePoint = fp;

                // Visually update the sprite
                var sr = weaponObj.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
                if (sr != null)
                {
                    string wPath = "Assets/Sprites/weapon/weapons.png";
                    var allWeapons = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(wPath);
                    foreach (var asset in allWeapons) {
                        if (asset is Sprite s && s.name == "weapons_" + spriteIndex) {
                            sr.sprite = s;
                            break;
                        }
                    }
                }
#endif

                // Relink to PlayerController
                if (ctrl != null)
                {
                    ctrl.currentWeapon = newComp;
                }

                if (Core.RunManager.Instance != null)
                {
                    Core.RunManager.Instance.CurrentWeaponType = data.Type;
                }

                Debug.Log($"[Armory] 무기가 {data.WeaponName}(으)로 성공적으로 교체 및 장착되었습니다!");
            }
            else
            {
                Debug.LogError($"[Armory] 무기 교체 실패! currentWeapon is null? {currentWeapon == null}");
                if (currentWeapon != null)
                {
                    Debug.LogError($"[Armory] weaponData is null? {currentWeapon.weaponData == null}");
                }
            }
        }
    }
}
