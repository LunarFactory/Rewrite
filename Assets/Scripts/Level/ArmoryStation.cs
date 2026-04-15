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

        public WeaponData[] weaponLibrary;

        public void OnInteract(GameObject interactEntity)
        {
            var player = interactEntity.GetComponent<Player.PlayerController>();
            if (player == null) return;

            int currentIndex = System.Array.IndexOf(weaponLibrary, player.GetCurrentWeapon());
            int nextIndex = (currentIndex + 1) % weaponLibrary.Length;
            WeaponData selectedData = weaponLibrary[nextIndex];

            // 플레이어에게 데이터 주입 및 매니저에 저장
            player.SetCurrentWeapon(selectedData);
            if (Core.RunManager.Instance != null)
            {
                Core.RunManager.Instance.SetWeapon(selectedData);
            }
        }
    }
}
