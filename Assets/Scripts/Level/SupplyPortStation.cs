using UnityEngine;
using UI; // SupplyPortUI를 찾기 위해 추가

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SupplyPortStation : InteractableBase
    {
        public override string GetInteractPrompt()
        {
            return "보급 포트 접근";
        }

        public override void OnInteract(GameObject interactEntity)
        {
            if (UpgradeUIController.Instance != null)
            {
                UpgradeUIController.Instance.Open();
            }
            else
            {
                Debug.LogError("씬에 SupplyPortUI가 없습니다!");
            }
        }
    }

}