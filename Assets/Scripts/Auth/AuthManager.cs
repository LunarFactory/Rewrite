using System.Threading.Tasks;
using UnityEngine;

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
                return new AuthResult
                {
                    Success = false,
                    Message = "AuthWebClient가 할당되지 않았습니다.",
                };
            }

            // AuthWebClient의 코루틴 호출
            StartCoroutine(
                AuthWebClient.Instance.Login(
                    id,
                    password,
                    (success, message) =>
                    {
                        tcs.SetResult(
                            new AuthResult
                            {
                                Success = success,
                                Message = success ? "로그인 성공" : message,
                            }
                        );
                    }
                )
            );

            return await tcs.Task;
        }

        // [회원가입 로직]
        public async Task<AuthResult> Signup(string id, string password, string email)
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            if (AuthWebClient.Instance == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "AuthWebClient가 할당되지 않았습니다.",
                };
            }

            // AuthWebClient의 코루틴 호출
            StartCoroutine(
                AuthWebClient.Instance.SignUp(
                    id,
                    password,
                    email,
                    (success, message) =>
                    {
                        tcs.SetResult(
                            new AuthResult
                            {
                                Success = success,
                                Message = success ? "회원가입 성공" : message,
                            }
                        );
                    }
                )
            );

            return await tcs.Task;
        }

        // [계정 찾기 로직 - 필요 시 구현]
        public async Task<AuthResult> RecoverAccount(string email)
        {
            // 현재 백엔드 API에 맞게 find-id 등을 호출하도록 확장 가능합니다.
            await Task.Delay(500); // 임시 대기
            return new AuthResult { Success = true, Message = "이메일을 확인해주세요." };
        }

        public async Task<AuthResult> Logout()
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            if (AuthWebClient.Instance == null)
            {
                return new AuthResult { Success = false, Message = "AuthWebClient가 없습니다." };
            }

            // 1. 서버에 로그아웃 요청
            StartCoroutine(
                AuthWebClient.Instance.Logout(
                    (success, message) =>
                    {
                        // 2. [핵심] 서버 응답이 성공이든 실패든 로컬 데이터는 지웁니다.
                        // (네트워크가 없어도 로그아웃은 되어야 하니까요)
                        PlayerPrefs.DeleteKey("AuthToken");
                        PlayerPrefs.DeleteKey("UserId");
                        PlayerPrefs.Save();

                        // 3. 로그 추적기 정보 갱신 (Guest로 변경)
                        if (Log.LogTracker.Instance != null)
                        {
                            Log.LogTracker.Instance.RefreshUserInfo();
                        }

                        tcs.SetResult(
                            new AuthResult
                            {
                                Success = success,
                                Message = success
                                    ? "로그아웃 되었습니다."
                                    : "서버 로그아웃 실패(로컬 데이터만 삭제)",
                            }
                        );
                    }
                )
            );

            return await tcs.Task;
        }
    }
}
