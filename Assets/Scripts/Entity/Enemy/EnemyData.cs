using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public enum EnemyTier
    {
        Normal,
        Elite,
        Special,
    }

    [System.Serializable]
    public struct BulletEntry
    {
        public string bulletKey; // AI가 찾을 이름 (예: "Normal", "Special")
        public GameObject bulletPrefab;
    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Stats")]
        [Tooltip("개체 이름")]
        public string enemyName;

        public string enemySubtitle;

        [Tooltip("최대 체력")]
        public int maxHealth = 30;

        [Tooltip("이동 속도")]
        public float baseMoveSpeed = 2f;

        [Tooltip("공격 피해")]
        public int baseAttackDamage = 10;
        public int baseRicochet = 0;
        public int basePierce = 0;
        public float baseHomingRange = 0f;
        public float baseHomingStrength = 0f;
        public float baseDecelerationRate = 0f;
        public float baseProjectileScale = 1f;
        public float baseProjectileSpeed = 1f;
        public float baseDamageIncreasedFlat = 0f;
        public float baseDamageIncreasedPercent = 0f;
        public float baseDamageTakenFlat = 0f;
        public float baseDamageTakenPercent = 0f;

        [Tooltip("피격 시 경직 시간")]
        public float hitstunDuration = 0.15f;

        [Header("Visuals")]
        public Sprite[] animationFrames; // 여기에 애니메이션 프레임들을 넣습니다.
        public Sprite[] directionalSprites; // 8방향 정지 이미지용
        public float defaultFPS = 10f; // 애니메이션도 다를 경우

        [Header("Settings")]
        public bool isInvincible = false;

        [Tooltip("생성 설정")]
        public EnemyTier tier;
        public int cost;
        public int minFloor; // 등장 시작 층
        public int maxCountInWave; // 한 웨이브 최대 소환 수 (0이면 무제한)
        public string ComponentName;

        [Header("물리 설정")]
        public Vector2 colliderOffset = new Vector2(0f, 0f); // 발치에 피벗이 있을 때 유용
        public Vector2 colliderSize = new Vector2(1f, 1f); // 인스펙터에서 조절 가능

        [Header("탄환 목록")]
        public List<BulletEntry> bulletList;
    }
}
