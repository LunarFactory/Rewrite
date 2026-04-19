using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TMP 사용을 위해 필수 추가
using Auth;

namespace UI
{
    public class LoginUIController : MonoBehaviour
    {
        // ─── 패널 루트 ────────────────────────────────────────────────
        [Header("Right Panels (오른쪽 교체 패널)")]
        public GameObject panelLogin;
        public GameObject panelSignup;
        public GameObject panelRecover;

        // ─── 로그인 패널 필드 ─────────────────────────────────────────
        [Header("Login Panel Fields")]
        public TMP_InputField loginIdInput; // InputField -> TMP_InputField
        public TMP_InputField loginPwInput;
        public Button loginBtn;
        public Button toSignupBtn;
        public Button toRecoverBtn;

        // ─── 회원가입 패널 필드 ───────────────────────────────────────
        [Header("Signup Panel Fields")]
        public TMP_InputField signupIdInput;
        public TMP_InputField signupPwInput;
        public TMP_InputField signupEmailInput;
        public Button signupSubmitBtn;
        public Button signupBackBtn;

        // ─── 계정 찾기 패널 필드 ──────────────────────────────────────
        [Header("Recover Panel Fields")]
        public TMP_InputField recoverEmailInput;
        public Button recoverSubmitBtn;
        public Button recoverBackBtn;

        // ─── 공통 ─────────────────────────────────────────────────────
        [Header("General")]
        public TextMeshProUGUI statusText; // Text -> TextMeshProUGUI

        // ─────────────────────────────────────────────────────────────
        private void Start()
        {
            BindButtons();
            ShowLogin();
        }

        private void SafeFixClearBackground(Button btn)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null && img.color.a < 0.05f)
            {
                img.color = new Color(0, 0, 0, 0.01f);
            }

