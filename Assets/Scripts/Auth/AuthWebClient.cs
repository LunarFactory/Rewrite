using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Auth
{
    // --- 서버와 주고받을 데이터 규격 (DTO) ---

    [Serializable] // 유니티 JsonUtility가 인식할 수 있게 꼭 붙여야 합니다!
    public class SignUpRequest
    {
        public string userId;
        public string password;
        public string email;
    }

    [Serializable]
    public class TokenResponse
    {
        public string userId;
        public string accessToken;
    }

    [Serializable]
    public class FindIdRequest { public string email; }

    [Serializable]
    public class FindIdResponse { public string userId; }

    [Serializable]
    public class ResetPasswordRequest
    {
        public string email;
        public string userId;
        public string newPassword;
    }

    public class AuthWebClient : MonoBehaviour
    {
        public static AuthWebClient Instance { get; private set; }

        // EC2 퍼블릭 IP와 포트 (보안 그룹에서 8080 포트가 열려있어야 함)
        private readonly string baseUrl = "http://13.209.66.250:8080/api/v1/player/auth";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        // [회원가입]
        public IEnumerator SignUp(string id, string pw, string email, Action<bool, string> callback)
        {
            var data = new SignUpRequest
            {
                userId = id,
                password = pw,
                email = email,
            };
            string json = JsonUtility.ToJson(data);

            yield return StartCoroutine(
                PostRequest(
                    "/signup",
                    json,
                    (success, response) =>
                    {
                        callback?.Invoke(success, success ? "회원가입 성공!" : response);
                    }
                )
            );
        }

        // [로그인]
        public IEnumerator Login(string id, string pw, System.Action<bool, string> callback)
        {
            var data = new { userId = id, password = pw }; // 익명 객체 사용 시 JsonUtility는 지원 안 할 수 있음 (별도 DTO 권장)
            string json = $"{{\"userId\":\"{id}\", \"password\":\"{pw}\"}}";

            yield return StartCoroutine(
                PostRequest(
                    "/login",
                    json,
                    (success, response) =>
                    {
                        if (success)
                        {
                            var res = JsonUtility.FromJson<TokenResponse>(response);
                            PlayerPrefs.SetString("AuthToken", res.accessToken);
                            PlayerPrefs.SetString("UserId", res.userId);
                            PlayerPrefs.Save(); // 즉시 반영
                            callback?.Invoke(true, res.userId);
                        }
                        else
                        {
                            callback?.Invoke(false, response);
                        }
                    }
                )
            );
        }

        // [공통 POST 요청 메서드]
        private IEnumerator PostRequest(string endpoint, string json, Action<bool, string> callback)
        {
            using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // 로그인 후의 요청이라면 토큰을 헤더에 담아야 함 (예: 로그아웃)
                if (PlayerPrefs.HasKey("AuthToken"))
                {
                    request.SetRequestHeader(
                        "Authorization",
                        "Bearer " + PlayerPrefs.GetString("AuthToken")
                    );
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, request.downloadHandler.text);
                }
                else
                {
                    callback?.Invoke(false, request.error);
                }
            }
        }

        public IEnumerator Logout(Action<bool, string> callback)
        {
            // 로그아웃은 보통 보낼 데이터(Body)가 없으므로 빈 JSON을 보냅니다.
            yield return StartCoroutine(
                PostRequest(
                    "/logout",
                    "{}",
                    (success, response) =>
                    {
                        callback?.Invoke(success, success ? "로그아웃 성공" : response);
                    }
                )
            );
        }
        public IEnumerator FindId(string email, Action<bool, string> callback)
        {
            // 1. URL 구성 확인 (로그로 찍어서 브라우저에 직접 붙여넣어 보세요)
            string url = $"{baseUrl}/find-id?email={UnityWebRequest.EscapeURL(email)}";
            Debug.Log($"[AuthWebClient] 요청 URL: {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string rawResponse = request.downloadHandler.text;
                    Debug.Log($"[AuthWebClient] 서버 응답: {rawResponse}");

                    try
                    {
                        // 서버가 {"userId": "아이디"} 가 아니라 그냥 "아이디"만 보낼 수도 있어요.
                        if (rawResponse.StartsWith("{"))
                        {
                            var res = JsonUtility.FromJson<FindIdResponse>(rawResponse);
                            callback?.Invoke(true, res.userId);
                        }
                        else
                        {
                            // 그냥 문자열로 오면 바로 반환
                            callback?.Invoke(true, rawResponse);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[AuthWebClient] 파싱 에러: {e.Message}");
                        callback?.Invoke(false, "데이터 형식 오류");
                    }
                }
                else
                {
                    // 404, 500 에러 등 확인
                    Debug.LogError($"[AuthWebClient] HTTP 에러: {request.responseCode} | {request.error}");
                    callback?.Invoke(false, $"에러: {request.responseCode}");
                }
            }
        }

        public IEnumerator ResetPassword(string email, string id, string newPw, Action<bool, string> callback)
        {
            var data = new ResetPasswordRequest
            {
                email = email,
                userId = id,
                newPassword = newPw
            };
            string json = JsonUtility.ToJson(data);

            yield return StartCoroutine(PostRequest("/find-password", json, (success, response) =>
            {
                callback?.Invoke(success, success ? "비밀번호가 성공적으로 변경되었습니다." : response);
            }));
        }
    }
}
