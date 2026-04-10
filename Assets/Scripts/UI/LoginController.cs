using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Auth;


namespace UI
{
    public class LoginController : MonoBehaviour
    {
        public InputField idInput;
        public InputField passwordInput;
        public Button loginButton;
        public Button signupButton;
        public Text statusText;

        private void Start()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);
                
            if (signupButton != null)
                signupButton.onClick.AddListener(OnSignupClicked);
        }

        private void OnLoginClicked()
        {
            if (string.IsNullOrEmpty(idInput?.text) || string.IsNullOrEmpty(passwordInput?.text))
            {
                SetStatus("ID와 Password를 입력하세요.", Color.red);
                return;
            }

            SetStatus("로그인 중...", Color.blue);
            loginButton.interactable = false;
            
            StartCoroutine(PerformLoginRoutine());
        }

        private IEnumerator PerformLoginRoutine()
        {
            var loginTask = AuthManager.Instance.Login(idInput.text, passwordInput.text);
            
            // Wait for Task to finish on current thread
            while (!loginTask.IsCompleted)
            {
                yield return null;
            }

            var result = loginTask.Result;
            
            if (result.Success)
            {
                SetStatus("로그인 성공!", Color.green);
                Debug.Log($"Logged in with ID: {result.UserId}");
                
                // Unity-safe delay
                yield return new WaitForSeconds(0.5f);

                // Load Lobby
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.LoadScene("LobbyScene");
                }
                else
                {
                    SceneManager.LoadScene("LobbyScene");
                }
            }
            else
            {
                SetStatus($"로그인 실패: {result.Message}", Color.red);
                loginButton.interactable = true;
            }
        }

        private void OnSignupClicked()
        {
            if (string.IsNullOrEmpty(idInput?.text) || string.IsNullOrEmpty(passwordInput?.text))
            {
                SetStatus("ID와 Password를 입력하세요.", Color.red);
                return;
            }
            
            SetStatus("회원가입 요청됨...", Color.blue);
            StartCoroutine(PerformSignupRoutine());
        }

        private IEnumerator PerformSignupRoutine()
        {
            var signupTask = AuthManager.Instance.Signup(idInput.text, passwordInput.text);
            
            while (!signupTask.IsCompleted)
            {
                yield return null;
            }

            var result = signupTask.Result;
            if (result.Success)
            {
                SetStatus("회원가입 성공! 이제 로그인하세요.", Color.green);
            }
            else
            {
                SetStatus($"회원가입 실패: {result.Message}", Color.red);
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