            // [수정] 자식 중에서 TMP 텍스트를 찾아 레이캐스트 설정을 켭니다.
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.raycastTarget = true;
        }

        private void BindButtons()
        {
            // 인스펙터 연결 보정 로직 (기존 이름 유지)
            var canvas = gameObject;
            if (canvas != null)
            {
                foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (b.name == "SignupSubmitBtn") signupSubmitBtn = b;
                    if (b.name == "SignupBackBtn") signupBackBtn = b;
                    if (b.name == "RecoverSubmitBtn") recoverSubmitBtn = b;
                    if (b.name == "RecoverBackBtn") recoverBackBtn = b;
                }
            }

            CleanButton(loginBtn);
            CleanButton(toSignupBtn);
            CleanButton(toRecoverBtn);
            CleanButton(signupSubmitBtn);
            CleanButton(signupBackBtn);
            CleanButton(recoverSubmitBtn);
            CleanButton(recoverBackBtn);

            if (loginBtn != null) loginBtn.onClick.AddListener(OnLoginClicked);
            if (toSignupBtn != null) toSignupBtn.onClick.AddListener(ShowSignup);
            if (toRecoverBtn != null) toRecoverBtn.onClick.AddListener(ShowRecover);

            if (signupSubmitBtn != null) signupSubmitBtn.onClick.AddListener(OnSignupClicked);
            if (signupBackBtn != null) signupBackBtn.onClick.AddListener(ShowLogin);

            if (recoverSubmitBtn != null) recoverSubmitBtn.onClick.AddListener(OnRecoverClicked);
            if (recoverBackBtn != null) recoverBackBtn.onClick.AddListener(ShowLogin);

            Debug.Log("[LoginUIController] TMP 기반 모든 버튼 바인딩 완료!");
        }

        private void CleanButton(Button btn)
        {
            if (btn == null) return;
            btn.onClick.RemoveAllListeners();
        }

        // ─── 패널 전환 ────────────────────────────────────────────────
        private void ClearAllInputs()
        {
            if (signupIdInput != null) signupIdInput.text = "";
            if (signupPwInput != null) signupPwInput.text = "";
            if (signupEmailInput != null) signupEmailInput.text = "";
            if (recoverEmailInput != null) recoverEmailInput.text = "";
            if (loginIdInput != null) loginIdInput.text = "";
            if (loginPwInput != null) loginPwInput.text = "";
        }

        public void ShowLogin()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, true);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecover, false);
            ClearStatus();
        }

        public void ShowSignup()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, true);
            SetPanelActive(panelRecover, false);
            ClearStatus();
        }

        public void ShowRecover()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecover, true);
            ClearStatus();
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }

        // ─── 로그인 로직 ──────────────────────────────────────────────
        private void OnLoginClicked()
        {
            if (string.IsNullOrEmpty(loginIdInput?.text) || string.IsNullOrEmpty(loginPwInput?.text))
            {
                SetStatus("ID와 Password를 입력하세요.", Color.red);
                return;
            }
            SetStatus("로그인 중...", Color.blue);
            StartCoroutine(LoginRoutine());
        }

        private IEnumerator LoginRoutine()
        {
            var task = AuthManager.Instance.Login(loginIdInput.text, loginPwInput.text);
            while (!task.IsCompleted) yield return null;

            if (task.Result.Success)
            {
                SetStatus("로그인 성공!", Color.green);
                yield return new WaitForSeconds(0.5f);
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                SetStatus($"로그인 실패: {task.Result.Message}", Color.red);
            }
        }

        // ─── 회원가입 로직 ────────────────────────────────────────────
        private void OnSignupClicked()
        {
            if (string.IsNullOrEmpty(signupIdInput?.text) ||
                string.IsNullOrEmpty(signupPwInput?.text) ||
                string.IsNullOrEmpty(signupEmailInput?.text))
            {
                SetStatus("모든 정보를 입력하세요.", Color.red);
                return;
            }
            SetStatus("회원가입 중...", Color.blue);
            StartCoroutine(SignupRoutine());
        }

        private IEnumerator SignupRoutine()
        {
            var task = AuthManager.Instance.Signup(signupIdInput.text, signupPwInput.text, signupEmailInput.text);
            while (!task.IsCompleted) yield return null;

            if (task.Result.Success)
            {
                SetStatus("회원가입 성공! 로그인 화면으로 돌아갑니다.", Color.green);
                yield return new WaitForSeconds(1.5f);
                ShowLogin();
            }
            else
            {
                SetStatus($"생성 실패: {task.Result.Message}", Color.red);
            }
        }

        // ─── 계정 찾기 로직 ───────────────────────────────────────────
        private void OnRecoverClicked()
        {
            if (string.IsNullOrEmpty(recoverEmailInput?.text))
            {
                SetStatus("이메일을 입력하세요.", Color.red);
                return;
            }
            SetStatus("정보 확인 중...", Color.blue);
            StartCoroutine(RecoverRoutine());
        }

        private IEnumerator RecoverRoutine()
        {
            var task = AuthManager.Instance.RecoverAccount(recoverEmailInput.text);
            while (!task.IsCompleted) yield return null;

            if (task.Result.Success)
                SetStatus(task.Result.Message, Color.green);
            else
                SetStatus($"확인 실패: {task.Result.Message}", Color.red);
        }

        // ─── 상태 텍스트 ──────────────────────────────────────────────
        private void SetStatus(string message, Color color)
        {
            if (statusText != null)
            {
                // [안전장치] statusText가 부모 패널이 꺼질 때 같이 꺼지지 않도록 캔버스 최상단으로 이동
                if (statusText.transform.parent != transform)
                {
                    statusText.transform.SetParent(transform, true);
                }
                statusText.transform.SetAsLastSibling();
                statusText.gameObject.SetActive(true);

                statusText.text = message;
                statusText.color = color;

                // 위치 보정
                statusText.rectTransform.anchoredPosition = new Vector2(0, -250);
            }
        }

        private void ClearStatus()
        {
            if (statusText != null) statusText.text = "";
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ShowLogin();
            }
#endif
        }
    }
}