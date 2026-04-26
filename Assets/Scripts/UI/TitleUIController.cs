using System.Collections;
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
        public Button settingsButton;
        public Button quitButton;
        public Button logoutButton;

        [Header("Main Menu")]
        public GameObject mainMenuPanel; // 메인 버튼들이 들어있는 부모 컨테이너

        [Header("UI Panels")]
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

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (settingsPanel != null && settingsPanel.backButton != null)
                settingsPanel.backButton.onClick.AddListener(ShowMainMenu);
            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);
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

        private async void OnLogoutClicked()
        {
            // 버튼 비활성화 (중복 클릭 방지)
            logoutButton.interactable = false;

            // Manager에게 로그아웃을 시킵니다.
            var result = await Auth.AuthManager.Instance.Logout();

            if (result.Success)
            {
                Debug.Log("로그아웃 성공: " + result.Message);
            }

            // 결과와 상관없이 로그인 씬으로 이동
            SceneManager.LoadScene("LoginScene");
        }
    }
}
