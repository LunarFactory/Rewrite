using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;

namespace UI.DDA
{
    public class DDAAnalyticsPanel : MonoBehaviour
    {
        private GameObject panelRoot;
        private UIRadarChart radarChart;
        private TextMeshProUGUI logText;
        private UnityEngine.UI.ScrollRect scrollRect;
        private TextMeshProUGUI apmLabel;
        private TextMeshProUGUI accLabel;
        private TextMeshProUGUI evaLabel;
        private TMP_FontAsset defaultFont;

        private void Awake()
        {
            // 프로젝트 규격에 맞춘 폰트 로드
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts/Galmuri11");
            BuildPanel();
            WaveManager.OnDDACalculated += HandleDDACalculated;
        }

        private void OnEnable()
        {
            StartCoroutine(ScrollToBottom());
        }

        private System.Collections.IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnDestroy()
        {
            WaveManager.OnDDACalculated -= HandleDDACalculated;
        }

        private void BuildPanel()
        {
            panelRoot = new GameObject("DDA_AnalyticsPanel");
            panelRoot.transform.SetParent(this.transform, false);
            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;

            // 패널 배경
            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.4f);

            // 타이틀 텍스트
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelRoot.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f); titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f); titleRt.anchoredPosition = new Vector2(0, -20);
            titleRt.sizeDelta = new Vector2(450, 60);

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "[ AI DDA SYSTEM DASHBOARD ]";
            title.fontSize = 24; title.alignment = TextAlignmentOptions.Center;
            title.color = Color.cyan; title.fontStyle = FontStyles.Bold;
            if (defaultFont != null) title.font = defaultFont;

            BuildRadarChartArea(panelRoot.transform);
            BuildLogArea(panelRoot.transform);
        }

        private void BuildRadarChartArea(Transform parent)
        {
            GameObject chartArea = new GameObject("ChartArea");
            chartArea.transform.SetParent(parent, false);
            RectTransform rt = chartArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.7f); rt.anchorMax = new Vector2(0.5f, 0.7f);
            rt.anchoredPosition = new Vector2(0, -60); rt.sizeDelta = new Vector2(300, 300);

            // 차트 뒷배경 (연한 회색)
            GameObject bgPoly = new GameObject("BgPoly");
            bgPoly.transform.SetParent(chartArea.transform, false);
            bgPoly.AddComponent<CanvasRenderer>();
            UIRadarChart bgChart = bgPoly.AddComponent<UIRadarChart>();
            bgChart.color = new Color(1, 1, 1, 0.1f);
            bgChart.radius = 120f; bgChart.UpdateValues(1f, 1f, 1f);

            // 실제 유저 데이터 차트 (진한 청록색)
            GameObject valPoly = new GameObject("ValPoly");
            valPoly.transform.SetParent(chartArea.transform, false);
            valPoly.AddComponent<CanvasRenderer>();
            radarChart = valPoly.AddComponent<UIRadarChart>();
            radarChart.color = new Color(0, 1, 1, 0.6f);
            radarChart.radius = 120f;

            apmLabel = CreateLabel(chartArea.transform, "APM\n0", new Vector2(0, 140));
            accLabel = CreateLabel(chartArea.transform, "명중률\n50%", new Vector2(-140, -70));
            evaLabel = CreateLabel(chartArea.transform, "회피율\n50%", new Vector2(140, -70));
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, Vector2 pos)
        {
            GameObject lblObj = new GameObject("Label");
            lblObj.transform.SetParent(parent, false);
            RectTransform rt = lblObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(150, 40);

            TextMeshProUGUI tmp = lblObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = 18; tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            if (defaultFont != null) tmp.font = defaultFont;
            return tmp;
        }

        private void BuildLogArea(Transform parent)
        {
            GameObject logArea = new GameObject("LogArea");
            logArea.transform.SetParent(parent, false);
            RectTransform rt = logArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.05f); rt.anchorMax = new Vector2(0.9f, 0.45f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            Image bg = logArea.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            scrollRect = logArea.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 15f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(logArea.transform, false);
            RectTransform vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(15, 15); vpRt.offsetMax = new Vector2(-15, -15);
            viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject textObj = new GameObject("LogText");
            textObj.transform.SetParent(viewport.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 1); textRt.anchorMax = new Vector2(1, 1);
            textRt.pivot = new Vector2(0.5f, 1);
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = new Vector2(0, 0);

            logText = textObj.AddComponent<TextMeshProUGUI>();
            logText.fontSize = 16; logText.color = Color.white;
            logText.text = "시스템 대기 중...\n";
            logText.alignment = TextAlignmentOptions.TopLeft;
            logText.richText = true;
            if (defaultFont != null) logText.font = defaultFont;

            UnityEngine.UI.ContentSizeFitter csf = textObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            csf.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRt;
            scrollRect.content = textRt;
        }

        private void HandleDDACalculated(float s, float c, float alpha, int baseBudget, int finalBudget, float rawApm, float accuracy, float evasion)
        {
            float apmRate = Mathf.Clamp01(rawApm / 300f);

            // 차트 모양 보정: 제곱근(Sqrt) 곡선을 사용하여 0~1 사이의 비율을 유지하면서 낮은 값만 부드럽게 끌어올립니다.
            // 예: 0.1 -> 0.31, 0.5 -> 0.70, 0.9 -> 0.94, 1.0 -> 1.0
            // 이렇게 하면 소총/저격총 등 무기 종류에 상관없이 최댓값을 초과하지 않으면서 찌그러짐을 방지할 수 있습니다.
            float visualApm = Mathf.Sqrt(apmRate);
            float visualAcc = Mathf.Sqrt(accuracy);
            float visualEva = Mathf.Sqrt(evasion);

            if (radarChart != null) radarChart.UpdateValues(visualApm, visualAcc, visualEva);
            
            if (apmLabel != null) apmLabel.text = $"APM\n{rawApm:F0}";
            if (accLabel != null) accLabel.text = $"명중률\n{accuracy:P0}";
            if (evaLabel != null) evaLabel.text = $"회피율\n{evasion:P0}";

            if (logText != null)
            {
                int floor = 1;
                int wave = 1;
                if (RunManager.Instance != null) floor = RunManager.Instance.CurrentFloor;
                if (WaveManager.Instance != null) wave = WaveManager.Instance.CurrentWave;

                string newLog = $"[ {floor}층 - {wave}웨이브 종료 ]\n" +
                                $"- APM: {rawApm:F0}, 명중률: {accuracy:P0}, 회피율: {evasion:P0}\n" +
                                $"- 숙련도: {s:F2}, 이탈도: {c:F2} => 보정치: {alpha:F2}\n" +
                                $"- 다음 웨이브 예산 보정: {baseBudget} ▶ {finalBudget}\n\n";

                logText.text = logText.text + newLog;
                if (logText.text.Length > 2500) logText.text = logText.text.Substring(logText.text.Length - 2500);

                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(ScrollToBottom());
                }
            }
        }
    }
}
