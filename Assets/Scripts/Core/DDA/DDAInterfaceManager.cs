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
        float[] inputFeatures = PrepareInputTensor(logData);

        // 2. ONNX Runtime용 Tensor 생성 (Shape: [1, 4])
        var inputTensor = new DenseTensor<float>(inputFeatures, new int[] { 1, 5, 5 });

        // 3. 입력값 매핑
        // "input" 부분은 실제 ONNX 모델의 인풋 레이어 이름과 정확히 일치해야 합니다.
        // (이전 스크린샷의 모델이라면 "game_metrics_30s" 여야 합니다)
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("game_metrics_30s", inputTensor),
        };

        // 4. 추론 실행 (using 블록을 통해 결과 사용 후 즉시 메모리 해제)
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(
            inputs
        );

        // 5. 결과 추출 (단일 출력 레이어에 [1, 2] 형태의 결과가 나온다고 가정한 로직)
        var outputTensor = results.First().AsTensor<float>();
        float s = results.First(r => r.Name == "S_score").AsTensor<float>().GetValue(0);
        float c = results.First(r => r.Name == "C_risk").AsTensor<float>().GetValue(0);

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

    public float[] PrepareInputTensor(WaveLogData logData)
    {
        const int sequenceLength = 5;
        const int featureCount = 5;
        float[] tensorData = new float[sequenceLength * featureCount]; // 총 50개

        var frames = logData.time_series_frames;
        int frameCount = frames != null ? frames.Count : 0;

        // 최근 10개만 가져오되, 데이터가 부족하면 있는 만큼만 가져옴
        int itemsToCopy = Mathf.Min(frameCount, sequenceLength);
        int startIdx = frameCount - itemsToCopy;

        for (int i = 0; i < itemsToCopy; i++)
        {
            var frame = frames[startIdx + i];

            // 텐서의 행(Row) 위치 계산: (10개 중 뒤쪽부터 채우기 위해 오프셋 계산)
            // 데이터가 3개라면 인덱스 7, 8, 9 행에 배치됨 (앞은 0.0f 패딩)
            int targetRow = (sequenceLength - itemsToCopy) + i;
            int rowOffset = targetRow * featureCount;

            // 사용자님이 요청하신 순서대로 피쳐 맵핑
            tensorData[rowOffset + 0] = frame.apm; // apm (정규화 포함)
            tensorData[rowOffset + 1] = frame.inverse_hit_rate; // inverse_hit_rate
            tensorData[rowOffset + 2] = frame.hp_retention_rate; // hp_retention_rate
            tensorData[rowOffset + 3] = frame.accuracy; // accuracy
            tensorData[rowOffset + 4] = frame.attack_item_efficiency; // attack_time_efficiency
        }

        return tensorData;
    }
}
