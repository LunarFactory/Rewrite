using System.Threading.Tasks;
using Log;
using Unity.InferenceEngine;
using UnityEngine;

public class DDAInferenceManager : MonoBehaviour
{
    public static DDAInferenceManager Instance { get; private set; }

    [Header("Sentis Assets")]
    [SerializeField]
    private ModelAsset modelAsset; // 프로젝트창의 .onnx 파일 드래그
    private Model _runtimeModel;
    private Worker _worker;

    [Header("DDA Settings")]
    [Range(0.1f, 2.0f)]
    public float currentAlpha = 1.0f;

    private void Awake()
    {
        Instance = this;
        // 1. 모델 로드 및 워커(엔진) 생성
        _runtimeModel = ModelLoader.Load(modelAsset);
        _worker = new Worker(_runtimeModel, BackendType.GPUCompute);
    }

    /// <summary>
    /// 웨이브 로그를 기반으로 다음 웨이브의 난이도(alpha)를 추론합니다.
    /// </summary>
    public (float s, float c, float alpha) InferDifficulty(WaveLogData logData)
    {
        // 2. 입력 데이터 준비 (모델의 입력 피처 순서와 일치해야 함)
        // 예: APM, Accuracy, HitsTaken, HP_Retention 4개의 피처를 사용한다고 가정
        float[] inputFeatures = new float[]
        {
            logData.dashboard_summary.apm / 300f, // 정규화 (예: 최대 300)
            logData.dashboard_summary.accuracy_rate, // 0~1
            logData.dashboard_summary.hits_taken / 20f, // 정규화
            logData.dashboard_summary.hp_retention_rate, // 0~1
        };

        using Tensor inputTensor = new Tensor<float>(new TensorShape(1, 4), inputFeatures);

        // 3. 추론 실행
        _worker.SetInput("input", inputTensor);

        // 4. 결과값 추출 (출력이 [1, 2] 형태라고 가정: [0]=s, [1]=c)
        Tensor<float> outputTensor = _worker.PeekOutput() as Tensor<float>;
        var results = outputTensor.ReadbackAndClone(); // GPU 데이터를 CPU로 읽어옴

        float s = outputTensor[0]; // Skill (숙련도)
        float c = outputTensor[1]; // Churn (이탈 위험)

        // 5. 알파 계산 로직 (예시 공식: s가 높으면 alpha 증가, c가 높으면 alpha 감소)
        // 사용자님의 구체적인 공식에 따라 수정하세요.
        float targetAlpha = CalculateAlpha(s, c);
        currentAlpha = targetAlpha;

        return (s, c, targetAlpha);
    }

    private float CalculateAlpha(float s, float c)
    {
        // 예시: 기본 1.0에서 숙련도가 높으면 난이도 업(+), 이탈위험이 높으면 난이도 다운(-)
        float alpha = 1.0f + (s * 0.5f) - (c * 0.5f);
        return Mathf.Clamp(alpha, 0.5f, 2.0f); // 최소 0.5배 ~ 최대 2.0배 제한
    }

    private void OnDestroy()
    {
        CleanupWorker();

        // 싱글톤 참조를 명확히 해제
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDisable()
    {
        // 워커를 여기서 미리 정리해주는 것이 안전합니다.
        CleanupWorker();
    }

    private void CleanupWorker()
    {
        if (_worker != null)
        {
            _worker.Dispose();
            _worker = null;
            Debug.Log("[DDA] Worker disposed successfully.");
        }
    }
}
