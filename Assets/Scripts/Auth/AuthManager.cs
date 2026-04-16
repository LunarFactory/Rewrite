using System.Threading.Tasks;
using UnityEngine;
using Auth;

namespace Auth
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // [로그인 로직]
        public async Task<AuthResult> Login(string id, string password)
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            if (AuthWebClient.Instance == null)
            {
                return new AuthResult { Success = false, Message = "AuthWebClient가 할당되지 않았습니다." };
            }

            // AuthWebClient의 코루틴 호출
            StartCoroutine(AuthWebClient.Instance.Login(id, password, (success, message) =>
            {
                tcs.SetResult(new AuthResult 
                { 
                    Success = success, 
                    Message = success ? "로그인 성공" : message 
                });
            }));

            return await tcs.Task;
        }

        // [회원가입 로직]
        public async Task<AuthResult> Signup(string id, string password, string email)
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            if (AuthWebClient.Instance == null)
            {
                return new AuthResult { Success = false, Message = "AuthWebClient가 할당되지 않았습니다." };
            }

            // AuthWebClient의 코루틴 호출
            StartCoroutine(AuthWebClient.Instance.SignUp(id, password, email, (success, message) =>
            {
                tcs.SetResult(new AuthResult 
                { 
                    Success = success, 
                    Message = success ? "회원가입 성공" : message 
                });
            }));

            return await tcs.Task;
        }

        // [계정 찾기 로직 - 필요 시 구현]
        public async Task<AuthResult> RecoverAccount(string email)
        {
            // 현재 백엔드 API에 맞게 find-id 등을 호출하도록 확장 가능합니다.
            await Task.Delay(500); // 임시 대기
            return new AuthResult { Success = true, Message = "이메일을 확인해주세요." };
        }
    }
}