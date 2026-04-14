using UnityEngine;

namespace Level
{
    public interface IInteractable
    {
        string GetInteractPrompt();
        void OnInteract(GameObject interactEntity);
    }
}
