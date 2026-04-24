using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public enum UIState
{
    None, // 게임 플레이 중 (UI 없음)
    Pause, // 일시정지 메뉴
    Upgrade, // 보급 포트 (상점)
    GameOver, // 게임 오버
}

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private PauseUI _pauseUI;
        private GameoverUI _gameoverUI;
        private PlayerUI _playerUI;
        private UpgradeUI _upgradeUI;

        private TMP_FontAsset _font;
        public static UIManager Instance { get; private set; }
        private InputAction _currentEscape;
        private UIState _currentState = UIState.None;
        public UIState CurrentState => _currentState;

        private bool _isInitialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // EventSystem이 없으면 버튼 클릭이 안 되므로 보장
            if (EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
                esGo.transform.parent = gameObject.transform;
            }
        }

        private void Start()
        {
            RequestStateChange(UIState.None);
        }

        public void LoadUI(PlayerInput input)
        {
            if (_isInitialized || input == null)
                return;
            if (_font == null)
            {
                // 프로젝트 기존 한글 폰트 로드 (Galmuri11)
                _font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri9");
                if (_font == null)
                    _font = Resources.Load<TMP_FontAsset>("Fonts/Galmuri11");
            }
            _playerInput = input;
            if (_pauseUI == null)
                _pauseUI = gameObject.AddComponent<PauseUI>();
            _pauseUI.SetFont(_font);
            if (_gameoverUI == null)
                _gameoverUI = gameObject.AddComponent<GameoverUI>();
            _gameoverUI.SetFont(_font);
            if (_playerUI == null)
                _playerUI = gameObject.AddComponent<PlayerUI>();
            _playerUI.SetFont(_font);
            _playerUI.SetPlayerInput(input);
            if (_upgradeUI == null)
                _upgradeUI = gameObject.AddComponent<UpgradeUI>();
            _upgradeUI.SetFont(_font); // UIManager.LoadUI(PlayerInput input) 내부에서...
            _isInitialized = true;
            EnterNewState(UIState.None);
        }

        private void OnEscPerformed(InputAction.CallbackContext context)
        {
            switch (_currentState)
            {
                case UIState.None:
                    // 아무것도 안 열려있을 땐 일시정지 시도
                    RequestStateChange(UIState.Pause);
                    break;

                case UIState.Pause:
                    // 일시정지 중이면 다시 플레이 상태로
                    RequestStateChange(UIState.None);
                    break;

                case UIState.Upgrade:
                    // 상점 중이면 상점 닫기
                    RequestStateChange(UIState.None);
                    break;

                case UIState.GameOver:
                    // 게임오버 시에는 ESC 무시 (혹은 로비 이동)
                    break;
            }
        }

        public void RequestStateChange(UIState newState)
        {
            if (_currentState == newState)
                return;

            // 1. 이전 상태 정리
            ExitCurrentState();

            // 2. 새로운 상태 진입
            _currentState = newState;
            EnterNewState(newState);
        }

        private void ExitCurrentState()
        { // 현재 할당된 ESC 액션이 있다면 구독을 해제합니다.
            if (_currentEscape != null)
            {
                _currentEscape.performed -= OnEscPerformed;
                _currentEscape.Disable(); // 필요시 비활성화
            }
            switch (_currentState)
            {
                case UIState.Pause:
                    _pauseUI.ToggleUI(false);
                    break;
                case UIState.Upgrade:
                    _upgradeUI.ToggleUI(false);
                    break;
                default:
                    break;
            }
        }

        private void EnterNewState(UIState state)
        {
            GameState _gamestate = GameState.Playing;
            string mapName = "Player";
            switch (state)
            {
                case UIState.None:
                    mapName = "Player";
                    _gamestate = GameState.Playing;
                    break;

                case UIState.Pause:
                    mapName = "UI";
                    _gamestate = GameState.Paused;
                    _pauseUI.ToggleUI(true);
                    break;

                case UIState.Upgrade:
                    mapName = "UI";
                    _gamestate = GameState.Paused;
                    _upgradeUI.ToggleUI(true);
                    break;

                case UIState.GameOver:
                    mapName = "UI";
                    _gamestate = GameState.GameOver;
                    _gameoverUI.GameOver();
                    break;
            }
            SetInputMap(mapName);
            GameManager.Instance.ChangeState(_gamestate);
            if (_playerInput != null)
            {
                _currentEscape = _playerInput.currentActionMap.FindAction("Escape");
                if (_currentEscape != null)
                {
                    _currentEscape.Enable();
                    _currentEscape.performed += OnEscPerformed;
                }
            }
        }

        private void SetInputMap(string mapName)
        {
            if (_playerInput == null)
                return;
            _playerInput.SwitchCurrentActionMap(mapName);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
