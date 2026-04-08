using UnityEngine;

namespace Core
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public enum WaveType { Mob, Shop, Rest, Boss }
        [field: SerializeField] public int CurrentWave { get; private set; } = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartFloor(int floorNumber)
        {
            CurrentWave = 1;
            StartWave(CurrentWave);
        }

        public void StartWave(int waveNumber)
        {
            CurrentWave = waveNumber;
            WaveType type = GetWaveType(CurrentWave);

            Debug.Log($"[WaveManager] Wave {CurrentWave} Started - Type: {type}");
            
            Log.PlayerLogManager.Instance?.OnWaveStarted(CurrentWave);
            
            switch (type)
            {
                case WaveType.Mob:
                    for (int i = 0; i < 3 + RunManager.Instance.CurrentFloor; i++)
                    {
                        if (Level.TestSetup.EnemyPrefab != null)
                        {
                            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * 8f;
                            Instantiate(Level.TestSetup.EnemyPrefab, randomPos, Quaternion.identity).SetActive(true);
                        }
                    }
                    Invoke(nameof(TestCompleteWave), 10f); // 10 seconds for test
                    break;
                case WaveType.Shop:
                    Invoke(nameof(TestCompleteWave), 1f);
                    break;
                case WaveType.Rest:
                    Invoke(nameof(TestCompleteWave), 1f);
                    break;
                case WaveType.Boss:
                    Invoke(nameof(TestCompleteWave), 5f);
                    break;
            }
        }

        private void TestCompleteWave()
        {
            CompleteCurrentWave();
        }

        public void CompleteCurrentWave()
        {
            Debug.Log($"[WaveManager] Wave {CurrentWave} Completed.");
            
            Log.PlayerLogManager.Instance?.OnWaveCompleted(CurrentWave);
            
            if (CurrentWave < 9)
            {
                StartWave(CurrentWave + 1);
            }
            else
            {
                RunManager.Instance.AdvanceFloor();
            }
        }

        private WaveType GetWaveType(int wave)
        {
            if (wave == 4) return WaveType.Shop;
            if (wave == 8) return WaveType.Rest;
            if (wave == 9) return WaveType.Boss;
            return WaveType.Mob; 
        }
    }
}
