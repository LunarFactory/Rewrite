using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Auth;

namespace UI
{
    public class LoginController : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject loginPanel;   // RightContainer/LoginPanel
        public GameObject signupPanel;  // RightContainer/SignupPanel

        [Header("Login Fields")]
        public InputField loginIdInput;
        public InputField loginPwInput;
        public Button loginBtn;
        public Button toSignupBtn;
        public Button toRecoverBtn;

        [Header("General")]
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
            SetupButtonListeners();
            ShowLoginPanel();
        }

        private void SetupButtonListeners()
        {
            if (loginBtn != null) 
            {
                loginBtn.onClick.RemoveAllListeners();
                loginBtn.onClick.AddListener(OnLoginClicked);
            }

            if (toSignupBtn != null)
                toSignupBtn.onClick.AddListener(ShowSignupPanel);

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
            if (string.IsNullOrEmpty(loginIdInput?.text) || string.IsNullOrEmpty(loginPwInput?.text))
            {
                SetStatus("ID와 Password를 입력하세요.", Color.red);
                return;
            }

            SetStatus("로그인 중...", Color.blue);
            StartCoroutine(PerformLoginRoutine());
        }

        private IEnumerator PerformLoginRoutine()
        {
            var loginTask = AuthManager.Instance.Login(loginIdInput.text, loginPwInput.text);
            while (!loginTask.IsCompleted) yield return null;

            var result = loginTask.Result;
            if (result.Success)
            {
                SetStatus("로그인 성공!", Color.green);
                yield return new WaitForSeconds(0.5f);
                SceneManager.LoadScene("LobbyScene");
            }
            else
            {
                SetStatus($"로그인 실패: {result.Message}", Color.red);
            }
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
                if (signupStatusText != null) { signupStatusText.text = "ID와 Password를 입력하세요."; signupStatusText.color = Color.red; }
                return;
            }

            if (pw != pwConfirm)
            {
                if (signupStatusText != null) { signupStatusText.text = "비밀번호가 일치하지 않습니다."; signupStatusText.color = Color.red; }
                return;
            }

            if (signupStatusText    != null) { signupStatusText.text = "회원가입 처리 중..."; signupStatusText.color = Color.blue; }
            if (submitSignupButton  != null) submitSignupButton.interactable = false;

            StartCoroutine(MockSignupRoutine());
        }

        private IEnumerator MockSignupRoutine()
        {
            yield return new WaitForSeconds(1.0f);

            if (signupStatusText != null) { signupStatusText.text = "회원가입 성공! 로그인 화면으로 이동합니다."; signupStatusText.color = Color.green; }
            Debug.Log($"Signup successful for ID: {newIdInput?.text}");

            yield return new WaitForSeconds(1.5f);

            if (submitSignupButton != null) submitSignupButton.interactable = true;
            ShowLoginPanel();
        }

        private void SetStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
    }
}
