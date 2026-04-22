using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "ArtificialNeuralNetwork", menuName = "Items/Common/Artificial Neural Network")]
    public class ArtificialNeuralNetworkItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Neural Link Settings")]
        public float cooldown = 3f;
        public int maxNodes = 3;
        public float linkRange = 5f;
        public float synapticDamageMult = 0.5f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ArtificialNeuralNetworkTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ArtificialNeuralNetworkTracker>();
                tracker.Initialize(player, maxNodes, linkRange, synapticDamageMult, cooldown);
            }
        }
    }

    public class ArtificialNeuralNetworkTracker : MonoBehaviour
    {
        private static Material _sharedLightningMaterial;
        private PlayerStats _player;
        private float _cooldown;
        private int _maxNodes;
        private float _linkRange;
        private float _synapticDamageMult;
        private bool _isCooldown = false;
        public void Initialize(PlayerStats player, int maxNodes, float linkRange, float synapticDamageMult, float cooldown)
        {
            _player = player;
            _maxNodes = maxNodes;
            _linkRange = linkRange;
            _synapticDamageMult = synapticDamageMult;
            _cooldown = cooldown;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (!_isCooldown)
            {
                attacker.StartCoroutine(NeuralChainRoutine((EnemyStats)target, attacker));
                StartCoroutine(CooldownRoutine());
            }
        }
        private IEnumerator NeuralChainRoutine(EnemyStats firstTarget, PlayerStats player)
        {
            // 이미 번개에 맞은 적들을 추적 (중복 타격 방지)
            List<EnemyStats> activatedNodes = new List<EnemyStats>();
            EnemyStats currentTarget = firstTarget;

            // 시작 지점은 플레이어의 위치
            Vector3 startPos = player.transform.position;

            for (int i = 0; i < _maxNodes; i++)
            {
                if (currentTarget == null || currentTarget.isDead) break;

                // A. 데미지 적용 (플레이어 기본 공격력의 일정 비율)
                int chainDamage = Mathf.RoundToInt(player.DamageIncreased.GetValue(player.AttackDamage.GetValue() * _synapticDamageMult));
                currentTarget.TakeDamage(player, chainDamage, Color.cyan);
                Debug.Log($"연쇄 번개 발사됨 : {chainDamage}");
                activatedNodes.Add(currentTarget);

                // B. 시각 효과 (뉴런 연결선 그리기)
                CreateNeuralLinkVisual(startPos, currentTarget.transform.position);

                // C. 다음 노드(적) 탐색
                startPos = currentTarget.transform.position; // 이제 현재 적의 위치가 시작점
                EnemyStats nextTarget = FindClosestNode(startPos, activatedNodes);

                if (nextTarget == null) break; // 주변에 적이 없으면 종료

                currentTarget = nextTarget;

                // 전이 간격 (0.1초씩 끊어서 전이되는 느낌 연출)
                yield return new WaitForSeconds(0.05f);
            }
        }

        // 주변의 가장 가까운 적을 찾는 헬퍼 함수
        private EnemyStats FindClosestNode(Vector3 origin, List<EnemyStats> excludeList)
        {
            // 주변 원형 범위 내의 모든 콜라이더 검사
            Collider2D[] cols = Physics2D.OverlapCircleAll(origin, _linkRange);
            EnemyStats closest = null;
            float minDist = Mathf.Infinity;

            foreach (var col in cols)
            {
                if (col.TryGetComponent(out EnemyStats enemy))
                {
                    // 이미 맞았거나 죽은 적은 제외
                    if (excludeList.Contains(enemy) || enemy.isDead) continue;

                    float dist = Vector3.Distance(origin, enemy.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = enemy;
                    }
                }
            }
            return closest;
        }

        // 런타임에 LineRenderer를 생성하여 번개 시각화
        private void CreateNeuralLinkVisual(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("Neural_Link_Line");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            if (_sharedLightningMaterial == null)
            {
                _sharedLightningMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            // 머티리얼 및 색상 설정
            lr.material = _sharedLightningMaterial;
            lr.startColor = new Color(0.4f, 1f, 0.8f); // 신경망 느낌의 민트색
            lr.endColor = Color.white;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.08f;
            lr.sortingOrder = 100; // 다른 오브젝트보다 앞에 보이게

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            // 0.15초 뒤에 선 제거 (잔상 효과)
            Destroy(lineObj, 0.15f);
        }

        private IEnumerator CooldownRoutine()
        {
            _isCooldown = true;
            yield return new WaitForSeconds(_cooldown);
            _isCooldown = false;
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}