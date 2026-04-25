using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core;
using Player; // PlayerStats가 있는 네임스페이스
using UnityEngine;
using UnityEngine.Networking;

namespace Log
{
    public class LogTracker : MonoBehaviour
    {
        public static LogTracker Instance { get; private set; }

        private string run_id;

        private string user_id;

        [Header("Tracking State")]
        private bool _isTracking = false;
        private float _startTime;
        private Vector2 _lastPosition;
        private float _totalDistance;

        // 실시간 프레임 데이터 보관
        private WaveLogData _currentLog;
        private TimeSeriesFrame _currentFrame;

        // 카운터들
        private int _damageDealt;
        private int _hitsTaken;
        private int _enemyShot;
        private int _totalClicks;
        private int _totalAttackClicks;
        private int _totalHits;
        private int _totalDamageDealt;
        private int _totalHitsTaken;
        private int _startHp;

        private string _baseURL;

        private string _ip = "15.164.165.212";

        // 인터넷이 끊겼을 때 임시 보관할 큐
        private Queue<string> _localRunEndQueue = new Queue<string>();
        private Queue<string> _localLogQueue = new Queue<string>();
        private bool _isRetrying = false;

        private void Awake()
        {
            Instance = this;
            user_id = PlayerPrefs.GetString("UserId", "Guest");
            _baseURL = "http://" + _ip + ":8080/api/v1/player/game/logs/";
        }

        // 웨이브 시작 시 호출
        public void StartLogging(int floor, int wave, string seed)
        {
            _isTracking = true;
            _startTime = Time.time;
            _lastPosition = transform.position;
            _damageDealt = 0;
            _hitsTaken = 0;
            _enemyShot = 0;
            _totalDistance = 0;
            _totalClicks = _totalHits = _totalHitsTaken = 0;
            _totalDamageDealt = 0;
            _startHp = PlayerStats.LocalPlayer.currentHealth;

            _currentLog = new WaveLogData
            {
                log_id = Guid.NewGuid().ToString(),
                user_id = user_id, // 실제 ID 연동 필요
                run_id = run_id,
                seed = seed,
                wave_meta = new WaveMeta { floor = floor, wave = wave },
                time_series_frames = new List<TimeSeriesFrame>(),
            };

            StartCoroutine(TimeSeriesRecordingRoutine());
        }

        // 매 초마다 스냅샷 기록
        private IEnumerator TimeSeriesRecordingRoutine()
        {
            while (_isTracking)
            {
                yield return new WaitForSeconds(1.0f);

                var frame = new TimeSeriesFrame
                {
                    sec = Mathf.RoundToInt(Time.time - _startTime),
                    atk_clicks_total = _totalAttackClicks, // 해당 초의 누적값 또는 증분값
                    atk_clicks_hit = _totalHits,
                    enemy_atk_spawned = _enemyShot,
                    hitbox_collisions = _hitsTaken,
                    actual_dmg_dealt = _damageDealt,
                    hp_lost = _startHp - PlayerStats.LocalPlayer.currentHealth,
                    max_hp = PlayerStats.LocalPlayer.maxHealth,
                    apm = Mathf.RoundToInt(_totalClicks / ((Time.time - _startTime) / 60f)),
                };
                _damageDealt = 0;
                _hitsTaken = 0;

                _currentLog.time_series_frames.Add(frame);
            }
        }

        public void GenerateRunId()
        {
            run_id = Guid.NewGuid().ToString();
        }

        // 각종 이벤트 기록용 퍼블릭 메서드 (다른 스크립트에서 호출)
        public void RegisterClick() => _totalClicks++;

        public void RegisterAttackClick() => _totalAttackClicks++;

        public void RegisterHit(int damage)
        {
            _totalHits++;
            _totalDamageDealt += damage;
            _damageDealt += damage;
        }

        public void RegisterHitTaken()
        {
            _totalHitsTaken++;
            _hitsTaken++;
        }

        public void RegisterEnemyShot()
        {
            _enemyShot++;
        }

        // 웨이브 종료 시 최종 로그 반환
        public WaveLogData CompleteLogging(
            float alpha = 0.5f,
            float inferredS = 0.5f,
            float inferredC = 0.5f
        )
        {
            _isTracking = false;
            float clearTime = Time.time - _startTime;

            // 요약 데이터 계산
            _currentLog.wave_meta.clear_time_sec = clearTime;
            _currentLog.wave_meta.calculated_a = alpha;
            _currentLog.wave_meta.ai_inferred_S = inferredS;
            _currentLog.wave_meta.ai_inferred_C = inferredC;
            _currentLog.timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            _currentLog.dashboard_summary = new DashboardSummary
            {
                hits_taken = _totalHitsTaken,
                apm = Mathf.RoundToInt(_totalClicks / (clearTime / 60f)),
                dps = _totalDamageDealt / clearTime,
                accuracy_rate = _totalAttackClicks > 0 ? _totalAttackClicks / (float)_totalHits : 0,
                distance_moved = _totalDistance,
                hp_retention_rate = PlayerStats.LocalPlayer.currentHealth / _startHp,
            };

            return _currentLog;
        }

        private void Update()
        {
            if (!_isTracking)
                return;
            _totalDistance += Vector2.Distance(transform.position, _lastPosition);
            _lastPosition = transform.position;
        }

