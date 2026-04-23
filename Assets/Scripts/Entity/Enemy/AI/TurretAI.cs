using Entity;
using Player;
using UnityEngine;

namespace Enemy
{
    public class TurretAI : EnemyAI
    {
        private enum State
        {
            Rest,
            Aim,
            Fire,
        }

        private State _currentState = State.Rest;

        private GameObject _markerObj; // 코드에서 생성할 자식 객체
        private SpriteRenderer _markerSR;

        private float _stateTimer;
        private Vector2 _aimDirection;

        [Header("Turret Settings")]
        public float restDuration = 2f; // 휴식 시간 (바라만 봄)
        public float aimDuration = 1.5f; // 조준 시간 (추적하며 충전)
        public float fireDelay = 0.3f; // 발사 직전 고정 시간
        public Sprite targetingMarkerSprite; // 표식으로 쓸 이미지 (Sprite)
        public int markerSortingOrder = -1; // 발사 직전 고정 시간
        public GameObject bulletPrefab;
        private Transform playerTarget;
        private EnemySpriteAnimationModule _animationModule;
        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            if (rb != null)
                rb.bodyType = RigidbodyType2D.Static;

            // [핵심] 코드 단에서 자식 객체 직접 생성
            CreateMarker();
            _currentState = State.Rest;
            _stateTimer = restDuration;
            playerTarget = PlayerStats.LocalPlayer.transform;
            _animationModule = GetComponent<EnemySpriteAnimationModule>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void CreateMarker()
        {
            if (targetingMarkerSprite == null)
                return;

            // 1. 새 게임 오브젝트 생성 및 자식으로 설정
            _markerObj = new GameObject("Generated_TargetMarker");
            _markerObj.transform.SetParent(this.transform);

            // 2. SpriteRenderer 추가 및 이미지 설정
            _markerSR = _markerObj.AddComponent<SpriteRenderer>();
            _markerSR.sprite = targetingMarkerSprite;

            // 3. 레이어 순서 설정 (보통 발밑이므로 낮은 번호)
            _markerSR.sortingOrder = markerSortingOrder;

            // 4. 초기 상태는 비활성화
            _markerObj.SetActive(false);
        }

        protected override void ExecuteBehavior()
        {
            base.Update();
            if (stats.isStunned)
            {
                if (_markerObj != null)
                    _markerObj.SetActive(false);

                _currentState = State.Rest;
                _stateTimer = restDuration;
            }
            if (playerTarget == null)
                return;
            _stateTimer -= Time.deltaTime;

            switch (_currentState)
            {
                case State.Rest:
                    _aimDirection = (playerTarget.position - transform.position).normalized;
                    _animationModule.UpdateAnimation(Time.deltaTime, _aimDirection);
                    if (!stats.isStunned)
                        _spriteRenderer.color = Color.white;

                    if (_stateTimer <= 0)
                    {
                        // 조준 시작 시 활성화
                        if (_markerObj != null)
                            _markerObj.SetActive(true);
                        _markerObj.transform.position = playerTarget.position;
                        _currentState = State.Aim;
                        _stateTimer = aimDuration;
                    }
                    break;

                case State.Aim:
                    _aimDirection = (playerTarget.position - transform.position).normalized;
                    _animationModule.UpdateAnimation(Time.deltaTime, _aimDirection);

                    // 표식이 플레이어를 실시간 추적 (World 좌표 기준)
                    if (_markerObj != null)
                    {
                        if (!_markerObj.activeSelf)
                            _markerObj.SetActive(true);
                        _markerObj.transform.position = playerTarget.position;
                    }

                    float ratio = 1f - (_stateTimer / aimDuration);
                    _spriteRenderer.color = Color.Lerp(Color.white, Color.red, ratio);

                    if (_stateTimer <= 0)
                    {
                        // 발사 대기 진입 (더 이상 위치 갱신 안 함 = 정지)
                        _currentState = State.Fire;
                        _stateTimer = fireDelay;
                    }
                    break;

                case State.Fire:
                    if (!_markerObj.activeSelf)
                        _markerObj.SetActive(true);
                    _animationModule.UpdateAnimation(Time.deltaTime, _aimDirection);

                    if (_stateTimer <= 0)
                    {
                        Shoot();
                        // 발사 후 비활성화
                        if (_markerObj != null)
                            _markerObj.SetActive(false);

                        _currentState = State.Rest;
                        _stateTimer = restDuration;
                        _spriteRenderer.color = Color.white;
                    }
                    break;
            }
        }

        private void Shoot()
        {
            if (bulletPrefab == null)
                return;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            if (bullet.TryGetComponent(out Weapon.Projectile proj))
            {
                proj.Initialize(
                    _aimDirection,
                    new Weapon.ProjectileInfo
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
                        minSpeed = stats.ProjectileSpeed.GetValue() / 10,
                    },
                    stats
                );
            }
        }
    }
}
