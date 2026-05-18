using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PauseUI : OverlayUI
    {
        public static PauseUI Instance { get; private set; }
        private GameObject _canvasGo;
        private GameObject _pausePanel;

        protected void Start()
        {
            _canvasGo = BuildOrFindUI();
            BuildPausePanel(_canvasGo.transform);
        }

        public void ToggleUI(bool status)
        {
            _pausePanel.SetActive(status);
        }

        protected void BuildPausePanel(Transform parent)
        {
            _pausePanel = CreateUIObject("PausePanel", parent);
            FullStretch(_pausePanel);

            var bg = _pausePanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.85f); // 어두운 반투명 (글래스모피즘 흉내)

            var container = CreateUIObject("Container", _pausePanel.transform);
            CenterRect(container, new Vector2(400, 500));

            CreateText(
                "Title",
                container.transform,
                new Vector2(0, 150),
                new Vector2(400, 60),
                "PAUSED",
                48,
                Color.white,
                FontStyles.Bold
            );

            CreateButton(
                "ResumeBtn",
                container.transform,
                new Vector2(0, 30),
                new Vector2(240, 50),
                "계속하기",
                () => UIManager.Instance.RequestStateChange(UIState.None)
            );
            CreateButton(
                "LobbyBtn",
                container.transform,
                new Vector2(0, -50),
                new Vector2(240, 50),
                "로비로",
                ReturnToLobby
            );

            // --- DDA Analytics Dashboard 부착 ---
            GameObject ddaPanelGo = CreateUIObject("DDA_Analytics", _pausePanel.transform);
            var rt = ddaPanelGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            // 화면 우측 여백을 둡니다
            rt.anchoredPosition = new Vector2(-100, 0); 
            rt.sizeDelta = new Vector2(450, 600); 

            ddaPanelGo.AddComponent<UI.DDA.DDAAnalyticsPanel>();

            _pausePanel.SetActive(false);
        }
    }
}
