using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "ArtificialNeuralNetwork", menuName = "Items/Artificial Neural Network")]
    public class ArtificialNeuralNetwork : PassiveItemData // 부모를 상속받음
    {
        [Header("Neural Link Settings")]
        public float cooldown = 3f;
        public int maxNodes = 3;
        public float linkRange = 5f;
        public float synapticDamageMult = 0.5f;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            float lastFireTime = -999f;

            stats.OnAttackHit += (target, damage) =>
            {
                if (Time.time < lastFireTime + cooldown) return;
                lastFireTime = Time.time;
                stats.StartCoroutine(NeuralChainRoutine(target, stats));
            };
            
            Debug.Log($"{itemName} 효과가 적용되었습니다!");
        }

        private IEnumerator NeuralChainRoutine(EnemyBase firstTarget, PlayerStats stats)
        {
            // 이미 번개에 맞은 적들을 추적 (중복 타격 방지)
            List<EnemyBase> activatedNodes = new List<EnemyBase>();
            EnemyBase currentTarget = firstTarget;

            // 시작 지점은 플레이어의 위치
            Vector3 startPos = stats.transform.position;

            for (int i = 0; i < maxNodes; i++)
            {
                if (currentTarget == null || currentTarget.isDead) break;

                // A. 데미지 적용 (플레이어 기본 공격력의 일정 비율)
                float chainDamage = stats.AttackDamage.GetValue() * synapticDamageMult;
                currentTarget.TakeDamage(chainDamage);
                Debug.Log($"연쇄 번개 발사됨 : {chainDamage}");
                activatedNodes.Add(currentTarget);

                // B. 시각 효과 (뉴런 연결선 그리기)
                CreateNeuralLinkVisual(startPos, currentTarget.transform.position);

                // C. 다음 노드(적) 탐색
                startPos = currentTarget.transform.position; // 이제 현재 적의 위치가 시작점
                EnemyBase nextTarget = FindClosestNode(startPos, activatedNodes);

                if (nextTarget == null) break; // 주변에 적이 없으면 종료

                currentTarget = nextTarget;

                // 전이 간격 (0.1초씩 끊어서 전이되는 느낌 연출)
                yield return new WaitForSeconds(0.1f);
            }
        }

        // 주변의 가장 가까운 적을 찾는 헬퍼 함수
        private EnemyBase FindClosestNode(Vector3 origin, List<EnemyBase> excludeList)
        {
            // 주변 원형 범위 내의 모든 콜라이더 검사
            Collider2D[] cols = Physics2D.OverlapCircleAll(origin, linkRange);
            EnemyBase closest = null;
            float minDist = Mathf.Infinity;

            foreach (var col in cols)
            {
                if (col.TryGetComponent(out EnemyBase enemy))
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

            // 머티리얼 및 색상 설정
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.4f, 1f, 0.8f); // 신경망 느낌의 민트색
            lr.endColor = Color.white;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.03f;
            lr.sortingOrder = 100; // 다른 오브젝트보다 앞에 보이게

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            // 0.15초 뒤에 선 제거 (잔상 효과)
            Destroy(lineObj, 0.15f);
        }
    }
}