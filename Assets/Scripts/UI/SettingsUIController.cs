using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 환경설정 패널 컨트롤러.
    /// 볼륨/해상도/전체화면/수직동기화 설정을 버튼 클릭으로 순환하며 PlayerPrefs에 저장합니다.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        // ── PlayerPrefs 키 ──────────────────────────────────────────
        private const string KeyMasterVol  = "Vol_Master";
        private const string KeyBgmVol     = "Vol_BGM";
        private const string KeySfxVol     = "Vol_SFX";
        private const string KeyResolution  = "Resolution";
        private const string KeyFullscreen  = "Fullscreen";
        private const string KeyVSync       = "VSync";

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
        private static readonly int[]    VolSteps       = { 0, 20, 40, 60, 80, 100 };
        private static readonly string[] ResolutionOptions = { "1280×720", "1600×900", "1920×1080", "2560×1440" };

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
            _masterVolIdx  = PlayerPrefs.GetInt(KeyMasterVol, 5);   // 기본 100%
            _bgmVolIdx     = PlayerPrefs.GetInt(KeyBgmVol,    4);   // 기본 80%
            _sfxVolIdx     = PlayerPrefs.GetInt(KeySfxVol,    4);   // 기본 80%
            _resolutionIdx = PlayerPrefs.GetInt(KeyResolution, 2);  // 기본 1920×1080
            _fullscreen    = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
            _vsync         = PlayerPrefs.GetInt(KeyVSync, 1) == 1;
        }

        private void SaveAndClose()
        {
            PlayerPrefs.SetInt(KeyMasterVol,  _masterVolIdx);
            PlayerPrefs.SetInt(KeyBgmVol,     _bgmVolIdx);
            PlayerPrefs.SetInt(KeySfxVol,     _sfxVolIdx);
            PlayerPrefs.SetInt(KeyResolution,  _resolutionIdx);
            PlayerPrefs.SetInt(KeyFullscreen,  _fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KeyVSync,       _vsync ? 1 : 0);
            PlayerPrefs.Save();

            ApplySettings();

            // 뒤로가기: LobbyController가 처리
            Hide();
        }

        private void ApplySettings()
        {
            // 오디오
            AudioListener.volume = VolSteps[_masterVolIdx] / 100f;

            // 전체화면
            Screen.fullScreen = _fullscreen;

            // 수직동기화
            QualitySettings.vSyncCount = _vsync ? 1 : 0;

            // 해상도
            string res = ResolutionOptions[_resolutionIdx];
            var parts = res.Split('×');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int w) &&
                int.TryParse(parts[1], out int h))
            {
                Screen.SetResolution(w, h, _fullscreen);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  순환 로직
        // ─────────────────────────────────────────────────────────────

        private void CycleMasterVol()
        {
            _masterVolIdx = (_masterVolIdx + 1) % VolSteps.Length;
            RefreshBtn(masterVolBtn, $"{VolSteps[_masterVolIdx]}%");
        }

        private void CycleBgmVol()
        {
            _bgmVolIdx = (_bgmVolIdx + 1) % VolSteps.Length;
            RefreshBtn(bgmVolBtn, $"{VolSteps[_bgmVolIdx]}%");
        }

        private void CycleSfxVol()
        {
            _sfxVolIdx = (_sfxVolIdx + 1) % VolSteps.Length;
            RefreshBtn(sfxVolBtn, $"{VolSteps[_sfxVolIdx]}%");
        }

        private void CycleResolution()
        {
            _resolutionIdx = (_resolutionIdx + 1) % ResolutionOptions.Length;
            RefreshBtn(resolutionBtn, ResolutionOptions[_resolutionIdx]);
        }

        private void ToggleFullscreen()
        {
            _fullscreen = !_fullscreen;
            RefreshBtn(fullscreenBtn, _fullscreen ? "켜짐" : "꺼짐");
        }

        private void ToggleVSync()
        {
            _vsync = !_vsync;
            RefreshBtn(vsyncBtn, _vsync ? "켜짐" : "꺼짐");
        }

        private void RefreshAllUI()
        {
            RefreshBtn(masterVolBtn,  $"{VolSteps[_masterVolIdx]}%");
            RefreshBtn(bgmVolBtn,     $"{VolSteps[_bgmVolIdx]}%");
            RefreshBtn(sfxVolBtn,     $"{VolSteps[_sfxVolIdx]}%");
            RefreshBtn(resolutionBtn,  ResolutionOptions[_resolutionIdx]);
            RefreshBtn(fullscreenBtn,  _fullscreen ? "켜짐" : "꺼짐");
            RefreshBtn(vsyncBtn,       _vsync ? "켜짐" : "꺼짐");
        }

        // 버튼의 MainText를 갱신
        private static void RefreshBtn(Button btn, string text)
        {
            if (btn == null) return;
            var mt = btn.transform.Find("MainText")?.GetComponent<Text>();
            if (mt != null) mt.text = text;
        }
    }
}
