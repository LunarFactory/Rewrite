/*

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ModelUpdateManager : MonoBehaviour
{
    private string _localModelPath;
    private string _serverUrl = "https://api.yourserver.com/model-info"; // JSON 주소

    void Awake()
    {
        // 저장될 로컬 경로 설정
        _localModelPath = Path.Combine(Application.persistentDataPath, "dda_model.onnx");
    }

    public IEnumerator CheckAndUpdateModel()
    {
        // 1. 서버에서 최신 정보(JSON) 가져오기
        UnityWebRequest request = UnityWebRequest.Get(_serverUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // JSON 파싱 (버전/해시 체크 로직 추가)
            // if (localVersion < serverVersion) ...

            yield return DownloadNewModel(downloadUrlFromSnapshot);
        }
    }

    IEnumerator DownloadNewModel(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // 파일을 로컬에 저장
            File.WriteAllBytes(_localModelPath, www.downloadHandler.data);
            Debug.Log("모델 업데이트 완료: " + _localModelPath);
        }
    }
}

*/
