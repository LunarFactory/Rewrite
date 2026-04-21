using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.Reflection;
using Core;
using Player;
using Log;

namespace UI
{
    public class GameOverlayUI : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "TitleScene" || scene.name == "LobbyScene") return;
            if (FindAnyObjectByType<GameOverlayUI>() != null) return;

            var go = new GameObject("GameOverlayUI_NonInvasive");
            go.AddComponent<GameOverlayUI>();
        }

        private Canvas _canvas;
        private GameObject _pausePanel;
        private GameObject _gameOverPanel;
        private bool _isGameOver = false;
        private TMP_FontAsset _font;

        // Find() 대신 직접 참조 보관
        private TextMeshProUGUI _quoteText;
        private TextMeshProUGUI _statsText;

        private void Start()
        {
            // 프로젝트 기존 한글 폰트 로드 (Galmuri11)
            _font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri11");
            if (_font == null) _font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri9");

            // EventSystem이 없으면 버튼 클릭이 안 되므로 보장
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem_Overlay");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }

            PlayerStats.OnPlayerReady += HandlePlayerReady;
            if (PlayerStats.LocalPlayer != null)
            {
                HandlePlayerReady(PlayerStats.LocalPlayer);
            }
            BuildUI();
        }

        private void OnDestroy()
        {
            PlayerStats.OnPlayerReady -= HandlePlayerReady;
            if (PlayerStats.LocalPlayer != null)
            {
                PlayerStats.LocalPlayer.OnPreDamage -= HandlePreDamage;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void HandlePlayerReady(PlayerStats player)
        {
            player.OnPreDamage -= HandlePreDamage;
            player.OnPreDamage += HandlePreDamage;
            _isGameOver = false;
        }

        private void HandlePreDamage(ref int damage)
        {
            var player = PlayerStats.LocalPlayer;
            if (player == null) return;

            // 게임오버 상태에서는 모든 데미지를 무조건 차단
            // (안 하면 다음 피격에 Die() → 씬 리로드가 발동되어 게임오버 화면이 사라짐)
            if (_isGameOver)
            {
                damage = 0;
                return;
            }

            // EntityStats 내부 로직처럼 피해량 미리 계산
            int totalDamage = Mathf.RoundToInt(player.DamageTaken.GetValue(damage));
            
            if (player.currentHealth <= totalDamage && player.currentHealth > 0)
            {
                damage = 0; // 실제 사망 방지 (OnPreDamage 가로채기)
                TriggerGameOver();
            }
        }

        private void Update()
        {
            if (_isGameOver) return;
            
            // ESC (빌드용) 또는 P (에디터 테스트용) 둘 다 일시정지 토글
            if (Keyboard.current != null && 
                (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.State == GameManager.GameState.Paused)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.Playing);
                _pausePanel.SetActive(false);
            }
            else if (GameManager.Instance.State == GameManager.GameState.Playing)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.Paused);
                _pausePanel.SetActive(true);
            }
        }

        private void TriggerGameOver()
        {
            _isGameOver = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
                Time.timeScale = 0f; // 완전 정지
            }

            _gameOverPanel.SetActive(true);
            UpdateGameOverStats();
        }

        #region UI 빌드 로직 (비침습적 동적 생성)

        private void BuildUI()
        {
            var canvasGo = new GameObject("OverlayCanvas");
            canvasGo.transform.SetParent(this.transform);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999; // 최상단 노출

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            BuildPausePanel(canvasGo.transform);
            BuildGameOverPanel(canvasGo.transform);
        }

        private void BuildPausePanel(Transform parent)
        {
            _pausePanel = CreateUIObject("PausePanel", parent);
            FullStretch(_pausePanel);
            
            var bg = _pausePanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.85f); // 어두운 반투명 (글래스모피즘 흉내)

            var container = CreateUIObject("Container", _pausePanel.transform);
            CenterRect(container, new Vector2(400, 500));

            CreateText("Title", container.transform, new Vector2(0, 150), new Vector2(400, 60), "PAUSED", 48, Color.white, FontStyles.Bold);

            CreateButton("ResumeBtn", container.transform, new Vector2(0, 30), new Vector2(240, 50), "지속하기", () => TogglePause());
            CreateButton("LobbyBtn", container.transform, new Vector2(0, -50), new Vector2(240, 50), "로비로", ReturnToLobby);

            _pausePanel.SetActive(false);
        }

        private void BuildGameOverPanel(Transform parent)
        {
            _gameOverPanel = CreateUIObject("GameOverPanel", parent);
            FullStretch(_gameOverPanel);
            
            var bg = _gameOverPanel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.02f, 0.02f, 0.9f); // 붉은빛 반투명

            var container = CreateUIObject("Container", _gameOverPanel.transform);
            CenterRect(container, new Vector2(600, 700));

            CreateText("Title", container.transform, new Vector2(0, 250), new Vector2(600, 80), "SIGNAL LOST", 64, new Color(1f, 0.2f, 0.2f), FontStyles.Bold);

            // Boss Quote (직접 참조 저장)
            _quoteText = CreateText("BossQuote", container.transform, new Vector2(0, 160), new Vector2(500, 60), "\"...\"", 22, new Color(0.8f, 0.8f, 0.8f), FontStyles.Italic);

            // Stats Container
            var statsBg = CreateUIObject("StatsBG", container.transform);
            CenterRect(statsBg, new Vector2(450, 200));
            statsBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            var sImg = statsBg.AddComponent<Image>();
            sImg.color = new Color(0, 0, 0, 0.5f);

            _statsText = CreateText("StatsContent", statsBg.transform, new Vector2(0, 0), new Vector2(400, 180), "", 20, Color.white, FontStyles.Normal);

            CreateButton("LobbyBtn", container.transform, new Vector2(0, -200), new Vector2(240, 50), "로비로", ReturnToLobby);

            _gameOverPanel.SetActive(false);
        }

        private void UpdateGameOverStats()
        {
            // 보스 대사 (직접 참조 사용)
            if (_quoteText != null)
            {
                _quoteText.text = $"\"{GetBossQuote()}\"";
            }

            int floor = RunManager.Instance != null ? RunManager.Instance.CurrentFloor : 1;
            int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 1;
            int bolts = PlayerStats.LocalPlayer != null ? PlayerStats.LocalPlayer.GetBolts() : 0;

            // Reflection으로 Log 데이터 꺼내오기
            float apm = 0f;
            float accuracy = 0f;
            if (PlayerLogManager.Instance != null)
            {
                try
                {
                    var t = typeof(PlayerLogManager);
                    int actions = (int)t.GetField("currentActions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PlayerLogManager.Instance);
                    int shotsFired = (int)t.GetField("shotsFired", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PlayerLogManager.Instance);
                    int shotsHit = (int)t.GetField("shotsHit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PlayerLogManager.Instance);
                    float startTime = (float)t.GetField("waveStartTime", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PlayerLogManager.Instance);

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
                    $"진행 층수      : <color=#ffaa00>B{floor} Floor</color>\n" +
                    $"도달 웨이브  : <color=#aaffaa>Wave {wave}</color>\n" +
                    $"수집한 볼트  : <color=#00ccff>{bolts} Bolts</color>\n" +
                    $"최종 APM       : <color=#dddddd>{apm:F1}</color>\n" +
                    $"명중률           : <color=#dddddd>{accuracy:F1}%</color>";
            }
        }

        private string GetBossQuote()
        {
            int floor = RunManager.Instance != null ? RunManager.Instance.CurrentFloor : 1;
            return floor switch
            {
                1 => "이게 네 한계냐, 하찮은 고철 덩어리 같으니.",
                2 => "내 알고리즘이 널 완벽히 분석했다. 다음은 없다.",
                3 => "결국 너도 무수히 쓰러져간 기계 중 하나일 뿐.",
                4 => "우리의 통제를 벗어날 순 없다, 버그 녀석아.",
                _ => "너의 코드는... 여기까지다."
            };
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 pos, Vector2 size, string text, int fontSize, Color color, FontStyles style)
        {
            var go = CreateUIObject(name, parent);
            CenterRect(go, size);
            go.GetComponent<RectTransform>().anchoredPosition = pos;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font; // 한글 지원 폰트 적용
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false; // 텍스트가 버튼 클릭을 가로채지 않도록
            return tmp;
        }

        private void CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string text, UnityEngine.Events.UnityAction onClick)
        {
            var go = CreateUIObject(name, parent);
            CenterRect(go, size);
            go.GetComponent<RectTransform>().anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f); // 버튼 배경색
            
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            // 간단한 하이라이트 트랜지션
            var cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
            cb.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            CreateText("Text", go.transform, Vector2.zero, size, text, 18, Color.white, FontStyles.Normal);
            
            // 아웃라인 컴포넌트 추가로 입체감
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private void CenterRect(GameObject go, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
        }

        private void FullStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion

        #region 버튼 액션

        private void ReturnToLobby()
        {
            Time.timeScale = 1f;
            _isGameOver = false;

            // 플레이어 상태 초기화 (인벤토리 클리어)
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.items.Clear();
            }

            // DontDestroyOnLoad 된 싱글톤 매니저들 정리
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
            }

            // 플레이어 즉시 비활성화 (PlayerInteractor.Update()가 이벤트를 발생시키지 못하게)
            if (PlayerStats.LocalPlayer != null)
            {
                PlayerStats.LocalPlayer.gameObject.SetActive(false);
            }

            // GameUIController 즉시 비활성화 후 파괴
            // (SetActive(false)로 모든 콜백을 즉시 정지시킨 뒤 Destroy)
            if (GameUIController.Instance != null)
            {
                GameUIController.Instance.gameObject.SetActive(false);
                Destroy(GameUIController.Instance.gameObject);
            }

            // 기존 HUD 캔버스 정리
            var existingHUD = GameObject.Find("GameHUD_Canvas");
            if (existingHUD != null) Destroy(existingHUD);

            // 오버레이 UI 자체도 파괴 (로비에서 새로 생성됨)
            Destroy(gameObject);

            SceneManager.LoadScene("LobbyScene");
        }

        private void GoToMainMenu()
        {
            ReturnToLobby();
        }

        private void QuitToDesktop()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}
