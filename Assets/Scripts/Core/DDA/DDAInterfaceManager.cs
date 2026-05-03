using System.Collections.Generic;
using System.IO;
using System.Linq;
using Log;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using UnityEngine;

public class DDAInferenceManager : MonoBehaviour
{
    public static DDAInferenceManager Instance { get; private set; }

    // Sentis의 ModelAsset은 유니티 전용 포맷을 위한 것이므로 제거합니다.
    // ONNX Runtime은 파일 경로나 Byte 배열에서 직접 모델을 로드합니다.
    private InferenceSession _session;

    [Header("DDA Settings")]
    [Range(0.1f, 2.0f)]
    public float currentAlpha = 1.0f;

    private void Awake()
    {
        Instance = this;
        LoadModelFromDisk();
    }

    public void ReloadModel(string newModelPath)
    {
        try
        {
            // 기존 세션 해제
            CleanupSession();

            // 새 모델 로드 및 세션 생성
            _session = new InferenceSession(newModelPath);

            Debug.Log("<color=cyan>[AI]</color> 새로운 모델 엔진이 적용되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"모델 재로드 중 오류 발생: {e.Message}");
        }
    }

    public void LoadModelFromDisk()
    {
        string path = Path.Combine(
            Path.Combine(Application.persistentDataPath, "Model"),
            "dda_model.onnx"
        );

        Debug.Log($"모델 로드 경로: {path}");

        if (File.Exists(path))
        {
            try
            {
                // [중요] ONNX Runtime에서 모델 파일 경로로 바로 세션 생성
                _session = new InferenceSession(path);
                Debug.Log("성공적으로 외부 모델을 로드했습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ONNX 모델 로딩 실패: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("AI 모델 파일이 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 웨이브 로그를 기반으로 다음 웨이브의 난이도(alpha)를 추론합니다.
    /// </summary>
    public (float s, float c, float alpha) InferDifficulty(WaveLogData logData)
    {
        if (_session == null)
        {
            Debug.LogError("Inference Session이 초기화되지 않았습니다.");
            return (0, 0, currentAlpha);
        }

        // 1. 입력 데이터 준비 (모델의 입력 피처 개수와 일치해야 함)
        float[] inputFeatures = new float[]
        {
            logData.dashboard_summary.apm / 300f,
            logData.dashboard_summary.accuracy_rate,
            logData.dashboard_summary.hits_taken / 20f,
            logData.dashboard_summary.hp_retention_rate,
        };

        // 2. ONNX Runtime용 Tensor 생성 (Shape: [1, 4])
        var inputTensor = new DenseTensor<float>(inputFeatures, new int[] { 1, 4 });

        // 3. 입력값 매핑
        // "input" 부분은 실제 ONNX 모델의 인풋 레이어 이름과 정확히 일치해야 합니다.
        // (이전 스크린샷의 모델이라면 "game_metrics_30s" 여야 합니다)
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor),
        };

        // 4. 추론 실행 (using 블록을 통해 결과 사용 후 즉시 메모리 해제)
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(
            inputs
        );

        // 5. 결과 추출 (단일 출력 레이어에 [1, 2] 형태의 결과가 나온다고 가정한 로직)
        var outputTensor = results.First().AsTensor<float>();

        float s = outputTensor.GetValue(0); // Skill (숙련도)
        float c = outputTensor.GetValue(1); // Churn (이탈 위험)

        /*
        [참고] 만약 이전 스크린샷처럼 출력 레이어가 S_score, C_risk 2개로 나뉘어 있다면 아래 코드를 사용하세요.
        float s = results.First(r => r.Name == "S_score").AsTensor<float>().GetValue(0);
        float c = results.First(r => r.Name == "C_risk").AsTensor<float>().GetValue(0);
        */

        // 6. 알파 계산 로직
        float targetAlpha = CalculateAlpha(s, c);
        currentAlpha = targetAlpha;

        return (s, c, targetAlpha);
    }

    private float CalculateAlpha(float s, float c)
    {
        float alpha = 1.0f + (s * 0.5f) - (c * 0.5f);
        return Mathf.Clamp(alpha, 0.5f, 2.0f);
    }

    private void OnDestroy()
    {
        CleanupSession();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDisable()
    {
        CleanupSession();
    }

    private void CleanupSession()
    {
        if (_session != null)
        {
            _session.Dispose();
            _session = null;
            Debug.Log("[DDA] InferenceSession disposed successfully.");
        }
    }
}
