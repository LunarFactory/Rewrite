using Core;
using Entity;
using Player;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(LineRenderer))] // LineRenderer 자동 추가
    public class TurretAI : EnemyAI
    {
        private enum State
        {
            Rest,
            Aim,
            Fire,
        }

        private LineRenderer _lineRenderer;

        [Header("Laser Settings")]
        private LayerMask obstacleLayer;
        private string[] layers = { "Obstacle", "Drone" };
        public Color laserColor = new Color(1, 0, 0, 0.3f); // 기본 반투명 빨간색
        public float laserWidth = 0.05f;

        [Header("Turret Settings")]
        public float restDuration = 2f; // 휴식 시간
        public float aimDuration = 1.5f; // 조준 시간 (추적)
        public float fireDelay = 0.3f; // 발사 직전 고정 시간

        [Header("Visuals")]
        public Sprite targetingMarkerSprite;
        public int markerSortingOrder = -1;

        private State _currentState = State.Rest;
        private Vector2 _aimDirection;
        private Vector2 _lastKnownPosition; // [추가] 마지막으로 감지된 위치

        protected override void Awake()
        {
            base.Awake();
            SetupLineRenderer();
            obstacleLayer = LayerMask.GetMask(layers);
            SetState(State.Rest);
        }

        private void SetupLineRenderer()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            // 라인 렌더러 초기 설정
            _lineRenderer.startWidth = laserWidth;
            _lineRenderer.endWidth = laserWidth;
            _lineRenderer.useWorldSpace = true; // 월드 좌표계 사용

            // 반투명도를 적용하기 위해 재질(Material) 설정
            // 기본 Sprites-Default 재질을 쓰거나 전용 레이저 재질을 할당하세요.
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = laserColor;
            _lineRenderer.endColor = laserColor;

            _lineRenderer.enabled = false; // 처음엔 끔
        }

        protected override void ExecuteBehavior()
        {
            // 1. 플레이어 감지 로직 업데이트
            UpdatePlayerTracking();
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }

            if (stats.isStunned || stats.isStaggered)
            {
                _lineRenderer.enabled = false;
                return;
            }

            switch (_currentState)
            {
                case State.Rest:
                    HandleRestState();
                    break;
                case State.Aim:
                    HandleAimState();
                    break;
                case State.Fire:
                    HandleFireState();
                    break;
            }
        }

        private void UpdatePlayerTracking()
        {
            if (playerStat == null)
                return;

            // 스텔스 상태가 아닐 때만 마지막 위치를 갱신
            if (!playerStat.isStealth() && _currentState != State.Fire)
            {
                playerTarget = playerStat.transform;
                _lastKnownPosition = playerTarget.position;
            }
            else
            {
                // 스텔스라면 타겟을 null로 처리하지만, _lastKnownPosition은 유지됨
                playerTarget = null;
            }
        }

        private void UpdateLaser(Vector2 targetPos)
        {
            if (_lineRenderer == null)
                return;

            Vector2 origin = transform.position;
            Vector2 direction = (targetPos - origin).normalized;

            // 1. 무제한 사거리를 위해 아주 먼 거리(예: 1000f)까지 레이를 쏩니다.
            float maxDistance = 1000f;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, obstacleLayer);

            Vector2 endPoint;

            if (playerTarget != null)
            {
                // 플레이어가 감지될 때
                float distToPlayer = Vector2.Distance(origin, playerTarget.position);

                if (hit.collider != null)
                {
                    // 벽이 있다면 플레이어와 벽 중 더 가까운 곳까지만 그림
                    endPoint = origin + direction * Mathf.Min(distToPlayer, hit.distance);
                }
                else
                {
                    // 벽이 아예 없는 탁 트인 곳이라면 플레이어까지 그림
                    endPoint = playerTarget.position;
                }
            }
            else
            {
                // 플레이어가 감지되지 않을 때 (은신 등)
                if (hit.collider != null)
                {
                    // 벽이 있다면 벽 충돌 지점까지 그림
                    endPoint = hit.point;
                }
                else
                {
                    // 벽조직차 없는 허허벌판이라면 무한히(maxDistance) 뻗어나감
                    endPoint = origin + direction * maxDistance;
                }
            }

            // 2. 결과 적용
            _lineRenderer.SetPosition(0, origin);
            _lineRenderer.SetPosition(1, endPoint);

            // [연출] 조준 완료 시 선의 굵기나 농도 변화 (옵션)
            if (_currentState == State.Aim)
            {
                float ratio = 1f - (_stateTimer / aimDuration);
                Color currentAlpha = laserColor;
                currentAlpha.a = Mathf.Lerp(0.1f, 0.6f, ratio);
                _lineRenderer.startColor = currentAlpha;
                _lineRenderer.endColor = currentAlpha;
                _lineRenderer.startWidth = Mathf.Lerp(0.02f, laserWidth, ratio);
                _lineRenderer.endWidth = Mathf.Lerp(0.02f, laserWidth, ratio);
            }
        }

        private void HandleRestState()
        {
            _lineRenderer.enabled = false;
            // 플레이어를 보지 못하더라도 마지막 위치를 바라봄
            _aimDirection = (_lastKnownPosition - (Vector2)transform.position).normalized;
            _animationModule.UpdateAnimation(Time.deltaTime, _aimDirection);

            // [조건] 플레이어가 보이고(스텔스 X) + 사거리 안에 있을 때만 공격 시작
            if (_stateTimer <= 0 && playerTarget != null)
            {
                SetState(State.Aim);
            }
        }

        private void HandleAimState()
        {
            _lineRenderer.enabled = true;
            UpdateLaser(_lastKnownPosition); // 실시간 추적
            // 조준 중 플레이어가 사라지면 마지막 위치를 계속 조준, 나타나면 새 위치로 이동
            _aimDirection = (_lastKnownPosition - (Vector2)transform.position).normalized;
            _animationModule.UpdateAnimation(Time.deltaTime, _aimDirection);

            float ratio = 1f - (_stateTimer / aimDuration);
            sr.color = Color.Lerp(Color.white, Color.red, ratio);

            if (_stateTimer <= 0)
            {
                SetState(State.Fire);
            }
        }

        private void HandleFireState()
        {
            _lineRenderer.enabled = true;
            UpdateLaser(_lastKnownPosition);
            _lineRenderer.startColor = Color.yellow; // 번쩍!
            _lineRenderer.endColor = Color.yellow; // 번쩍!
            _lineRenderer.startWidth = laserWidth * 2f; // 굵게!
            // 발사 직전 고정 (이미 결정된 _aimDirection 사용)
            _animationModule.UpdateAnimation(Time.deltaTime, _aimDirection);

            if (_stateTimer <= 0)
            {
                Shoot();
                SetState(State.Rest);
            }
        }

        private void SetState(State newState)
        {
            _currentState = newState;
            switch (_currentState)
            {
                case State.Rest:
                    _stateTimer = restDuration;
                    sr.color = Color.white;
                    break;
                case State.Aim:
                    _stateTimer = aimDuration;
                    break;
                case State.Fire:
                    _stateTimer = fireDelay;
                    break;
            }
        }

        private void Shoot()
        {
            // [개선] 딕셔너리 기반 프리팹 로드
            var bulletPrefab = stats.GetBulletPrefab("Normal");
            if (bulletPrefab == null)
                return;

            // [개선] 오브젝트 풀링 사용
            GameObject bullet = ProjectileManager.Instance.Get(bulletPrefab);
            bullet.transform.position = transform.position;

            float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (bullet.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(
                    _aimDirection,
                    new ProjectileInfo
                    {
                        damage = Mathf.RoundToInt(
                            stats.DamageIncreased.GetValue(stats.AttackDamage.GetValue())
                        ),
                        pierceCount = (int)stats.Pierce.GetValue(),
                        ricochetCount = (int)stats.Ricochet.GetValue(),
                        homingRange = stats.HomingRange.GetValue(),
                        homingStrength = stats.HomingStrength.GetValue(),
                        decelerationRate = stats.DecelerationRate.GetValue(),
                        scale = stats.ProjectileScale.GetValue(),
                        speed = stats.ProjectileSpeed.GetValue(),
                        minSpeed = stats.ProjectileSpeed.GetValue() / 10f,
                    },
                    stats
                );
            }
        }
    }
}
