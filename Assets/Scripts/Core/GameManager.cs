using UnityEngine;
using Player;

namespace Core
{
    public enum ItemTier { Common, Uncommon, Rare, Boss};
    public class GameManager : MonoBehaviour // MCP SYNC TEST - IF YOU SEE THIS, IT WORKS!
    {
        public static GameManager Instance { get; private set; }
        public string projectVersion = "REWRITE-2026-v0.5"; // LIVE SYNC TEST - SHOULD APPEAR IN INSPECTOR
        public PlayerStats Player { get; private set; }


        public enum GameState { MainMenu, Playing, Paused, GameOver }
        public GameState State { get; private set; }

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
        }

        public void ChangeState(GameState newState)
        {
            State = newState;
            // Handle state transitions (e.g., time scale for pause)
            Time.timeScale = (State == GameState.Paused) ? 0f : 1f;
        }
        public void RegisterPlayer(PlayerStats player)
        {
            Player = player;
            Debug.Log("[GameManager] 플레이어가 등록되었습니다.");
        }
    }
}
