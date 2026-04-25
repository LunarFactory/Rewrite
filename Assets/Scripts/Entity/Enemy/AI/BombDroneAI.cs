using Core;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BombDroneAI : EnemyAI
    {
        private enum State
        {
            Moving,
            Priming,
            Exploding,
        }

        [Header("Explosion Settings")]
        [SerializeField]
        private float triggerRange = 2f; // 자폭 시퀀스 시작 거리

        [SerializeField]
        private float explosionRadius = 4f; // 실제 폭발 데미지 범위

        [SerializeField]
        private float fuseDuration = 1f; // 자폭 대기 시간 (2초)

        private State _currentState = State.Moving;
        private float _stateTimer;
        private LineRenderer _indicatorCircle; // 빨간 원 시각화용

        protected override void Awake()
        {
            base.Awake();
            _currentState = State.Moving;
            SetupIndicator();
        }

        private void SetupIndicator()
        {
            // 코드로 LineRenderer를 생성하여 빨간 원을 그립니다.
            _indicatorCircle = gameObject.AddComponent<LineRenderer>();
            _indicatorCircle.useWorldSpace = false;
            _indicatorCircle.loop = true;
            _indicatorCircle.startWidth = 0.05f;
            _indicatorCircle.endWidth = 0.05f;
            _indicatorCircle.material = new Material(Shader.Find("Sprites/Default"));
            _indicatorCircle.startColor = _indicatorCircle.endColor = new Color(1, 0, 0, 0.5f);
            _indicatorCircle.positionCount = 51; // 원을 구성할 점의 개수
            _indicatorCircle.enabled = false;

            // 원 그리기 좌표 계산
            for (int i = 0; i < 51; i++)
            {
                float angle = i * Mathf.PI * 2f / 50f;
                _indicatorCircle.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * explosionRadius,
                        Mathf.Sin(angle) * explosionRadius,
                        0
                    )
                );
            }
        }

        protected override void ExecuteBehavior()
        {
            if (playerStat != null)
                playerTarget = playerStat.isStealth() ? null : playerStat.transform;

            if (playerTarget == null || stats.isStaggered)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            switch (_currentState)
            {
                case State.Moving:
                    HandleMovingState();
                    break;
                case State.Priming:
                    HandlePrimingState();
                    break;
            }
        }

        private void HandleMovingState()
        {
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = dir * stats.MoveSpeed.GetValue();

            // 플레이어가 트리거 범위 안에 들어오면 카운트다운 시작
            if (distance <= triggerRange)
            {
                _currentState = State.Priming;
                _stateTimer = fuseDuration;
                _indicatorCircle.enabled = true; // 빨간 원 표시
                rb.linearVelocity = Vector2.zero; // 자폭 준비 중엔 멈춤 (혹은 아주 느리게 이동)
            }
        }

        private void HandlePrimingState()
        {
            _stateTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            // [연출] 시간이 갈수록 빨간 원이 점점 진해지거나 깜빡이게 함
            float alpha = Mathf.PingPong(Time.time * 10f, 1f);
            _indicatorCircle.startColor = _indicatorCircle.endColor = new Color(1, 0, 0, alpha);

            if (_stateTimer <= 0)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (_currentState == State.Exploding)
                return;
            _currentState = State.Exploding;

            // 1. 폭발 범위 내의 플레이어 체크
            float distToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distToPlayer <= explosionRadius)
            {
                if (playerStat != null)
                {
                    // 공격력만큼 데미지 전달
                    int damage = Mathf.RoundToInt(
                        stats.DamageIncreased.GetValue(stats.AttackDamage.GetValue())
                    );
                    playerStat.TakeDamage(stats, damage);
                }
            }

            // 2. 폭발 이펙트 (데이터 기반이라면 이펙트 매니저 등을 통해 호출)
            // CreateExplosionEffect();

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.activeEnemyCount--;
            }
            // 3. 자기 자신 파괴 (오브젝트 풀링 사용 시 반환)
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
