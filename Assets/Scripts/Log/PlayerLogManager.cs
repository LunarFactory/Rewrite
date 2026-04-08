using UnityEngine;

namespace Log
{
    public class PlayerLogManager : MonoBehaviour
    {
        public static PlayerLogManager Instance { get; private set; }

        // Data tracked per wave for AI estimation (APM, Accuracy, etc)
        private int currentActions;
        private int shotsFired;
        private int shotsHit;
        private float waveStartTime;
        
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

        public void OnWaveStarted(int wave)
        {
            currentActions = 0;
            shotsFired = 0;
            shotsHit = 0;
            waveStartTime = Time.time;
        }

        public void OnWaveCompleted(int wave)
        {
            float duration = Time.time - waveStartTime;
            if (duration <= 0) duration = 1f;

            float apm = (currentActions / duration) * 60f;
            float accuracy = shotsFired > 0 ? (float)shotsHit / shotsFired : 0f;

            Debug.Log($"[PlayerLogManager] Wave {wave} Ended. Time: {duration}s, APM: {apm:F1}, Accuracy: {accuracy:P1}");
            // Eventual integration point to send to backend ML system
        }

        public void RecordAction() => currentActions++;
        public void RecordShotFired() => shotsFired++;
        public void RecordShotHit() => shotsHit++;
    }
}
