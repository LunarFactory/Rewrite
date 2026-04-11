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
        public GameObject recoverPanel; // RightContainer/RecoverPanel (새로 추가)

        [Header("Login UI Elements")]
        public InputField loginIdInput;
        public InputField loginPwInput;
        public Button loginBtn;
        public Button toSignupBtn;
        public Button toRecoverBtn;    // 계정 찾기 창으로 전환 (새로 추가)
        public Text statusText;

        [Header("Signup UI Elements")]
        public InputField newIdInput;
        public InputField newPasswordInput;
        public InputField newNicknameInput;     // 닉네임 필드 (master 스타일)
        public Button submitSignupButton;       // 가입 완료
        public Button cancelSignupButton;       // 로그인으로 돌아가기
        public Text signupStatusText;

        [Header("Recover UI Elements")]
        public InputField recoverNicknameInput;
        public Button submitRecoverButton;
        public Button cancelRecoverButton;
        public Text recoverStatusText;

        private void Awake()
        {
            // AuthManager가 씬에 없으면 자동 생성
            if (AuthManager.Instance == null)
            {
                GameObject authObj = new GameObject("AuthManager");
                authObj.AddComponent<AuthManager>();
            }
        }

        private void Start()
        {
            SetupButtonListeners();
            
            // 초기 상태 강제 설정 (로그인만 활성화, 나머지는 비활성화)
            if (loginPanel   != null) loginPanel.SetActive(true);
            if (signupPanel  != null) signupPanel.SetActive(false);
            if (recoverPanel != null) recoverPanel.SetActive(false);

            if (statusText   != null) statusText.text = "";
        }

        private void SetupButtonListeners()
        {
            // Panel Auto-Binding
            if (loginPanel == null) { var go = GameObject.Find("Canvas/RightContainer/LoginPanel"); if (go != null) loginPanel = go; }
            if (signupPanel == null) { var go = GameObject.Find("Canvas/RightContainer/SignupPanel"); if (go != null) signupPanel = go; }
            if (recoverPanel == null) { var go = GameObject.Find("Canvas/RightContainer/RecoverPanel"); if (go != null) recoverPanel = go; }

            // Login Panel Auto-Binding
            if (loginPanel != null)
            {
                if (loginIdInput == null) { var tf = loginPanel.transform.Find("IDField"); if (tf != null) loginIdInput = tf.GetComponentInChildren<InputField>(); }
                if (loginPwInput == null) { var tf = loginPanel.transform.Find("PassField"); if (tf != null) loginPwInput = tf.GetComponentInChildren<InputField>(); }
                if (loginBtn == null) { var tf = loginPanel.transform.Find("LoginBtn"); if (tf != null) loginBtn = tf.GetComponent<Button>(); }
                if (toSignupBtn == null) { var tf = loginPanel.transform.Find("SignupBtn"); if (tf != null) toSignupBtn = tf.GetComponent<Button>(); }
                if (toRecoverBtn == null) { var tf = loginPanel.transform.Find("RecoverBtn"); if (tf != null) toRecoverBtn = tf.GetComponent<Button>(); }
                if (statusText == null) { var tf = loginPanel.transform.Find("StatusText"); if (tf != null) statusText = tf.GetComponent<Text>(); }
            }

            // Signup Panel Auto-Binding
            if (signupPanel != null)
            {
                if (newIdInput == null) { var tf = signupPanel.transform.Find("아이디Field"); if (tf != null) newIdInput = tf.GetComponentInChildren<InputField>(); }
                if (newPasswordInput == null) { var tf = signupPanel.transform.Find("비밀번호Field"); if (tf != null) newPasswordInput = tf.GetComponentInChildren<InputField>(); }
                if (newNicknameInput == null) { var tf = signupPanel.transform.Find("닉네임Field"); if (tf != null) newNicknameInput = tf.GetComponentInChildren<InputField>(); }
                if (submitSignupButton == null) { var tf = signupPanel.transform.Find("SubmitSignupBtn"); if (tf != null) submitSignupButton = tf.GetComponent<Button>(); }
                if (cancelSignupButton == null) { var tf = signupPanel.transform.Find("CancelSignupBtn"); if (tf != null) cancelSignupButton = tf.GetComponent<Button>(); }
                if (signupStatusText == null) { var tf = signupPanel.transform.Find("SignupStatusText"); if (tf != null) signupStatusText = tf.GetComponent<Text>(); }
            }

            // Recover Panel Auto-Binding
            if (recoverPanel != null)
            {
                if (recoverNicknameInput == null) { var tf = recoverPanel.transform.Find("닉네임Field"); if (tf != null) recoverNicknameInput = tf.GetComponentInChildren<InputField>(); }
                if (submitRecoverButton == null) { var tf = recoverPanel.transform.Find("SubmitRecoverBtn"); if (tf != null) submitRecoverButton = tf.GetComponent<Button>(); }
                if (cancelRecoverButton == null) { var tf = recoverPanel.transform.Find("CancelRecoverBtn"); if (tf != null) cancelRecoverButton = tf.GetComponent<Button>(); }
                if (recoverStatusText == null) { var tf = recoverPanel.transform.Find("RecoverStatusText"); if (tf != null) recoverStatusText = tf.GetComponent<Text>(); }

                if (submitRecoverButton != null) submitRecoverButton.onClick.AddListener(OnSubmitRecoverClicked);
                if (cancelRecoverButton != null) cancelRecoverButton.onClick.AddListener(ShowLoginPanel);
            }

            if (loginBtn != null) 
            {
                loginBtn.onClick.RemoveAllListeners();
                loginBtn.onClick.AddListener(OnLoginClicked);
            }

            if (toSignupBtn != null)
                toSignupBtn.onClick.AddListener(ShowSignupPanel);

            if (toRecoverBtn != null)
                toRecoverBtn.onClick.AddListener(ShowRecoverPanel);

            if (submitSignupButton != null)
                submitSignupButton.onClick.AddListener(OnSubmitSignupClicked);

            if (cancelSignupButton != null)
                cancelSignupButton.onClick.AddListener(ShowLoginPanel);
        }

        // ─────────────────────────────────────────
        //  Panel 전환
        // ─────────────────────────────────────────

        private void ShowLoginPanel()
        {
            if (loginPanel   != null) loginPanel.SetActive(true);
            if (signupPanel  != null) signupPanel.SetActive(false);
            if (recoverPanel != null) recoverPanel.SetActive(false);
            if (statusText   != null) statusText.text = "";
        }

        private void ShowSignupPanel()
        {
            if (loginPanel   != null) loginPanel.SetActive(false);
            if (signupPanel  != null) signupPanel.SetActive(true);
            if (recoverPanel != null) recoverPanel.SetActive(false);
            if (signupStatusText != null) signupStatusText.text = "";

            // 입력 필드 초기화
            if (newIdInput           != null) newIdInput.text = "";
            if (newPasswordInput     != null) newPasswordInput.text = "";
            if (newNicknameInput     != null) newNicknameInput.text = "";
        }

        private void ShowRecoverPanel()
        {
            if (loginPanel   != null) loginPanel.SetActive(false);
            if (signupPanel  != null) signupPanel.SetActive(false);
            if (recoverPanel != null) recoverPanel.SetActive(true);
            if (recoverStatusText != null) recoverStatusText.text = "";

            if (recoverNicknameInput != null) recoverNicknameInput.text = "";
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
            string nick     = newNicknameInput?.text ?? "";

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(nick))
            {
                if (signupStatusText != null) { signupStatusText.text = "모든 정보를 입력하세요."; signupStatusText.color = Color.red; }
                return;
            }

            if (signupStatusText    != null) { signupStatusText.text = "회원가입 처리 중..."; signupStatusText.color = Color.blue; }
            if (submitSignupButton  != null) submitSignupButton.interactable = false;

            StartCoroutine(PerformSignupRoutine(id, pw, nick));
        }

        private IEnumerator PerformSignupRoutine(string id, string pw, string nick)
        {
            // Master 브랜치 로직대로 AuthManager를 통한 실제 가입 처리
            var signupTask = AuthManager.Instance.Signup(id, pw, nick);
            while (!signupTask.IsCompleted) yield return null;

            var result = signupTask.Result;
            if (result.Success)
            {
                if (signupStatusText != null) { signupStatusText.text = "회원가입 성공! 초기 화면으로 돌아갑니다."; signupStatusText.color = Color.green; }
                yield return new WaitForSeconds(1.5f);
                
                // Master 브랜치 스타일: 가입 성공 시 씬 자체를 리로드하여 깔끔하게 초기화
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                if (signupStatusText != null) { signupStatusText.text = $"회원가입 실패: {result.Message}"; signupStatusText.color = Color.red; }
                if (submitSignupButton != null) submitSignupButton.interactable = true;
            }
        }

        private void SetStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
        // ─────────────────────────────────────────
        //  계정 찾기 (Recover)
        // ─────────────────────────────────────────

        private void OnSubmitRecoverClicked()
        {
            string nick = recoverNicknameInput?.text ?? "";
            if (string.IsNullOrEmpty(nick))
            {
                if (recoverStatusText != null) { recoverStatusText.text = "닉네임을 입력하세요."; recoverStatusText.color = Color.red; }
                return;
            }

            if (recoverStatusText   != null) { recoverStatusText.text = "정보 확인 중..."; recoverStatusText.color = Color.blue; }
            if (submitRecoverButton != null) submitRecoverButton.interactable = false;

            StartCoroutine(PerformRecoverRoutine(nick));
        }

        private IEnumerator PerformRecoverRoutine(string nick)
        {
            var task = AuthManager.Instance.RecoverAccount(nick);
            while (!task.IsCompleted) yield return null;

            var result = task.Result;
            if (result.Success)
            {
                // Bypass mode에서는 Message에 ID/PW 정보가 포함됨
                if (recoverStatusText != null) { recoverStatusText.text = result.Message; recoverStatusText.color = Color.green; }
            }
            else
            {
                if (recoverStatusText != null) { recoverStatusText.text = $"확인 실패: {result.Message}"; recoverStatusText.color = Color.red; }
            }

            if (submitRecoverButton != null) submitRecoverButton.interactable = true;
        }

    }
}
