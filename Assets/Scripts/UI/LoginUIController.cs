using System.Collections;
using Auth;
using TMPro; // TMP 사용을 위해 필수 추가
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LoginUIController : MonoBehaviour
    {
        // ─── 패널 루트 ────────────────────────────────────────────────
        [Header("Right Panels (오른쪽 교체 패널)")]
        public GameObject panelLogin;
        public GameObject panelSignup;
        public GameObject panelRecoverID;
        public GameObject panelRecoverPassword;

        // ─── 로그인 패널 필드 ─────────────────────────────────────────
        [Header("Login Panel Fields")]
        public TMP_InputField loginIdInput; // InputField -> TMP_InputField
        public TMP_InputField loginPwInput;
        public Button loginBtn;
        public Button toSignupBtn;
        public Button toRecoverIDBtn;
        public Button toRecoverPasswordBtn;

        // ─── 회원가입 패널 필드 ───────────────────────────────────────
        [Header("Signup Panel Fields")]
        public TMP_InputField signupIdInput;
        public TMP_InputField signupPwInput;
        public TMP_InputField signupEmailInput;
        public Button signupSubmitBtn;
        public Button signupBackBtn;

        // ─── 계정 찾기 패널 필드 ──────────────────────────────────────
        [Header("Recover Panel Fields")]
        public TMP_InputField recoverIDEmailInput;
        public Button recoverIDSubmitBtn;
        public Button recoverIDBackBtn;

        // ─── 비밀번호 찾기 패널 필드 ──────────────────────────────────────
        [Header("Recover Panel Fields")]
        public TMP_InputField recoverPasswordIDInput;
        public TMP_InputField recoverPasswordEmailInput;
        public TMP_InputField recoverPasswordNewPasswordInput;
        public Button recoverPasswordSubmitBtn;
        public Button recoverPasswordBackBtn;

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
            if (btn == null)
                return;
            var img = btn.GetComponent<Image>();
            if (img != null && img.color.a < 0.05f)
            {
                img.color = new Color(0, 0, 0, 0.01f);
            }

            // [수정] 자식 중에서 TMP 텍스트를 찾아 레이캐스트 설정을 켭니다.
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.raycastTarget = true;
        }

        private void BindButtons()
        {
            // 인스펙터 연결 보정 로직 (기존 이름 유지)
            var canvas = gameObject;
            if (canvas != null)
            {
                foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (b.name == "SignupSubmitBtn")
                        signupSubmitBtn = b;
                    if (b.name == "SignupBackBtn")
                        signupBackBtn = b;
                    if (b.name == "RecoverIDSubmitBtn")
                        recoverIDSubmitBtn = b;
                    if (b.name == "RecoverIDBackBtn")
                        recoverIDBackBtn = b;
                    if (b.name == "RecoverPasswordSubmitBtn")
                        recoverPasswordSubmitBtn = b;
                    if (b.name == "RecoverPasswordBackBtn")
                        recoverPasswordBackBtn = b;
                }
            }

            CleanButton(loginBtn);
            CleanButton(toSignupBtn);
            CleanButton(toRecoverIDBtn);
            CleanButton(toRecoverPasswordBtn);
            CleanButton(signupSubmitBtn);
            CleanButton(signupBackBtn);
            CleanButton(recoverIDSubmitBtn);
            CleanButton(recoverIDBackBtn);
            CleanButton(recoverPasswordSubmitBtn);
            CleanButton(recoverPasswordBackBtn);

            if (loginBtn != null)
                loginBtn.onClick.AddListener(OnLoginClicked);
            if (toSignupBtn != null)
                toSignupBtn.onClick.AddListener(ShowSignup);
            if (toRecoverIDBtn != null)
                toRecoverIDBtn.onClick.AddListener(ShowIDRecover);
            if (toRecoverPasswordBtn != null)
                toRecoverPasswordBtn.onClick.AddListener(ShowPasswordRecover);

            if (signupSubmitBtn != null)
                signupSubmitBtn.onClick.AddListener(OnSignupClicked);
            if (signupBackBtn != null)
                signupBackBtn.onClick.AddListener(ShowLogin);

            if (recoverIDSubmitBtn != null)
                recoverIDSubmitBtn.onClick.AddListener(OnRecoverIDClicked);
            if (recoverIDBackBtn != null)
                recoverIDBackBtn.onClick.AddListener(ShowLogin);

            if (recoverPasswordSubmitBtn != null)
                recoverPasswordSubmitBtn.onClick.AddListener(OnRecoverPasswordClicked);
            if (recoverPasswordBackBtn != null)
                recoverPasswordBackBtn.onClick.AddListener(ShowLogin);
        }

        private void CleanButton(Button btn)
        {
            if (btn == null)
                return;
            btn.onClick.RemoveAllListeners();
        }

        // ─── 패널 전환 ────────────────────────────────────────────────
        private void ClearAllInputs()
        {
            if (signupIdInput != null)
                signupIdInput.text = "";
            if (signupPwInput != null)
                signupPwInput.text = "";
            if (signupEmailInput != null)
                signupEmailInput.text = "";
            if (recoverIDEmailInput != null)
                recoverIDEmailInput.text = "";
            if (recoverPasswordEmailInput != null)
                recoverPasswordEmailInput.text = "";
            if (recoverPasswordIDInput != null)
                recoverPasswordIDInput.text = "";
            if (recoverPasswordNewPasswordInput != null)
                recoverPasswordNewPasswordInput.text = "";
            if (loginIdInput != null)
                loginIdInput.text = "";
            if (loginPwInput != null)
                loginPwInput.text = "";
        }

        public void ShowLogin()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, true);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecoverID, false);
            SetPanelActive(panelRecoverPassword, false);
            ClearStatus();
        }

        public void ShowSignup()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, true);
            SetPanelActive(panelRecoverID, false);
            SetPanelActive(panelRecoverPassword, false);
            ClearStatus();
        }

        public void ShowIDRecover()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecoverID, true);
            SetPanelActive(panelRecoverPassword, false);
            ClearStatus();
        }

        public void ShowPasswordRecover()
        {
            ClearAllInputs();
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecoverID, false);
            SetPanelActive(panelRecoverPassword, true);
            ClearStatus();
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        // ─── 로그인 로직 ──────────────────────────────────────────────
        private void OnLoginClicked()
        {
            if (
                string.IsNullOrEmpty(loginIdInput?.text) || string.IsNullOrEmpty(loginPwInput?.text)
            )
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
            while (!task.IsCompleted)
                yield return null;

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
            if (
                string.IsNullOrEmpty(signupIdInput?.text)
                || string.IsNullOrEmpty(signupPwInput?.text)
                || string.IsNullOrEmpty(signupEmailInput?.text)
            )
            {
                SetStatus("모든 정보를 입력하세요.", Color.red);
                return;
            }
            SetStatus("회원가입 중...", Color.blue);
            StartCoroutine(SignupRoutine());
        }

        private IEnumerator SignupRoutine()
        {
            var task = AuthManager.Instance.Signup(
                signupIdInput.text,
                signupPwInput.text,
                signupEmailInput.text
            );
            while (!task.IsCompleted)
                yield return null;

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
        private void OnRecoverIDClicked()
        {
            if (string.IsNullOrEmpty(recoverIDEmailInput?.text))
            {
                SetStatus("이메일을 입력하세요.", Color.red);
                return;
            }
            SetStatus("정보 확인 중...", Color.blue);
            StartCoroutine(RecoverRoutine());
        }

        private IEnumerator RecoverRoutine()
        {
            var task = AuthManager.Instance.RecoverID(recoverIDEmailInput.text);
            while (!task.IsCompleted)
                yield return null;

            if (task.Result.Success)
                SetStatus(task.Result.Message, Color.green);
            else
                SetStatus($"확인 실패: {task.Result.Message}", Color.red);
        }

        // ─── 비밀번호 변경 로직 ───────────────────────────────────────────
        private void OnRecoverPasswordClicked()
        {
            if (string.IsNullOrEmpty(recoverPasswordEmailInput?.text))
            {
                SetStatus("이메일을 입력하세요.", Color.red);
                return;
            }

            if (string.IsNullOrEmpty(recoverPasswordIDInput?.text))
            {
                SetStatus("이메일을 입력하세요.", Color.red);
                return;
            }

            if (string.IsNullOrEmpty(recoverPasswordNewPasswordInput?.text))
            {
                SetStatus("이메일을 입력하세요.", Color.red);
                return;
            }
            SetStatus("정보 확인 중...", Color.blue);
            StartCoroutine(RecoverPasswordRoutine());
        }

        private IEnumerator RecoverPasswordRoutine()
        {
            var task = AuthManager.Instance.RecoverPassword(
                recoverPasswordEmailInput.text,
                recoverPasswordIDInput.text,
                recoverPasswordNewPasswordInput.text
            );
            while (!task.IsCompleted)
                yield return null;

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
            if (statusText != null)
                statusText.text = "";
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (
                UnityEngine.InputSystem.Keyboard.current != null
                && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame
            )
            {
                ShowLogin();
            }
#endif
        }
    }
}
