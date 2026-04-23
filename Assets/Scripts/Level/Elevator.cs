using Core;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Elevator : InteractableBase
    {
        public override string GetInteractPrompt()
        {
            return "게임 진입";
        }

        public override void OnInteract(GameObject interactEntity)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.LoadScene("GameScene");
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
            if (RunManager.Instance != null)
            {
                RunManager.Instance.StartNewRun();
            }
        }
    }
}
