using UnityEngine;
using Entity;
using Player;
using Enemy;

namespace Drone
{
    public class Laser : MonoBehaviour
    {
        private float range = 8f;
        private LayerMask enemyLayer;
        private LineRenderer _line;
        private Transform _currentTarget;
        private float _damageAccumulator;

        private void Awake()
        {
            // 1. 컴포넌트가 붙었는지 로그로 확인
            Debug.Log($"{gameObject.name}에 레이저 모듈 장착 완료!");

            // 2. 레이어 마스크가 0이면 자동으로 "Enemy" 레이어 잡기
            enemyLayer = 1 << LayerMask.NameToLayer("Enemy");

            // 3. 라인 렌더러 설정
            _line = GetComponent<LineRenderer>();
            if (_line == null) _line = gameObject.AddComponent<LineRenderer>();

            _line.positionCount = 2;
            _line.startWidth = 0.1f;
            _line.endWidth = 0.1f;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = Color.magenta;
            _line.endColor = Color.red;
            _line.enabled = false;
            _line.sortingOrder = 10;
        }

        private void Update()
        {
            // 플레이어가 없으면 작동 안 함
            if (PlayerStats.LocalPlayer == null) return;

            FindClosestTarget();

            if (_currentTarget == null)
            {
                _line.enabled = false;
                return;
            }

            // 시각화
            _line.enabled = true;
            _line.SetPosition(0, transform.position);
            _line.SetPosition(1, _currentTarget.position);

            // 데미지 (초당 50%)
            float dps = Mathf.RoundToInt(PlayerStats.LocalPlayer.DamageIncreased.GetValue(PlayerStats.LocalPlayer.AttackDamage.GetValue() * 0.5f * DroneManager.Instance.globalDroneDamageMultiplier));
            _damageAccumulator += dps * Time.deltaTime;

            if (_damageAccumulator >= 1f)
            {
                int intDmg = Mathf.FloorToInt(_damageAccumulator);
                _currentTarget.GetComponent<EnemyStats>()?.TakeDamage(PlayerStats.LocalPlayer, intDmg, Color.magenta);
                _damageAccumulator -= intDmg;
            }
        }

        private void FindClosestTarget()
        {
            // range가 0이면 아무것도 못 찾음. 여기서 강제로 8 할당
            float currentRange = (range <= 0) ? 8f : range;
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, currentRange, enemyLayer);

            Transform closest = null;
            float minDistance = float.MaxValue;

            foreach (var enemyCollider in enemies)
            {
                var stats = enemyCollider.GetComponent<EntityStats>();
                if (stats != null && stats.isDead) continue;

                float distSqr = (enemyCollider.transform.position - transform.position).sqrMagnitude;
                if (distSqr < minDistance)
                {
                    minDistance = distSqr;
                    closest = enemyCollider.transform;
                }
            }
            _currentTarget = closest;
        }
    }
}