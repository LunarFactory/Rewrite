using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
                if (statusText != null) statusText.text = "ID와 Password를 입력하세요.";
                return;
            }

            if (statusText != null) statusText.text = "로그인 중...";
            loginButton.interactable = false;
            
            // Mock network call
            StartCoroutine(MockLoginRoutine());
        }

        private IEnumerator MockLoginRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            
            // Assume success
            if (statusText != null) statusText.text = "로그인 성공!";
            Debug.Log($"Logged in with ID: {idInput.text}");
            
            // Save mock token
            PlayerPrefs.SetString("SessionToken", "mock_token_12345");
            PlayerPrefs.SetString("UserId", idInput.text);
            PlayerPrefs.Save();

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

        private void OnSignupClicked()
        {
            // Just display a mock message
            if (statusText != null) statusText.text = "회원가입 요청됨 (진행중...)";
            Debug.Log("Signup Clicked. Mock flow.");
        }
    }
}
