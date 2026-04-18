using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerStealth : MonoBehaviour
    {
        [Header("Stealth Settings")]
        [Tooltip("기본 스텔스 지속 시간 (초)")]
        [SerializeField] private float stealthDuration = 3f;

        [Tooltip("업그레이드를 통해 늘릴 수 있는 최대 스텔스 지속 시간 (초)")]
        [SerializeField] private float maxStealthDuration = 5f;

        [Tooltip("스텔스 해제 후 재충전 속도 (1 = 지속 시간과 동일한 속도로 충전)")]
        [SerializeField] private float rechargeRate = 1f;

        // ── 공개 상태 프로퍼티 ────────────────────────────────────────
        /// <summary>현재 스텔스 발동 중</summary>
        public bool IsStealthActive { get; private set; }

        /// <summary>회피 구르기 중 (무적 포함)</summary>
        public bool IsDodging { get; private set; }

        /// <summary>스텔스 게이지가 완전히 충전되지 않아 사용 불가</summary>
        public bool IsRecharging => !IsStealthActive && stealthTimer < stealthDuration;

        /// <summary>현재 잔여 게이지 비율 (0 ~ 1)</summary>
        public float StealthRatio => Mathf.Clamp01(stealthTimer / stealthDuration);

        /// <summary>현재 설정된 최대 지속 시간</summary>
        public float MaxDuration => stealthDuration;

        /// <summary>절대 최대 지속 시간 (UI 슬라이더 폭 계산용)</summary>
        public float AbsoluteMaxDuration => maxStealthDuration;

        // ── 내부 상태 ─────────────────────────────────────────────────
        private float stealthTimer;
        private Coroutine activeCoroutine;

        private Rigidbody2D rb;
        /// <summary>플레이어 본체 + 무기 등 모든 자식 SpriteRenderer</summary>
        private SpriteRenderer[] allRenderers;

        // ────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            rb           = GetComponent<Rigidbody2D>();
            // 플레이어 본체와 모든 자식(무기 등)의 SpriteRenderer를 한꺼번에 수집
            allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Start()
        {
            stealthTimer = stealthDuration; // 시작 시 게이지 만충
        }

        private void Update()
        {
            // 스텔스 비활성 상태일 때 게이지 재충전
            if (!IsStealthActive && stealthTimer < stealthDuration)
            {
                stealthTimer = Mathf.Min(stealthDuration, stealthTimer + Time.deltaTime * rechargeRate);
            }
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 공개 API

        /// <summary>스텔스 활성화. 게이지가 꽉 차있을 때만 동작합니다.</summary>
        public void ActivateStealth()
        {
            // 충전이 안 됐거나 이미 활성 중이면 무시
            if (IsStealthActive || IsRecharging) return;

            activeCoroutine = StartCoroutine(StealthRoutine());
        }

        /// <summary>사격 등 외부 이벤트로 스텔스를 즉시 해제합니다.</summary>
        public void CancelStealth()
        {
            if (!IsStealthActive) return;

            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            // 남은 게이지 = 0 → 재충전 필요
            stealthTimer = 0f;
            EndStealth();
        }

        /// <summary>업그레이드로 스텔스 최대 지속 시간을 늘립니다. (최대 maxStealthDuration 까지)</summary>
        public void UpgradeStealthDuration(float additionalSeconds)
        {
            float newDuration = Mathf.Clamp(stealthDuration + additionalSeconds, 0f, maxStealthDuration);
            // 기존 비율 유지하며 타이머 조정
            float ratio = StealthRatio;
            stealthDuration = newDuration;
            stealthTimer    = stealthDuration * ratio;
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        #region 내부 코루틴

        private IEnumerator StealthRoutine()
        {
            IsStealthActive = true;
            IsDodging       = true;

            // 플레이어 + 무기 모두 반투명
            SetAlpha(0.3f);

            // 이동 속도 부스트 없이 즉시 스텔스로 진입
            yield return null;
            IsDodging = false;

            // 게이지를 시간에 따라 소모
            while (stealthTimer > 0f)
            {
                stealthTimer -= Time.deltaTime;
                yield return null;
            }

            stealthTimer    = 0f;
            activeCoroutine = null;
            EndStealth();
        }

        private void EndStealth()
        {
            IsStealthActive = false;
            IsDodging       = false;
            SetAlpha(1f);
        }

        private void SetAlpha(float alpha)
        {
            if (allRenderers == null) return;
            foreach (var sr in allRenderers)
            {
                if (sr == null) continue;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        #endregion
    }
}
