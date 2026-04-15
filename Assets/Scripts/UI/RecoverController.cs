using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Auth;

namespace UI
{
    public class RecoverController : MonoBehaviour
    {
        [Header("Recovery Fields")]
        [UnityEngine.Serialization.FormerlySerializedAs("nicknameInput")]
        public InputField emailInput;
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
            if (string.IsNullOrEmpty(emailInput?.text))
            {
                SetStatus("이메일을 입력하세요.", Color.red);
                return;
            }

            SetStatus("정보 확인 중...", Color.blue);
            StartCoroutine(PerformRecoveryRoutine());
        }

        private IEnumerator PerformRecoveryRoutine()
        {
            var task = AuthManager.Instance.RecoverAccount(emailInput.text);
            while (!task.IsCompleted) yield return null;

            if (task.Result.Success)
            {
                // In bypass mode, Message contains the ID/PW info
                SetStatus(task.Result.Message, Color.green);
            }
            else
            {
                SetStatus($"확인 실패: {task.Result.Message}", Color.red);
            }
        }

        private void OnBackClicked()
        {
            SceneManager.LoadScene("TitleScene");
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
