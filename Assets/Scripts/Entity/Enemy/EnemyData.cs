using UnityEngine;

namespace Enemy
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Stats")]
        [Tooltip("개체 이름")]
        public string enemyName;
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
        public float baseDecelerationRate = 1f;
        public float baseProjectileScale = 1f;
        public float baseProjectileSpeed = 1f;
        public float baseDamageIncreasedFlat = 0f;
        public float baseDamageIncreasedPercent = 0f;
        public float baseDamageTakenFlat = 0f;
        public float baseDamageTakenPercent = 0f;
        [Tooltip("피격 시 경직 시간")]
        public float hitstunDuration = 0.15f;
        [Tooltip("발사 딜레이")]
        public float shootDelay = 0.5f;

        [Header("Visuals")]
        public Sprite[] animationFrames; // 여기에 애니메이션 프레임들을 넣습니다.
        public float defaultFPS = 10f; // 애니메이션도 다를 경우

        [Header("Settings")]
        public bool isInvincible = false;

        [Header("Drone Specific")]
        public float moveDuration = 2f;
        public GameObject bulletPrefab;
        public float bulletSpeed = 10f;

        [Header("Directional sprite")]
        [Tooltip("순서: 동(0), 북동(1), 북(2), 북서(3), 서(4), 남서(5), 남(6), 남동(7)")]
        public Sprite[] directionalSprites;

        [Header("Turret Settings")]
        public float restDuration = 2f;    // 휴식 시간 (바라만 봄)
        public float aimDuration = 1.5f;   // 조준 시간 (추적하며 충전)
        public float fireDelay = 0.3f;     // 발사 직전 고정 시간
        public Sprite targetingMarkerSprite; // 표식으로 쓸 이미지 (Sprite)
        public int markerSortingOrder = -1;  // 발사 직전 고정 시간
    }
}