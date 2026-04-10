using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Auth;

namespace UI
{
    public class SignupController : MonoBehaviour
    {
        [Header("Signup Fields")]
        public InputField signupIdInput;
        public InputField signupPwInput;
        public InputField signupNicknameInput;
        public Button submitBtn;
        public Button backBtn;

        [Header("General")]
        public Text statusText;

        private void Start()
        {
            if (submitBtn != null) 
            {
                submitBtn.onClick.RemoveAllListeners();
                submitBtn.onClick.AddListener(OnSubmitClicked);
            }
            // backBtn navigation handled by Unity Event in Inspector
        }

        private void OnSubmitClicked()
        {
            if (string.IsNullOrEmpty(signupIdInput?.text) || 
                string.IsNullOrEmpty(signupPwInput?.text) || 
                string.IsNullOrEmpty(signupNicknameInput?.text))
            {
                SetStatus("모든 정보를 입력하세요.", Color.red);
                return;
            }

            SetStatus("계정 생성 중...", Color.blue);
            StartCoroutine(PerformSignupRoutine());
        }

        private IEnumerator PerformSignupRoutine()
        {
            var task = AuthManager.Instance.Signup(signupIdInput.text, signupPwInput.text, signupNicknameInput.text);
            while (!task.IsCompleted) yield return null;

            if (task.Result.Success)
            {
                SetStatus("계정 생성 성공! 로그인 화면으로 돌아갑니다.", Color.green);
                yield return new WaitForSeconds(1.5f);
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                SetStatus($"생성 실패: {task.Result.Message}", Color.red);
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
