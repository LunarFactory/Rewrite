using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Seeker))] // 추가
    [RequireComponent(typeof(AIPath))] // 추가
    [RequireComponent(typeof(AIDestinationSetter))]
    public class BaseDroneAI : EnemyAI
    {
        protected AIPath aiPath;
        protected AIDestinationSetter destinationSetter;

        protected override void Awake()
        {
            base.Awake();
            aiPath = GetComponent<AIPath>();
            destinationSetter = GetComponent<AIDestinationSetter>();

            // 초기 설정: 2D 설정 강제 및 속도 동기화
            InitAIPath();
        }

        private void InitAIPath()
        {
            if (aiPath == null)
                return;

            aiPath.maxSpeed = stats.MoveSpeed.GetValue();
            aiPath.orientation = OrientationMode.YAxisForward; // 2D 탑다운 기본값
            aiPath.gravity = Vector3.zero; // 2D 추락 방지
        }

        protected virtual void HandleMovingState()
        {
            // 1. 스텔스 및 타겟 존재 여부 확인
            UpdateTargetByStealth();
            if (playerTarget == null)
            {
                StopMovement();
                return;
            } // 3. 타겟이 있다면 이동 재개 및 목적지 갱신
            ResumeMovement();
        }

        protected void UpdateTargetByStealth()
        {
            if (playerStat == null)
                return;

            // 스텔스면 null, 아니면 트랜스폼 할당
            playerTarget = playerStat.isStealth() ? null : playerStat.transform;

            // 목적지 설정기에 타겟 전달 (중복 할당 방지를 위해 비교 후 할당)
            if (destinationSetter.target != playerTarget)
            {
                destinationSetter.target = playerTarget;
            }
        }

        protected void StopMovement()
        {
            if (aiPath.canMove)
            {
                aiPath.canMove = false;
                aiPath.SetPath(null); // 기존 경로 제거
                rb.linearVelocity = Vector2.zero; // 물리 속도 즉시 정지
            }
        }

        protected void ResumeMovement()
        {
            if (!aiPath.canMove)
            {
                aiPath.canMove = true;
            }

            // 실시간 스탯 변화 반영 (버프/디버프 등 대비)
            aiPath.maxSpeed = stats.MoveSpeed.GetValue();
        }

        protected override void ExecuteBehavior() { }
    }
}
