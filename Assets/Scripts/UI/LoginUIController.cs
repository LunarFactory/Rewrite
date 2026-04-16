using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Auth;

namespace UI
{
    /// <summary>
    /// TitleScene 전용 통합 컨트롤러.
    /// 왼쪽 패널은 고정하고, 오른쪽 세 패널(로그인/회원가입/계정찾기)을 Show/Hide로 전환.
    /// </summary>
    public class LoginUIController : MonoBehaviour
    {
        // ─── 패널 루트 ────────────────────────────────────────────────
        [Header("Right Panels (오른쪽 교체 패널)")]
        public GameObject panelLogin;
        public GameObject panelSignup;
        public GameObject panelRecover;

        // ─── 로그인 패널 필드 ─────────────────────────────────────────
        [Header("Login Panel Fields")]
        public InputField loginIdInput;
        public InputField loginPwInput;
        public Button loginBtn;
        public Button toSignupBtn;
        public Button toRecoverBtn;

        // ─── 회원가입 패널 필드 ───────────────────────────────────────
        [Header("Signup Panel Fields")]
        public InputField signupIdInput;
        public InputField signupPwInput;
        [UnityEngine.Serialization.FormerlySerializedAs("signupNicknameInput")]
        public InputField signupEmailInput;
        public Button signupSubmitBtn;
        public Button signupBackBtn;

        // ─── 계정 찾기 패널 필드 ──────────────────────────────────────
        [Header("Recover Panel Fields")]
        [UnityEngine.Serialization.FormerlySerializedAs("recoverNicknameInput")]
        public InputField recoverEmailInput;
        public Button recoverSubmitBtn;
        public Button recoverBackBtn;

        // ─── 공통 ─────────────────────────────────────────────────────
        [Header("General")]
        public Text statusText;

        // ─────────────────────────────────────────────────────────────
        private void Start()
        {
            BindButtons();
            ShowLogin(); // 시작 시 로그인 패널 표시
        }


        private void SafeFixClearBackground(Button btn)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null && img.color.a < 0.05f)
            {
                img.color = new Color(0, 0, 0, 0.01f); // 눈에 안보이지만 클릭(Raycast)을 즉각 캐치하는 1% 알파 코팅
            }

            // 안에 있는 텍스트의 클릭 판정도 무조건 켬
            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null) txt.raycastTarget = true;
        }

        private void BindButtons()
        {
            // [안전 장치] 에디터 인스펙터에서 버튼(Submit과 Back 등) 연결이 뒤바뀐 상태로 저장되어 
            // 가입 버튼을 눌렀을 때 엉뚱하게 뒤로가기가 실행되는 것을 막기 위해 강제로 실제 이름으로 덮어씁니다.
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

            // 컴포넌트 정리 (이전에 붙어있던 SceneLoadTrigger 등이 에러를 낼 수 있음)
            CleanButton(loginBtn);
            CleanButton(toSignupBtn);
            CleanButton(toRecoverBtn);
            CleanButton(signupSubmitBtn);
            CleanButton(signupBackBtn);
            CleanButton(recoverSubmitBtn);
            CleanButton(recoverBackBtn);

            // 로그인 패널
            if (loginBtn != null) loginBtn.onClick.AddListener(OnLoginClicked);
            if (toSignupBtn != null) toSignupBtn.onClick.AddListener(ShowSignup);
            if (toRecoverBtn != null) toRecoverBtn.onClick.AddListener(ShowRecover);

            // 회원가입 패널
            if (signupSubmitBtn != null) signupSubmitBtn.onClick.AddListener(OnSignupClicked);
            if (signupBackBtn != null) signupBackBtn.onClick.AddListener(ShowLogin);

            // 계정 찾기 패널
            if (recoverSubmitBtn != null) recoverSubmitBtn.onClick.AddListener(OnRecoverClicked);
            if (recoverBackBtn != null) recoverBackBtn.onClick.AddListener(ShowLogin);

            Debug.Log("[TitleUIController] 모든 버튼 바인딩 완료!");
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
            Debug.Log("[TitleUIController] ShowLogin() called! Switching to Login Panel.");
            ClearAllInputs();
            SetPanelActive(panelLogin, true);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecover, false);
            ClearStatus();
        }

        public void ShowSignup()
        {
            Debug.Log("[TitleUIController] ShowSignup() called! Switching to Signup Panel.");
            ClearAllInputs();
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, true);
            SetPanelActive(panelRecover, false);
            ClearStatus();
        }

        public void ShowRecover()
        {
            Debug.Log("[TitleUIController] ShowRecover() called! Switching to Recover Panel.");
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
                // 메시지가 표시될 때는 statusText를 Canvas의 최상단 레벨로 꺼내어 특정 패널이 꺼져도 영향을 받지 않도록 합니다.
                var canvas = gameObject;
                if (canvas != null && statusText.transform.parent != canvas.transform)
                {
                    statusText.transform.SetParent(canvas.transform, true);
                }
                statusText.transform.SetAsLastSibling();
                statusText.gameObject.SetActive(true);

                statusText.text = message;
                statusText.color = color;

                // 로그인이나 타 버튼들과 겹치지 않도록 안전한 위치로 강제 이동 (-250은 우측 하단 여백)
                statusText.rectTransform.anchoredPosition = new Vector2(0, -250);
            }
        }

        private void ClearStatus()
        {
            if (statusText != null) statusText.text = "";
        }

        private void Update()
        {
            // 사용자가 New Input System을 쓰기 때문에 구버전 Input은 꺼두거나 제거
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[TitleUIController] ESC 키 입력 감지 -> 강제 ShowLogin() 호출");
                ShowLogin();
            }
#endif
        }

    }
}
