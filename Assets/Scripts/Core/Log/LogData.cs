using System;
using System.Collections.Generic;

namespace Log
{
    [Serializable]
    public class WaveLogData
    {
        public string log_id;
        public string user_id;
        public string run_id;
        public string seed;
        public string timestamp;
        public WaveMeta wave_meta;
        public DashboardSummary dashboard_summary;
        public List<TimeSeriesFrame> time_series_frames;
    }

    [Serializable]
    public class WaveMeta
    {
        public string version_id;
        public int floor;
        public int wave;
        public float clear_time_sec;
        public float ai_inferred_S;
        public float ai_inferred_C;
        public float calculated_a;
        public bool fail_safe;
    }

    [Serializable]
    public class DashboardSummary
    {
        public int hits_taken;
        public int apm;
        public float dps;
        public float accuracy_rate;
        public float distance_moved;
        public float hp_retention_rate;
    }

    [Serializable]
    public class TimeSeriesFrame
    {
        public int sec;
        public int atk_clicks_total;
        public int atk_clicks_hit;
        public int enemy_atk_spawned;
        public int hitbox_collisions;
        public int base_dmg_expected;
        public int hp_lost;
        public int max_hp;
        public int enemy_shot_count;
        public float inverse_hit_rate;
        public float hp_retention_rate;
        public float apm;
        public float accuracy;
        public float attack_item_efficiency;
    }

    // ==========================================
    // 1. 데이터 모델 (서버로 보낼 택배 상자들)
    // ==========================================
    [Serializable]
    public class RunEndLogData
    {
        public string log_id;
        public string user_id;
        public string run_id;
        public string timestamp;
        public RunResult run_result;
        public FinalBuild final_build;
    }

    [Serializable]
    public class RunResult
    {
        public string clear_status; // "GAME_OVER" 또는 "CLEAR"
        public int final_floor;
        public int final_wave;
        public int total_play_time_sec;
        public string cause_of_death; // 나를 죽인 적 이름
    }

    [Serializable]
    public class FinalBuild
    {
        public string weapon;
        public List<string> acquired_items; // 먹은 아이템 목록
    }
}
