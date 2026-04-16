using UnityEngine;

namespace Core
{
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        public int CurrentFloor { get; private set; } = 1;
        public int CurrentSeed { get; private set; }
        private Weapons.WeaponData CurrentWeaponData;

        [Header("Economy")]
        [SerializeField] private int bolts = 0;
        public int Bolts => bolts; // 외부에서 읽기 전용

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Auto start run for testing prototype
            StartNewRun();
        }

        public void StartNewRun()
        {
            CurrentSeed = Random.Range(1000, 99999);
            CurrentFloor = 1;
            Random.InitState(CurrentSeed);
            Debug.Log($"[RunManager] New Run Started - Seed: {CurrentSeed}, Floor: {CurrentFloor}");

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.StartFloor(CurrentFloor);
            }
        }

        public void AdvanceFloor()
        {
            if (CurrentFloor < 5)
            {
                CurrentFloor++;
                Debug.Log($"[RunManager] Advanced to Floor {CurrentFloor}");
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.StartFloor(CurrentFloor);
                }
            }
            else
            {
                Debug.Log("[RunManager] Run Cleared!");
                // Game clear logic
            }
        }

        public void SetWeapon(Weapons.WeaponData data)
        {
            CurrentWeaponData = data;
        }

        public Weapons.WeaponData GetWeapon()
        {
            return CurrentWeaponData;
        }

        public bool AddBolts(int amount)
        {
            if (amount > 0)
            {
                bolts += amount;
                Debug.Log($"볼트 획득! 현재 잔액: {bolts}");
                // 여기에 볼트 UI 업데이트 이벤트를 넣으면 좋습니다.
                return true;
            }
            else
            {
                if (bolts >= amount)
                {
                    bolts -= amount;
                    return true;
                }
                return false;
            }
        }
    }
}
