using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using Core;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ElevatorStation : MonoBehaviour, IInteractable
    {
        public string GetInteractPrompt()
        {
            return "게임 진입 (Elevator)";
        }

        public void OnInteract(GameObject interactEntity)
        {
            Debug.Log("[Elevator] 본 게임으로 진입합니다!");
            
            if (RunManager.Instance != null)
            {
                RunManager.Instance.StartNewRun();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.LoadScene("SampleScene");
            }
            else
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }
}
