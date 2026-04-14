using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SupplyPortStation : MonoBehaviour, IInteractable
    {
        public string GetInteractPrompt()
        {
            return "보급 포트 접근 (Supply Port)";
        }

        public void OnInteract(GameObject interactEntity)
        {
            Debug.Log("[Supply Port] 보급 포트 시스템 진입. (UI 준비중)");
        }
    }
}
