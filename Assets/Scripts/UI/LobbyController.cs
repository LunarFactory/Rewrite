using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LobbyController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;      // 기본 로비 버튼 묶음
        public GameObject statsPanel;     // 통계 패널
        public GameObject settingsPanel;  // 설정 패널

        [Header("Main Buttons")]
        public Text welcomeText;
        public Button startButton;
        public Button statsButton;
        public Button settingsButton;
        public Button quitButton;

        [Header("Controllers")]
        public StatsController    statsController;
        public SettingsController settingsController;

        private void Start()
        {
            string userId = PlayerPrefs.GetString("UserId", "Guest");
            if (welcomeText != null)
                welcomeText.text = $"Welcome, {userId}!";

            if (startButton    != null) startButton.onClick.AddListener(OnStartClicked);
            if (statsButton    != null) statsButton.onClick.AddListener(OnStatsClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (quitButton     != null) quitButton.onClick.AddListener(OnQuitClicked);

            // 뒤로가기 버튼 연결 (각 패널의 BackBtn)
            WireBackButtons();

            ShowMainPanel();
        }

        // ─────────────────────────────────────────
        //  패널 전환
        // ─────────────────────────────────────────

        private void ShowMainPanel()
        {
            if (mainPanel     != null) mainPanel.SetActive(true);
            if (statsPanel    != null) statsPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OnStartClicked()
        {
            Debug.Log("Game Start clicked.");
            if (UIManager.Instance != null)
                UIManager.Instance.LoadScene("SampleScene");
            else
                SceneManager.LoadScene("SampleScene");
        }

        private void OnStatsClicked()
        {
            if (mainPanel  != null) mainPanel.SetActive(false);
            if (statsPanel != null)
            {
                if (statsController != null)
                    statsController.Show();
                else
                    statsPanel.SetActive(true);
            }
        }

        private void OnSettingsClicked()
        {
            if (mainPanel     != null) mainPanel.SetActive(false);
            if (settingsPanel != null)
            {
                if (settingsController != null)
                    settingsController.Show();
                else
                    settingsPanel.SetActive(true);
            }
        }

        private void OnQuitClicked()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.QuitGame();
            else
                Application.Quit();
        }

        // ─────────────────────────────────────────
        //  뒤로가기 연결
        // ─────────────────────────────────────────

        private void WireBackButtons()
        {
            // StatsPanel 뒤로가기
            if (statsPanel != null)
            {
                var backBtn = statsPanel.transform.Find("StatsBackBtn")?.GetComponent<Button>();
                if (backBtn != null)
                    backBtn.onClick.AddListener(ShowMainPanel);

                // StatsController의 backButton도 연결
                if (statsController != null && statsController.backButton == null)
                    statsController.backButton = backBtn;
            }

            // SettingsPanel 뒤로가기는 SettingsController.SaveAndClose()가 Hide()를 호출하므로
            // Hide() 뒤에 MainPanel을 보여줘야 함 → 별도 버튼 연결
            if (settingsPanel != null)
            {
                var backBtn = settingsPanel.transform.Find("SettingsBackBtn")?.GetComponent<Button>();
                if (backBtn != null)
                {
                    // 기존 SettingsController 리스너 위에 mainPanel 활성화 추가
                    backBtn.onClick.AddListener(() =>
                    {
                        if (mainPanel != null) mainPanel.SetActive(true);
                    });
                }
            }
        }
    }
}
