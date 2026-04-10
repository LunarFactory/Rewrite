using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Auth;

namespace UI
{
    public class LoginController : MonoBehaviour
    {
        [Header("Login Fields")]
        public InputField loginIdInput;
        public InputField loginPwInput;
        public Button loginBtn;
        public Button toSignupBtn;
        public Button toRecoverBtn;

        [Header("General")]
        public Text statusText;

        private void Start()
        {
            SetupButtonListeners();
        }

        private void SetupButtonListeners()
        {
            if (loginBtn != null) 
            {
                loginBtn.onClick.RemoveAllListeners();
                loginBtn.onClick.AddListener(OnLoginClicked);
            }
            // Navigation is now handled explicitly via Unity Events (SceneLoadTrigger) in the Inspector
        }

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
