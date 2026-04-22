using Player;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public enum ItemTier
    {
        Common,
        Uncommon,
        Rare,
        Boss,
    };

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
    };

    public class GameManager : MonoBehaviour // MCP SYNC TEST - IF YOU SEE THIS, IT WORKS!
    {
        public string projectVersion = "REWRITE-2026-v0.5"; // LIVE SYNC TEST - SHOULD APPEAR IN INSPECTOR
        public static GameManager Instance { get; private set; }
        private GameState State;
        private PlayerInput playerInputSystem;

        public bool isGameOver = false;

        public Sprite crosshairSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            State = GameState.Playing; // Default for now
            gameObject.AddComponent<UIManager>();
        }

        private void Start()
        {
            playerInputSystem = FindAnyObjectByType<PlayerStats>().GetComponent<PlayerInput>();
            var ui = gameObject.GetComponent<UIManager>();
            ui.crosshairSprite = crosshairSprite;
            ui.LoadUI(playerInputSystem);
        }

        public void ChangeState(GameState newState)
        {
            State = newState;
            // Handle state transitions (e.g., time scale for pause)
            Time.timeScale = (State == GameState.Paused) ? 0f : 1f;
        }
    }
}
