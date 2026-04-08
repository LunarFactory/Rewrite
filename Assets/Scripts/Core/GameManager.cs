using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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
    }
}
