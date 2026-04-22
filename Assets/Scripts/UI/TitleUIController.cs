using TMPro; // TMP 사용을 위해 필수 추가
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class TitleController : MonoBehaviour
    {
        public TextMeshProUGUI welcomeText;
        public Button startButton;
        public Button statsButton; // 통계 버튼
        public Button settingsButton;
        public Button quitButton;

        [Header("Main Menu")]
        public GameObject mainMenuPanel; // 메인 버튼들이 들어있는 부모 컨테이너

        [Header("UI Panels")]
        public StatsUIController statsPanel;
        public SettingsUIController settingsPanel;

        private void Start()
        {
            string userId = PlayerPrefs.GetString("UserId", "Guest");
            if (welcomeText != null)
            {
                welcomeText.text = $"Welcome, {userId}!";
            }

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (statsButton != null)
                statsButton.onClick.AddListener(OnStatsClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // 각 패널의 Back 버튼을 눌렀을 때 메인 메뉴를 다시 켜도록 연결
            if (statsPanel != null && statsPanel.backButton != null)
                statsPanel.backButton.onClick.AddListener(ShowMainMenu);

            if (settingsPanel != null && settingsPanel.backButton != null)
                settingsPanel.backButton.onClick.AddListener(ShowMainMenu);
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        private void OnStartClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.LoadScene("LobbyScene");
            }
            else
            {
                SceneManager.LoadScene("LobbyScene");
            }
        }

        private void OnStatsClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false); // 메인 메뉴 숨기기

            if (statsPanel != null)
                statsPanel.Show();
        }

        private void OnSettingsClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false); // 메인 메뉴 숨기기

            if (settingsPanel != null)
                settingsPanel.Show();
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
