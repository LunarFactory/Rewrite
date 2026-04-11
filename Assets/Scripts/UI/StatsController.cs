using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 통계 패널 컨트롤러.
    /// 현재는 시안(placeholder) 상태이며, 서버 로그 연동 후 <see cref="LoadStats"/>를 구현합니다.
    /// </summary>
    public class StatsController : MonoBehaviour
    {
        [Header("통계 수치 (Value Text)")]
        public Text totalGamesValue;
        public Text winsValue;
        public Text lossesValue;
        public Text winRateValue;
        public Text bestKillValue;
        public Text avgSurvivalValue;
        public Text longestStreakValue;

        [Header("패널 제어")]
        public Button backButton;

        /// <summary>LobbyController에서 패널 활성화 시 호출됩니다.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            LoadStats();
        }

        public void Hide() => gameObject.SetActive(false);

        // ─────────────────────────────────────────────────────────────
        //  데이터 로드 (TODO: 서버 API 연동)
        // ─────────────────────────────────────────────────────────────

        private void LoadStats()
        {
            // TODO: 서버에서 실제 통계 데이터 수신 후 아래 값들을 갱신하세요.
            //
            // 예시:
            //   StartCoroutine(FetchStatsFromServer(userId, OnStatsFetched));
            //
            // 현재는 placeholder 표시

            SetValue(totalGamesValue, "—");
            SetValue(winsValue,       "—");
            SetValue(lossesValue,     "—");
            SetValue(winRateValue,    "—  %");
            SetValue(bestKillValue,   "—");
            SetValue(avgSurvivalValue,"—  분");
            SetValue(longestStreakValue, "—  연승");
        }

        /// <summary>
        /// 서버 데이터 수신 후 호출할 메서드 (예약).
        /// </summary>
        public void ApplyStats(int totalGames, int wins, int losses,
                               int bestKill, float avgSurvival, int longestStreak)
        {
            float winRate = totalGames > 0 ? (float)wins / totalGames * 100f : 0f;

            SetValue(totalGamesValue,    $"{totalGames} 경기");
            SetValue(winsValue,          $"{wins} 승");
            SetValue(lossesValue,        $"{losses} 패");
            SetValue(winRateValue,       $"{winRate:F1}  %");
            SetValue(bestKillValue,      $"{bestKill}");
            SetValue(avgSurvivalValue,   $"{avgSurvival:F1}  분");
            SetValue(longestStreakValue,  $"{longestStreak}  연승");
        }

        private static void SetValue(Text t, string v)
        {
            if (t != null) t.text = v;
        }
    }
}
