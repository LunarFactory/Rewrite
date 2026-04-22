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

    public class AuthWebClient : MonoBehaviour
    {
        public static AuthWebClient Instance { get; private set; }

        // EC2 퍼블릭 IP와 포트 (보안 그룹에서 8080 포트가 열려있어야 함)
        private readonly string baseUrl = "http://3.35.26.80:8080/api/v1/player/auth";

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
        public IEnumerator SignUp(
            string id,
            string pw,
            string email,
            System.Action<bool, string> callback
        )
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
                            PlayerPrefs.SetString("AuthToken", res.accessToken); // JWT 토큰 저장
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
        private IEnumerator PostRequest(
            string endpoint,
            string json,
            System.Action<bool, string> callback
        )
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
    }
}
