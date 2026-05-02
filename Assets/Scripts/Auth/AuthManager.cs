using System.IO;
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

        // [아이디 찾기]
        public async Task<AuthResult> RecoverID(string email)
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            if (AuthWebClient.Instance == null)
                return AuthResult.Failed("AuthWebClient 인스턴스가 없습니다.");

            // 코루틴 호출
            StartCoroutine(
                AuthWebClient.Instance.FindId(
                    email,
                    (success, result) =>
                    {
                        tcs.SetResult(
                            new AuthResult
                            {
                                Success = success,
                                // 성공 시 메시지에 ID를 넣어주거나 UserId 필드에 할당
                                Message = success
                                    ? $"사용자 아이디 : {result}"
                                    : "해당 이메일로 가입된 아이디가 없습니다.",
                                UserId = success ? result : null,
                            }
                        );
                    }
                )
            );

            return await tcs.Task;
        }

        // [비밀번호 재설정]
        public async Task<AuthResult> RecoverPassword(string email, string id, string newPassword)
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            if (AuthWebClient.Instance == null)
                return AuthResult.Failed("AuthWebClient가 없습니다.");

            StartCoroutine(
                AuthWebClient.Instance.ResetPassword(
                    email,
                    id,
                    newPassword,
                    (success, message) =>
                    {
                        tcs.SetResult(new AuthResult { Success = success, Message = message });
                    }
                )
            );

            return await tcs.Task;
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

        public async Task<bool> UpdateAIModelAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            StartCoroutine(
                AuthWebClient.Instance.GetLatestModelInfo(
                    (success, info, error) =>
                    {
                        if (!success)
                        {
                            tcs.SetResult(false);
                            return;
                        }

                        // 폴더 및 파일 경로 설정
                        string folderPath = Path.Combine(Application.persistentDataPath, "model");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        string savePath = Path.Combine(folderPath, "dda_model.onnx");
                        string localVersion = PlayerPrefs.GetString("LocalModelVersion", "");

                        // 버전이 같고 파일도 실제로 존재할 때만 통과
                        if (localVersion == info.versionId && File.Exists(savePath))
                        {
                            Debug.Log(
                                "<color=green>모델이 이미 최신 폴더 내에 존재합니다.</color>"
                            );
                            tcs.SetResult(true);
                            return;
                        }

                        // 다운로드 진행
                        StartCoroutine(
                            AuthWebClient.Instance.DownloadModelFile(
                                info.downloadUrl,
                                savePath,
                                (dlSuccess, dlError) =>
                                {
                                    if (dlSuccess)
                                    {
                                        PlayerPrefs.SetString("LocalModelVersion", info.versionId);
                                        PlayerPrefs.Save();
                                        DDAInferenceManager.Instance?.ReloadModel(savePath);
                                        tcs.SetResult(true);
                                    }
                                    else
                                    {
                                        tcs.SetResult(false);
                                    }
                                }
                            )
                        );
                    }
                )
            );

            return await tcs.Task;
        }
    }
}
