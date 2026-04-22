using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Enemy;
using Player;
using System;
using Unity.VisualScripting;

namespace Item
{
    [CreateAssetMenu(fileName = "LightningRod", menuName = "Items/Boss/LightningRod")]
    public class LightningRodItem : PassiveItemData
    {
        public LightningEffect effectData;
        public float searchRange = 7f;
        public float damageMultiplier = 30f; // 3000%
        public float duration = 5f;         // 부착 유지 시간

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LightningRodTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LightningRodTracker>();
                tracker.Initialize(player, effectData, searchRange, damageMultiplier, duration);
            }
        }
    }
    public class LightningRodTracker : MonoBehaviour
    {
        private static Material _sharedLightningMaterial;
        private PlayerStats _player;
        private LightningEffect _effectData;
        private float _duration;

        public void Initialize(PlayerStats player, LightningEffect effectData, float searchRange, float damageMultiplier, float duration)
        {
            _player = player;
            _effectData = effectData;
            _effectData.searchRange = searchRange;
            _effectData.damageMultiplier = damageMultiplier;
            _effectData.duration = duration;
            _duration = duration;

            // [핵심] 유저님이 추가한 OnKill 이벤트를 구독합니다.
            // 이 이벤트는 '이 개체(플레이어)가 누군가를 죽였을 때' 호출됩니다.
            _player.OnKill -= HandleTargetKilled;
            _player.OnKill += HandleTargetKilled;
        }

        private void HandleTargetKilled(EntityStats entity)
        {
            // 1. 방금 죽은 적(victim)의 위치를 기준으로 주변 적 탐색
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(entity.transform.position, _effectData.searchRange, LayerMask.GetMask("Enemy"));

            EntityStats closest = null;
            float minDist = Mathf.Infinity;

            foreach (var col in nearbyEnemies)
            {
                // 죽은 놈 본인 제외 및 살아있는 놈 탐색
                if (col.gameObject == entity.gameObject) continue;

                if (col.TryGetComponent(out EntityStats nextEnemy) && !nextEnemy.isDead)
                {
                    float dist = Vector2.Distance(entity.transform.position, nextEnemy.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = nextEnemy;
                    }
                }
            }

            // 2. 다음 적이 있다면 번개 전이 실행
            if (closest != null)
            {
                TransferLightning(entity.transform.position, closest);
            }
        }

        private void TransferLightning(Vector3 startPos, EntityStats nextTarget)
        {
            // A. 시각적 연결 (지난번 신경망 코드의 LineRenderer 로직 활용)
            CreateLightningVisual(startPos, nextTarget.transform.position);

            // B. BuffManager를 통한 효과 적용 (3000% 데미지는 ActiveEffect.OnStart에서 처리됨)

            if (nextTarget.TryGetComponent<BuffManager>(out BuffManager buff))
            {
                // 30초간 유지되는 번개 부착
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

            // 머티리얼 및 색상 설정
            lr.material = _sharedLightningMaterial;
            lr.startColor = Color.yellow; // 신경망 느낌의 민트색
            lr.endColor = Color.lightYellow;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.08f;
            lr.sortingOrder = 100;

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            // 0.15초 뒤에 선 제거 (잔상 효과)
            Destroy(lineObj, 0.15f);
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnKill -= HandleTargetKilled;
        }
    }
}