        /// <summary>
        /// 게임이 끝났을 때(사망/클리어) 호출하는 메인 함수
        /// </summary>
        public void OnRunEnded(string status, string killerName = "none")
        {
            // 1. 현재 게임 상태를 긁어와서 데이터 조립
            RunEndLogData logData = CollectActualRunData(status, killerName);

            // 2. 서버가 읽을 수 있게 JSON 문자열로 변환
            string jsonData = JsonUtility.ToJson(logData, true);
            SaveLogToFile(jsonData, "RunLog");

            // 3. 서버로 전송!
            StartCoroutine(PostRunEndLog(jsonData));
        }

        private RunEndLogData CollectActualRunData(string status, string killerName)
        {
            // 인벤토리에서 먹은 아이템 리스트 뽑아오기 (예시)
            List<string> itemIds = new List<string>();
            if (InventoryManager.Instance != null)
            {
                foreach (var item in InventoryManager.Instance.items)
                {
                    itemIds.Add(item.itemName);
                }
            }

            // 최종 성적표 작성
            return new RunEndLogData
            {
                log_id = Guid.NewGuid().ToString(),
                user_id = user_id, // 연동된 유저 ID
                run_id = run_id, // 이번 게임의 고유 ID
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),

                run_result = new RunResult
                {
                    clear_status = status,
                    final_floor = RunManager.Instance.CurrentFloor, // GameManager.Instance.CurrentFloor 등으로 변경
                    final_wave = WaveManager.Instance.CurrentWave, // GameManager.Instance.CurrentWave 등으로 변경
                    total_play_time_sec = Mathf.RoundToInt(Time.time - _startTime), // 게임 한 판 전체 플레이 시간
                    cause_of_death = killerName,
                },

                final_build = new FinalBuild
                {
                    weapon = PlayerStats.LocalPlayer.GetWeaponData().weaponName,
                    acquired_items = itemIds,
                },
            };
        }

        private IEnumerator PostRunEndLog(string jsonData)
        {
            using (UnityWebRequest request = new UnityWebRequest(_baseURL + "run-end", "POST"))
            {
                if (PlayerPrefs.HasKey("AuthToken"))
                {
                    string token = PlayerPrefs.GetString("AuthToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        // 표준 Bearer 토큰 형식으로 전달
                        request.SetRequestHeader("Authorization", "Bearer " + token);
                    }
                }
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("[RunEndLogger] 전송 실패, 로컬 큐에 임시 저장합니다.");
                    _localRunEndQueue.Enqueue(jsonData);
                }
                else
                {
                    Debug.Log("[RunEndLogger] 런 종료 로그 전송 성공!");
                }
            }
        }

        // 웨이브 종료 시 호출됨
        public void SendWaveLog(WaveLogData actualData)
        {
            string jsonData = JsonUtility.ToJson(actualData);
            StartCoroutine(PostWaveLog(jsonData));
        }

        private IEnumerator PostWaveLog(string jsonData)
        {
            using (UnityWebRequest request = new UnityWebRequest(_baseURL + "wave", "POST"))
            {
                if (PlayerPrefs.HasKey("AuthToken"))
                {
                    string token = PlayerPrefs.GetString("AuthToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        // 표준 Bearer 토큰 형식으로 전달
                        request.SetRequestHeader("Authorization", "Bearer " + token);
                    }
                }
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[DataLogger] 전송 실패, 큐에 저장: {request.error}");
                    _localLogQueue.Enqueue(jsonData);
                }
                else
                {
                    Debug.Log("[DataLogger] 로그 전송 성공");
                    // 성공 시 대기 중인 큐 처리 시도
                    if (_localLogQueue.Count > 0 && !_isRetrying)
                    {
                        StartCoroutine(RetryQueuedLogs());
                    }
                }
            }
        }

        private IEnumerator RetryQueuedLogs()
        {
            _isRetrying = true;
            while (_localLogQueue.Count > 0)
            {
                string nextLog = _localLogQueue.Peek();
                yield return StartCoroutine(PostWaveLog(nextLog));

                // 전송 성공 시에만 큐에서 제거 (PostWaveLog 내부 로직에 따라 조절)
                _localLogQueue.Dequeue();
                yield return new WaitForSeconds(1.0f); // 서버 부하 방지
            }
            _isRetrying = false;
        }

        public void EndWaveAndSend(float alpha, float s, float c)
        {
            // 1. 데이터 조립 (기존 CompleteLogging 호출)
            WaveLogData waveData = CompleteLogging(alpha, s, c);

            // 2. [추가] 로컬 파일로 저장 (눈으로 확인용)
            string jsonData = JsonUtility.ToJson(waveData, true);
            SaveLogToFile(jsonData, "WaveLog");

            // 3. [핵심] 서버로 전송 시도
            SendWaveLog(waveData);

            Debug.Log(
                $"[LogTracker] 웨이브 {waveData.wave_meta.wave} 로그 조립 및 전송 시퀀스 시작"
            );
        }

        private void SaveLogToFile(string jsonData, string prefix)
        {
            string fileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                File.WriteAllText(path, jsonData);
                Debug.Log($"<color=cyan>[Export]</color> {prefix} 저장 완료: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"파일 저장 실패: {e.Message}");
            }
        }
    }
}
