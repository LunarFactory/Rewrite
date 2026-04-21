using UnityEngine;
using Entity;

namespace Enemy
{
    public class TurretEnemy : EnemyStats
    {
        private enum State { Rest, Aim, Fire }
        private State _currentState = State.Rest;

        private GameObject _markerObj; // 코드에서 생성할 자식 객체
        private SpriteRenderer _markerSR;

        private float _stateTimer;
        private Vector2 _aimDirection;

        protected override void Start()
        {
            base.Start();
            if (_rb != null) _rb.bodyType = RigidbodyType2D.Static;

            // [핵심] 코드 단에서 자식 객체 직접 생성
            CreateMarker();

            if (data != null)
            {
                _currentState = State.Rest;
                _stateTimer = data.restDuration;
            }
        }

        private void CreateMarker()
        {
            if (data == null || data.targetingMarkerSprite == null) return;

            // 1. 새 게임 오브젝트 생성 및 자식으로 설정
            _markerObj = new GameObject("Generated_TargetMarker");
            _markerObj.transform.SetParent(this.transform);

            // 2. SpriteRenderer 추가 및 이미지 설정
            _markerSR = _markerObj.AddComponent<SpriteRenderer>();
            _markerSR.sprite = data.targetingMarkerSprite;

            // 3. 레이어 순서 설정 (보통 발밑이므로 낮은 번호)
            _markerSR.sortingOrder = data.markerSortingOrder;

            // 4. 초기 상태는 비활성화
            _markerObj.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();
            if (isStunned)
            {
                if (_markerObj != null) _markerObj.SetActive(false);

                _currentState = State.Rest;
                _stateTimer = data.restDuration;

            }
            if (playerTarget == null) return;
            _stateTimer -= Time.deltaTime;

            switch (_currentState)
            {
                case State.Rest:
                    _aimDirection = (playerTarget.position - transform.position).normalized;
                    _animator.UpdateAnimation(Time.deltaTime, _aimDirection);
                    if (!isStunned) _spriteRenderer.color = Color.white;

                    if (_stateTimer <= 0)
                    {
                        // 조준 시작 시 활성화
                        if (_markerObj != null) _markerObj.SetActive(true);
                        _markerObj.transform.position = playerTarget.position;
                        _currentState = State.Aim;
                        _stateTimer = data.aimDuration;
                    }
                    break;

                case State.Aim:
                    _aimDirection = (playerTarget.position - transform.position).normalized;
                    _animator.UpdateAnimation(Time.deltaTime, _aimDirection);

                    // 표식이 플레이어를 실시간 추적 (World 좌표 기준)
                    if (_markerObj != null)
                    {
                        if (!_markerObj.activeSelf) _markerObj.SetActive(true);
                        _markerObj.transform.position = playerTarget.position;
                    }

                    float ratio = 1f - (_stateTimer / data.aimDuration);
                    _spriteRenderer.color = Color.Lerp(Color.white, Color.red, ratio);

                    if (_stateTimer <= 0)
                    {
                        // 발사 대기 진입 (더 이상 위치 갱신 안 함 = 정지)
                        _currentState = State.Fire;
                        _stateTimer = data.fireDelay;
                    }
                    break;

                case State.Fire:
                    if (!_markerObj.activeSelf) _markerObj.SetActive(true);
                    _animator.UpdateAnimation(Time.deltaTime, _aimDirection);

                    if (_stateTimer <= 0)
                    {
                        Shoot();
                        // 발사 후 비활성화
                        if (_markerObj != null) _markerObj.SetActive(false);

                        _currentState = State.Rest;
                        _stateTimer = data.restDuration;
                        _spriteRenderer.color = Color.white;
                    }
                    break;
            }
        }

        private void Shoot()
        {
            if (data == null || data.bulletPrefab == null) return;

            GameObject bullet = Instantiate(data.bulletPrefab, transform.position, Quaternion.identity);
            if (bullet.TryGetComponent(out Weapon.Projectile proj))
            {
                proj.Initialize(_aimDirection, new Weapon.ProjectileInfo
                {
                    damage = Mathf.RoundToInt(DamageIncreased.GetValue(AttackDamage.GetValue())),
                    pierceCount = (int)Pierce.GetValue(),
                    ricochetCount = (int)Ricochet.GetValue(),
                    homingRange = HomingRange.GetValue(),
                    homingStrength = HomingStrength.GetValue(),
                    decelerationRate = DecelerationRate.GetValue(),
                    scale = ProjectileScale.GetValue(),
                    speed = ProjectileSpeed.GetValue(),
                    minSpeed = ProjectileSpeed.GetValue() / 10,
                }, this);
            }
        }
    }
}