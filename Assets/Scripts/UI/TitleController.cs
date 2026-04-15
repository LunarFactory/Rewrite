using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI
{
    public class TitleController : MonoBehaviour
    {
        public Text welcomeText;
        public Button startButton;
        public Button supportPortButton;
        public Button settingsButton;
        public Button quitButton;

        private void Start()
        {
            string userId = PlayerPrefs.GetString("UserId", "Guest");
            if (welcomeText != null)
            {
                welcomeText.text = $"Welcome, {userId}!";
            }

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            
            if (supportPortButton != null)
                supportPortButton.onClick.AddListener(OnSupportPortClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnStartClicked()
        {
            Debug.Log("Game Start clicked.");
            // For now, load SampleScene where TestSetup resides, or GameScene
            // Looking at the active scene context, we could explicitly load the existing scene.
            // But let's build logic assuming "SampleScene" or whichever your test scene is called.
            // Wait, we can specify the name via Unity Build Settings. 
            // In the workspace, TestSetup is attached to "SampleScene" initially. Let's call it "SampleScene".
            if (UIManager.Instance != null)
            {
                UIManager.Instance.LoadScene("LobbyScene");
            }
            else
            {
                SceneManager.LoadScene("LobbyScene");
            }
        }

        private void OnSupportPortClicked()
        {
            Debug.Log("보급 포트 (Support Port) 진입. (준비중)");
            // Typically show a modal or change view to upgrades
        }

        private void OnSettingsClicked()
        {
            Debug.Log("환경설정 (Settings) 오픈. (준비중)");
            // Show settings modal
        }

        private void OnQuitClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
