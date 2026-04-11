using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LoginController : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject loginPanel;   // RightContainer/LoginPanel
        public GameObject signupPanel;  // RightContainer/SignupPanel

        [Header("Login UI Elements")]
        public InputField idInput;
        public InputField passwordInput;
        public Button loginButton;
        public Button signupButton;     // 회원가입 창으로 전환
        public Text statusText;

        [Header("Signup UI Elements")]
        public InputField newIdInput;
        public InputField newPasswordInput;
        public InputField confirmPasswordInput; // 비밀번호 확인
        public Button submitSignupButton;       // 가입 완료
        public Button cancelSignupButton;       // 로그인으로 돌아가기
        public Text signupStatusText;

        private void Start()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);

            if (signupButton != null)
                signupButton.onClick.AddListener(ShowSignupPanel);

            if (submitSignupButton != null)
                submitSignupButton.onClick.AddListener(OnSubmitSignupClicked);

            if (cancelSignupButton != null)
                cancelSignupButton.onClick.AddListener(ShowLoginPanel);

            // 비밀번호 확인 필드 자동 연결 (씬에서 못 찾으면 직접 탐색)
            if (confirmPasswordInput == null)
            {
                var signupPanelTF = signupPanel != null ? signupPanel.transform : null;
                if (signupPanelTF != null)
                {
                    var tf = signupPanelTF.Find("비밀번호확인Field");
                    if (tf != null) confirmPasswordInput = tf.GetComponent<InputField>();
                }
            }

            ShowLoginPanel();
        }

        // ─────────────────────────────────────────
        //  Panel 전환
        // ─────────────────────────────────────────

        private void ShowLoginPanel()
        {
            if (loginPanel  != null) loginPanel.SetActive(true);
            if (signupPanel != null) signupPanel.SetActive(false);
            if (statusText  != null) statusText.text = "";
        }

        private void ShowSignupPanel()
        {
            if (loginPanel  != null) loginPanel.SetActive(false);
            if (signupPanel != null) signupPanel.SetActive(true);
            if (signupStatusText != null) signupStatusText.text = "";

            // 입력 필드 초기화
            if (newIdInput           != null) newIdInput.text = "";
            if (newPasswordInput     != null) newPasswordInput.text = "";
            if (confirmPasswordInput != null) confirmPasswordInput.text = "";
        }

        // ─────────────────────────────────────────
        //  로그인
        // ─────────────────────────────────────────

        private void OnLoginClicked()
        {
            if (string.IsNullOrEmpty(idInput?.text) || string.IsNullOrEmpty(passwordInput?.text))
            {
                if (statusText != null) statusText.text = "ID와 Password를 입력하세요.";
                return;
            }

            if (statusText  != null) statusText.text = "로그인 중...";
            if (loginButton != null) loginButton.interactable = false;

            StartCoroutine(MockLoginRoutine());
        }

        private IEnumerator MockLoginRoutine()
        {
            yield return new WaitForSeconds(1.0f);

            if (statusText != null) statusText.text = "로그인 성공!";
            Debug.Log($"Logged in with ID: {idInput.text}");

            PlayerPrefs.SetString("SessionToken", "mock_token_12345");
            PlayerPrefs.SetString("UserId", idInput.text);
            PlayerPrefs.Save();

            if (UIManager.Instance != null)
                UIManager.Instance.LoadScene("LobbyScene");
            else
                SceneManager.LoadScene("LobbyScene");
        }

        // ─────────────────────────────────────────
        //  회원가입
        // ─────────────────────────────────────────

        private void OnSubmitSignupClicked()
        {
            string id       = newIdInput?.text ?? "";
            string pw       = newPasswordInput?.text ?? "";
            string pwConfirm= confirmPasswordInput?.text ?? "";

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
            {
                if (signupStatusText != null) signupStatusText.text = "ID와 Password를 입력하세요.";
                return;
            }

            if (pw != pwConfirm)
            {
                if (signupStatusText != null) signupStatusText.text = "비밀번호가 일치하지 않습니다.";
                return;
            }

            if (signupStatusText    != null) signupStatusText.text = "회원가입 처리 중...";
            if (submitSignupButton  != null) submitSignupButton.interactable = false;

            StartCoroutine(MockSignupRoutine());
        }

        private IEnumerator MockSignupRoutine()
        {
            yield return new WaitForSeconds(1.0f);

            if (signupStatusText != null) signupStatusText.text = "회원가입 성공! 로그인 화면으로 이동합니다.";
            Debug.Log($"Signup successful for ID: {newIdInput.text}");

            yield return new WaitForSeconds(1.5f);

            if (submitSignupButton != null) submitSignupButton.interactable = true;
            ShowLoginPanel();
        }
    }
}
