using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "LightningRod", menuName = "Items/Boss/LightningRod")]
    public class LightningRodItem : PassiveItemData
    {
        public LightningEffect effectData;
        public int count = 5;
        public float searchRange = 7f;
        public float damageMultiplier = 30f;
        public float duration = 5f;
        public float cooldown = 3f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LightningRodTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LightningRodTracker>();
                tracker.Initialize(
                    player,
                    effectData,
                    count,
                    searchRange,
                    damageMultiplier,
                    duration,
                    cooldown
                );
            }
        }
    }

    public class LightningRodTracker : MonoBehaviour
    {
        private static Material _sharedLightningMaterial;
        private PlayerStats _player;
        private LightningEffect _effectData;
        private int _maxCount;
        private float _duration;
        private float _cooldown;

        private bool _isCooldown = false;
        private int _currentChainCount; // ScriptableObject 오염 방지용 런타임 카운트

        public void Initialize(
            PlayerStats player,
            LightningEffect effectData,
            int count,
            float searchRange,
            float damageMultiplier,
            float duration,
            float cooldown
        )
        {
            _player = player;
            _effectData = effectData;
            _effectData.searchRange = searchRange;
            _effectData.damageMultiplier = damageMultiplier;
            _maxCount = count;
            _duration = duration;
            _cooldown = cooldown;

            _player.OnKill -= HandleTargetKilled;
            _player.OnKill += HandleTargetKilled;
        }

        private void HandleTargetKilled(EntityStats entity)
        {
            // 1. 현재 죽은 적에게 피뢰침 디버프가 있었는지 확인 (연쇄 트리거 판별)
            bool isChainTrigger = false;
            if (entity.TryGetComponent(out BuffManager targetBuff))
            {
                if (targetBuff.HasEffect(_effectData) != null)
                {
                    isChainTrigger = true;
                }
            }

            // 2. 연쇄 중이 아닌 '최초 처치'인데 쿨다운 중이라면 무시
            if (!isChainTrigger && _isCooldown)
                return;

            // 3. 주변 적 검색
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(
                entity.transform.position,
                _effectData.searchRange,
                LayerMask.GetMask("Enemy")
            );

            EntityStats closest = null;
            float minDist = Mathf.Infinity;

            foreach (var col in nearbyEnemies)
            {
                if (col.gameObject == entity.gameObject)
                    continue;

                if (col.TryGetComponent(out EntityStats nextEnemy) && !nextEnemy.isDead)
                {
                    float dist = Vector2.Distance(
                        entity.transform.position,
                        nextEnemy.transform.position
                    );
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = nextEnemy;
                    }
                }
            }

            // 4. 전이할 적이 있다면 실행
            if (closest != null)
            {
                // 연쇄 트리거였다면 카운트 차감, 최초 트리거였다면 최대 수치에서 시작
                int nextCount = isChainTrigger ? _currentChainCount - 1 : _maxCount - 1;

                if (nextCount > 0)
                {
                    // 최초 처치 시에만 글로벌 쿨다운을 돌립니다.
                    if (!isChainTrigger)
                        StartCoroutine(CooldownRoutine());

                    // [핵심] 코루틴으로 전이를 위임하여 동일 프레임 폭발을 방지합니다.
                    StartCoroutine(
                        TransferLightningRoutine(entity.transform.position, closest, nextCount)
                    );
                }
            }
        }

        private IEnumerator TransferLightningRoutine(
            Vector3 startPos,
            EntityStats nextTarget,
            int remainingCount
        )
        {
            // 0.04초(약 2~3프레임)의 미세한 지연을 주어 동일 프레임 재귀를 끊어냅니다.
            // 타격이 순차적으로 들어가며 연쇄 번개 특유의 리드미컬한 연출이 완성됩니다.
            yield return new WaitForSeconds(0.04f);

            if (nextTarget == null || nextTarget.isDead)
                yield break;

            // 시각적 선 생성
            CreateLightningVisual(startPos, nextTarget.transform.position);

            if (nextTarget.TryGetComponent<BuffManager>(out BuffManager buff))
            {
                // 원본 에셋을 해치지 않고, Tracker의 멤버 변수를 징검다리 삼아 카운트를 넘겨줍니다.
                _currentChainCount = remainingCount;
                buff.ApplyEffect(_effectData, _duration, _player);
            }
        }

        private void CreateLightningVisual(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new("Lightning_Rod_Line");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            if (_sharedLightningMaterial == null)
            {
                _sharedLightningMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            lr.material = _sharedLightningMaterial;
            lr.startColor = Color.yellow;
            lr.endColor = new Color(1f, 1f, 0.6f);
            lr.startWidth = 0.1f; // 조금 더 잘 보이게 두께 상향
            lr.endWidth = 0.1f;
            lr.sortingOrder = 100;

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            Destroy(lineObj, 0.12f);
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
                _player.OnKill -= HandleTargetKilled;
        }
    }
}
