using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 환경설정 패널 컨트롤러.
    /// 볼륨/해상도/전체화면/수직동기화 설정을 버튼 클릭으로 순환하며 PlayerPrefs에 저장합니다.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        // ── PlayerPrefs 키 ──────────────────────────────────────────
        private const string KeyMasterVol = "Vol_Master";
        private const string KeyBgmVol = "Vol_BGM";
        private const string KeySfxVol = "Vol_SFX";
        private const string KeyResolution = "Resolution";
        private const string KeyFullscreen = "Fullscreen";
        private const string KeyVSync = "VSync";

        // ── 버튼 참조 ───────────────────────────────────────────────
        [Header("오디오 버튼")]
        public Button masterVolBtn;
        public Button bgmVolBtn;
        public Button sfxVolBtn;

        [Header("화면 버튼")]
        public Button resolutionBtn;
        public Button fullscreenBtn;
        public Button vsyncBtn;

        [Header("패널 제어")]
        public Button backButton;

        // ── 내부 상태 ────────────────────────────────────────────────
        private static readonly int[] VolSteps = { 0, 20, 40, 60, 80, 100 };
        private static readonly string[] ResolutionOptions =
        {
            "1280×720",
            "1600×900",
            "1920×1080",
            "2560×1440",
        };

        private int _masterVolIdx;
        private int _bgmVolIdx;
        private int _sfxVolIdx;
        private int _resolutionIdx;
        private bool _fullscreen;
        private bool _vsync;

        // ─────────────────────────────────────────────────────────────
        //  초기화
        // ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            masterVolBtn?.onClick.AddListener(CycleMasterVol);
            bgmVolBtn?.onClick.AddListener(CycleBgmVol);
            sfxVolBtn?.onClick.AddListener(CycleSfxVol);
            resolutionBtn?.onClick.AddListener(CycleResolution);
            fullscreenBtn?.onClick.AddListener(ToggleFullscreen);
            vsyncBtn?.onClick.AddListener(ToggleVSync);
            backButton?.onClick.AddListener(SaveAndClose);
        }

        private void Start()
        {
            // 게임 시작 시 저장된 설정을 불러와서 즉시 적용
            LoadPrefs();
            ApplySettings();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LoadPrefs();
            RefreshAllUI();
        }

        public void Hide() => gameObject.SetActive(false);

        // ─────────────────────────────────────────────────────────────
        //  로드 / 저장
        // ─────────────────────────────────────────────────────────────

        private void LoadPrefs()
        {
            _masterVolIdx = PlayerPrefs.GetInt(KeyMasterVol, 5); // 기본 100%
            _bgmVolIdx = PlayerPrefs.GetInt(KeyBgmVol, 4); // 기본 80%
            _sfxVolIdx = PlayerPrefs.GetInt(KeySfxVol, 4); // 기본 80%
            _resolutionIdx = PlayerPrefs.GetInt(KeyResolution, 2); // 기본 1920×1080
            _fullscreen = PlayerPrefs.GetInt(KeyFullscreen, 0) == 1; // 기본값 0 (창 모드)으로 변경
            _vsync = PlayerPrefs.GetInt(KeyVSync, 1) == 1;
        }

        private void SaveAndClose()
        {
            PlayerPrefs.SetInt(KeyMasterVol, _masterVolIdx);
            PlayerPrefs.SetInt(KeyBgmVol, _bgmVolIdx);
            PlayerPrefs.SetInt(KeySfxVol, _sfxVolIdx);
            PlayerPrefs.SetInt(KeyResolution, _resolutionIdx);
            PlayerPrefs.SetInt(KeyFullscreen, _fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KeyVSync, _vsync ? 1 : 0);
            PlayerPrefs.Save();

            ApplySettings();
            Debug.Log("설정이 저장되고 적용되었습니다.");
            // 뒤로가기: LobbyController가 처리
            Hide();
        }

        private void ApplySettings()
        {
            // 오디오 (현재 소리가 없으므로 주석 처리)
            // AudioListener.volume = VolSteps[_masterVolIdx] / 100f;

            // 수직동기화
            QualitySettings.vSyncCount = _vsync ? 1 : 0;

            // 해상도 및 전체화면 모드 결정
            string res = ResolutionOptions[_resolutionIdx];
            // '×' (U+00D7) 또는 일반 'x' 모두 대응
            char separator = res.Contains("×") ? '×' : 'x';
            var parts = res.Split(separator);

            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            {
                // FullScreenMode를 명시적으로 지정 (전체화면 창모드 vs 일반 창모드)
                FullScreenMode mode = _fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                Screen.SetResolution(w, h, mode);
                
                Debug.Log($"[Settings] 적용됨 - 해상도: {w}x{h}, 모드: {mode}, 수직동기화: {_vsync}");
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  순환 로직
        // ─────────────────────────────────────────────────────────────

        private void CycleMasterVol()
        {
            _masterVolIdx = (_masterVolIdx + 1) % VolSteps.Length;
            string val = $"{VolSteps[_masterVolIdx]}%";
            RefreshBtn(masterVolBtn, val);
            Debug.Log($"Master Volume 변경: {val}");
        }

        private void CycleBgmVol()
        {
            _bgmVolIdx = (_bgmVolIdx + 1) % VolSteps.Length;
            string val = $"{VolSteps[_bgmVolIdx]}%";
            RefreshBtn(bgmVolBtn, val);
            Debug.Log($"BGM Volume 변경: {val}");
        }

        private void CycleSfxVol()
        {
            _sfxVolIdx = (_sfxVolIdx + 1) % VolSteps.Length;
            string val = $"{VolSteps[_sfxVolIdx]}%";
            RefreshBtn(sfxVolBtn, val);
            Debug.Log($"SFX Volume 변경: {val}");
        }

        private void CycleResolution()
        {
            _resolutionIdx = (_resolutionIdx + 1) % ResolutionOptions.Length;
            string res = ResolutionOptions[_resolutionIdx];
            RefreshBtn(resolutionBtn, res);
            Debug.Log($"해상도 변경: {res}");
        }

        private void ToggleFullscreen()
        {
            _fullscreen = !_fullscreen;
            string val = _fullscreen ? "켜짐" : "꺼짐";
            RefreshBtn(fullscreenBtn, val);
            Debug.Log($"전체화면 모드: {val}");
        }

        private void ToggleVSync()
        {
            _vsync = !_vsync;
            string val = _vsync ? "켜짐" : "꺼짐";
            RefreshBtn(vsyncBtn, val);
            Debug.Log($"수직동기화: {val}");
        }

        private void RefreshAllUI()
        {
            RefreshBtn(masterVolBtn, $"{VolSteps[_masterVolIdx]}%");
            RefreshBtn(bgmVolBtn, $"{VolSteps[_bgmVolIdx]}%");
            RefreshBtn(sfxVolBtn, $"{VolSteps[_sfxVolIdx]}%");
            RefreshBtn(resolutionBtn, ResolutionOptions[_resolutionIdx]);
            RefreshBtn(fullscreenBtn, _fullscreen ? "켜짐" : "꺼짐");
            RefreshBtn(vsyncBtn, _vsync ? "켜짐" : "꺼짐");
        }

        // 버튼의 MainText를 갱신
        private static void RefreshBtn(Button btn, string text)
        {
            if (btn == null)
                return;

            var transform = btn.transform.Find("MainText");
            if (transform == null) return;

            // 1. TextMeshProUGUI 시도
            var tmp = transform.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                return;
            }

            // 2. 레거시 Text 시도
            var legacyText = transform.GetComponent<Text>();
            if (legacyText != null)
            {
                legacyText.text = text;
            }
        }
    }
}
