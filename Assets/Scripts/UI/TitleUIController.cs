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
    public class TitleUIController : MonoBehaviour
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
        public InputField signupNicknameInput;
        public Button signupSubmitBtn;
        public Button signupBackBtn;

        // ─── 계정 찾기 패널 필드 ──────────────────────────────────────
        [Header("Recover Panel Fields")]
        public InputField recoverNicknameInput;
        public Button recoverSubmitBtn;
        public Button recoverBackBtn;

        // ─── 공통 ─────────────────────────────────────────────────────
        [Header("General")]
        public Text statusText;

        // ─────────────────────────────────────────────────────────────
        private void Start()
        {
            FixRaycastBlockers();
            FixButtonLayouts();
            BindButtons();
            ShowLogin(); // 시작 시 로그인 패널 표시
        }

        private void FixButtonLayouts()
        {
            // 투명 배경 버튼(Color.clear)이 클릭을 온전히 못 받는 유니티 이슈 방지: 아주 옅은 투명도를 강제로 줌
            SafeFixClearBackground(signupBackBtn);
            SafeFixClearBackground(recoverBackBtn);

            // "아이디/비밀번호 찾기" 버튼이 텍스트 위쪽만 눌리는 이유: 기존 씬에서 텍스트가 아래로 삐져나와 있어서 조준이 안맞는 현상.
            // 텍스트를 버튼의 정확한 정중앙으로 강제 정렬시켜 패딩을 동일하게 맞춤
            if (toRecoverBtn != null)
            {
                var txt = toRecoverBtn.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.rectTransform.anchorMin = Vector2.zero;
                    txt.rectTransform.anchorMax = Vector2.one;
                    txt.rectTransform.offsetMin = Vector2.zero;
                    txt.rectTransform.offsetMax = Vector2.zero;
                    txt.alignment = TextAnchor.MiddleCenter;
                }
                SafeFixClearBackground(toRecoverBtn);
            }
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

        private void FixRaycastBlockers()
        {
            // Canvas 내의 모든 Text, Image 중 Button이나 InputField가 없는 요소는 클릭을 가로채지 못하게 막음.
            // (특히 아래쪽에 깔린 StatusText가 500x100 크기로 뒷단 버튼 클릭을 방해했던 문제 해결)
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                foreach (var text in canvas.GetComponentsInChildren<Text>(true))
                {
                    if (text.GetComponentInParent<Button>() == null && text.GetComponentInParent<InputField>() == null)
                        text.raycastTarget = false;
                }
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    if (img.GetComponentInParent<Button>() == null && img.GetComponentInParent<InputField>() == null)
                        img.raycastTarget = false;
                }
            }
            else
            {
                Debug.LogWarning("[TitleUIController] Canvas를 찾을 수 없어 Raycast 블로커를 해제하지 못했습니다.");
            }
        }

        private void BindButtons()
        {
            // 컴포넌트 정리 (이전에 붙어있던 SceneLoadTrigger 등이 에러를 낼 수 있음)
            CleanButton(loginBtn);
            CleanButton(toSignupBtn);
            CleanButton(toRecoverBtn);
            CleanButton(signupSubmitBtn);
            CleanButton(signupBackBtn);
            CleanButton(recoverSubmitBtn);
            CleanButton(recoverBackBtn);

            // 로그인 패널
            if (loginBtn != null)         loginBtn.onClick.AddListener(OnLoginClicked);
            if (toSignupBtn != null)      toSignupBtn.onClick.AddListener(ShowSignup);
            if (toRecoverBtn != null)     toRecoverBtn.onClick.AddListener(ShowRecover);

            // 회원가입 패널
            if (signupSubmitBtn != null)  signupSubmitBtn.onClick.AddListener(OnSignupClicked);
            if (signupBackBtn != null)    signupBackBtn.onClick.AddListener(ShowLogin);

            // 계정 찾기 패널
            if (recoverSubmitBtn != null) recoverSubmitBtn.onClick.AddListener(OnRecoverClicked);
            if (recoverBackBtn != null)   recoverBackBtn.onClick.AddListener(ShowLogin);

            Debug.Log("[TitleUIController] 모든 버튼 바인딩 완료!");
        }

        private void CleanButton(Button btn)
        {
            if (btn == null) return;

            // 기존 Scene 로드 트리거가 있다면 삭제 (MissingReference 방지)
            var sceneTrigger = btn.GetComponent<SceneLoadTrigger>();
            if (sceneTrigger != null) Destroy(sceneTrigger);

            // 런타임 리스너 모두 초기화
            btn.onClick.RemoveAllListeners();
        }

        // ─── 패널 전환 ────────────────────────────────────────────────
        public void ShowLogin()
        {
            Debug.Log("[TitleUIController] ShowLogin() called! Switching to Login Panel.");
            SetPanelActive(panelLogin, true);
            SetPanelActive(panelSignup, false);
            SetPanelActive(panelRecover, false);
            ClearStatus();
        }

        public void ShowSignup()
        {
            Debug.Log("[TitleUIController] ShowSignup() called! Switching to Signup Panel.");
            SetPanelActive(panelLogin, false);
            SetPanelActive(panelSignup, true);
            SetPanelActive(panelRecover, false);
            ClearStatus();
        }

        public void ShowRecover()
        {
            Debug.Log("[TitleUIController] ShowRecover() called! Switching to Recover Panel.");
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
                SceneManager.LoadScene("LobbyScene");
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
                string.IsNullOrEmpty(signupNicknameInput?.text))
            {
                SetStatus("모든 정보를 입력하세요.", Color.red);
                return;
            }
            SetStatus("계정 생성 중...", Color.blue);
            StartCoroutine(SignupRoutine());
        }

        private IEnumerator SignupRoutine()
        {
            var task = AuthManager.Instance.Signup(signupIdInput.text, signupPwInput.text, signupNicknameInput.text);
            while (!task.IsCompleted) yield return null;

            if (task.Result.Success)
            {
                SetStatus("계정 생성 성공! 로그인 화면으로 돌아갑니다.", Color.green);
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
            if (string.IsNullOrEmpty(recoverNicknameInput?.text))
            {
                SetStatus("닉네임을 입력하세요.", Color.red);
                return;
            }
            SetStatus("정보 확인 중...", Color.blue);
            StartCoroutine(RecoverRoutine());
        }

        private IEnumerator RecoverRoutine()
        {
            var task = AuthManager.Instance.RecoverAccount(recoverNicknameInput.text);
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
                statusText.text = message;
                statusText.color = color;
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
