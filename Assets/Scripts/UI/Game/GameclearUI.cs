using System.Reflection;
using Core;
using Log;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameclearUI : OverlayUI
    {
        public static GameclearUI Instance { get; private set; }
        private GameObject _canvasGo;
        private GameObject _gameClearPanel;

        // Find() 대신 직접 참조 보관
        private TextMeshProUGUI _quoteText;
        private TextMeshProUGUI _statsText;

        protected void Start()
        {
            _canvasGo = BuildOrFindUI();
            BuildGameClearPanel(_canvasGo.transform);
        }

        public void ToggleUI(bool status)
        {
            _gameClearPanel.SetActive(status);
        }

        public void GameClear()
        {
            TriggerGameClear();
        }

        private void TriggerGameClear()
        {
            UpdateGameClearStats();
            _gameClearPanel.SetActive(true);
        }

        private void BuildGameClearPanel(Transform parent)
        {
            _gameClearPanel = CreateUIObject("GameClearPanel", parent);
            FullStretch(_gameClearPanel);

            var bg = _gameClearPanel.AddComponent<Image>();
            bg.color = new Color(0.0f, 0.00f, 0.00f, 0.9f); // 붉은빛 반투명

            var container = CreateUIObject("Container", _gameClearPanel.transform);
            CenterRect(container, new Vector2(600, 700));

            CreateText(
                "Title",
                container.transform,
                new Vector2(0, 250),
                new Vector2(600, 80),
                "REWROTE",
                64,
                new Color(0f, 0f, 0.1f),
                FontStyles.Bold
            );

            // Boss Quote (직접 참조 저장)
            _quoteText = CreateText(
                "BossQuote",
                container.transform,
                new Vector2(0, 160),
                new Vector2(500, 60),
                "\"...\"",
                22,
                new Color(0.8f, 0.8f, 0.8f),
                FontStyles.Italic
            );

            // Stats Container
            var statsBg = CreateUIObject("StatsBG", container.transform);
            CenterRect(statsBg, new Vector2(450, 200));
            statsBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            var sImg = statsBg.AddComponent<Image>();
            sImg.color = new Color(0, 0, 0, 0.5f);

            _statsText = CreateText(
                "StatsContent",
                statsBg.transform,
                new Vector2(0, 0),
                new Vector2(400, 180),
                "",
                20,
                Color.white,
                FontStyles.Normal
            );

            CreateButton(
                "LobbyBtn",
                container.transform,
                new Vector2(0, -200),
                new Vector2(240, 50),
                "로비로",
                ReturnToLobby
            );

            _gameClearPanel.SetActive(false);
        }

        private void UpdateGameClearStats()
        {
            // 보스 대사 (직접 참조 사용)
            if (_quoteText != null)
            {
                _quoteText.text = $"\"{GetBossQuote()}\"";
            }

            int floor = RunManager.Instance != null ? RunManager.Instance.CurrentFloor : 1;
            int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 1;
            int bolts = PlayerStats.LocalPlayer != null ? PlayerStats.LocalPlayer.GetBolts() : 0;

            // 층별 크레딧 차등 지급 (1층:50, 2층:60, 3층:70, 4층:80, 5층+:100)
            int earnedCredits = GetCreditsByFloor(floor);
            int currentCredits = PlayerPrefs.GetInt("LobbyCredits", 0);
            PlayerPrefs.SetInt("LobbyCredits", currentCredits + earnedCredits);
            PlayerPrefs.Save();

            // Reflection으로 Log 데이터 꺼내오기
            float apm = 0f;
            float accuracy = 0f;
            if (LogTracker.Instance != null)
            {
                try
                {
                    var t = typeof(LogTracker);
                    int actions = (int)
                        t.GetField("currentActions", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(LogTracker.Instance);
                    int shotsFired = (int)
                        t.GetField("shotsFired", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(LogTracker.Instance);
                    int shotsHit = (int)
                        t.GetField("shotsHit", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(LogTracker.Instance);
                    float startTime = (float)
                        t.GetField("waveStartTime", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(LogTracker.Instance);

                    float duration = Mathf.Max(Time.time - startTime, 1f);
                    apm = (actions / duration) * 60f;
                    accuracy = shotsFired > 0 ? ((float)shotsHit / shotsFired) * 100f : 0f;
                }
                catch { }
            }

            // 통계 텍스트 (직접 참조 사용)
            if (_statsText != null)
            {
                _statsText.alignment = TextAlignmentOptions.TopLeft;
                _statsText.lineSpacing = 15f;
                _statsText.text =
                    $"진행 층 수 : <color=#ffaa00>B{floor} Floor</color>\n"
                    + $"도달 웨이브 : <color=#aaffaa>Wave {wave}</color>\n"
                    + $"수집한 볼트 : <color=#00ccff>{bolts} Bolts</color>\n"
                    + $"최종 APM : <color=#dddddd>{apm:F1}</color>\n"
                    + $"명중률 : <color=#dddddd>{accuracy:F1}%</color>\n\n"
                    + $"<color=#ffd700>+ {earnedCredits} 크레딧 획득!</color>";
            }
        }

        private int GetCreditsByFloor(int floor)
        {
            return floor switch
            {
                1 => 50,
                2 => 60,
                3 => 70,
                4 => 80,
                _ => 100,
            };
        }

        private string GetBossQuote()
        {
            return "운명을 다시 쓰다";
        }
    }
}
