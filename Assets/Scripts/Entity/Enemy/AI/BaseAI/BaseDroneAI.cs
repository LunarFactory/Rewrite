using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BaseDroneAI : EnemyAI
    {
        [Header("Separation Settings")]
        [SerializeField]
        protected float separationDistance = 1.2f; // 적끼리 유지할 최소 거리

        [SerializeField]
        protected float smoothing = 5f; // 이동 부드러움 (관성)

        [Header("Obstacle Avoidance")]
        [SerializeField]
        private float lookAheadDistance = 2f; // 더듬이 길이 (얼마나 멀리 볼 것인가)

        [SerializeField]
        private float avoidWeight = 3f; // 벽을 피하는 힘 (보통 분리보다 강해야 함)

        [SerializeField]
        private LayerMask obstacleLayer;

        protected override void Awake()
        {
            base.Awake();
            obstacleLayer = LayerMask.GetMask("Obstacle");
        }

        protected virtual void HandleMovingState()
        {
            if (playerTarget == null)
                return;

            // 1. 기본적으로 플레이어를 향하는 방향
            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector2 finalDirection = directionToPlayer;

            // 2. 얇은 레이저 대신 '드론 크기만한 원'을 앞으로 던져서 벽을 감지 (CircleCast)
            RaycastHit2D hit = Physics2D.CircleCast(
                transform.position,
                0.5f,
                directionToPlayer,
                lookAheadDistance,
                obstacleLayer
            );

            // 3. 벽에 막혔을 때의 '미끄러지기' 처리
            if (hit.collider != null)
            {
                // hit.normal은 벽에서 뻗어나오는 수직 화살표
                Vector2 normal = hit.normal;

                // 벽을 따라 미끄러지는 양쪽 방향(접선)을 계산합니다. (수직 벡터를 90도 회전)
                Vector2 slideLeft = new Vector2(-normal.y, normal.x);
                Vector2 slideRight = new Vector2(normal.y, -normal.x);

                // 플레이어에게 향하는 방향(directionToPlayer)과 더 비슷한(내적이 큰) 쪽을 선택!
                if (
                    Vector2.Dot(slideLeft, directionToPlayer)
                    > Vector2.Dot(slideRight, directionToPlayer)
                )
                {
                    finalDirection = slideLeft;
                }
                else
                {
                    finalDirection = slideRight;
                }

                // 꿀팁: 벽에 너무 비비적거리지 않게, 벽 바깥쪽(normal)으로 아주 살짝만 밀어줍니다.
                finalDirection = (finalDirection + normal * 0.2f).normalized;
            }

            // 4. (옵션) 몹들끼리 뭉치지 않게 하는 분리(Separation) 로직 추가
            // Vector2 separation = GetSeparationForce();
            // finalDirection = (finalDirection + separation).normalized;

            // 5. 최종 이동 적용 (물리 속도)
            float speed = stats.MoveSpeed.GetValue();
            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                finalDirection * speed,
                Time.deltaTime * smoothing
            );

            // [사거리 체크 및 상태 전환은 동일]
        }

        private Vector2 GetSeparationForce()
        {
            Vector2 result = Vector2.zero;
            // LayerMask를 "Enemy"로 설정하여 적들끼리만 체크하도록 함
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(
                transform.position,
                separationDistance,
                1 << gameObject.layer
            );

            foreach (var neighbor in neighbors)
            {
                if (neighbor.gameObject == gameObject)
                    continue;

                // 너무 가까운 적과의 거리에 반비례하는 밀어내는 힘 계산
                Vector2 diff = (Vector2)transform.position - (Vector2)neighbor.transform.position;
                result += diff.normalized / Mathf.Max(diff.magnitude, 0.1f);
            }

            return result;
        }

        protected override void ExecuteBehavior() { }
    }
}
