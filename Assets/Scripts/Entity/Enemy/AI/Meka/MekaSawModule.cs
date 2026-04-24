using Core;
using Enemy;
using Player;
using UnityEngine;
using Weapon;

public class MekaSawModule : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites4Way; // 0: 우, 1: 상, 2: 좌, 3: 하

    [SerializeField]
    private SpriteRenderer sr;

    public void Init(Sprite[] sprites)
    {
        sprites4Way = sprites;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
    }

    public void LookAwayFromPlayer(Vector2 playerPos)
    {
        // 플레이어에게서 멀어지는 방향
        Vector2 awayDir = ((Vector2)transform.position - playerPos).normalized;

        float angle = Mathf.Atan2(awayDir.y, awayDir.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360;

        int index = Mathf.RoundToInt(angle / 90f) % 4;
        sr.sprite = sprites4Way[index];
    }

    public void FirePlasma(EnemyStats stats)
    {
        // [개선] 딕셔너리 기반 프리팹 로드
        var bulletPrefab = stats.GetBulletPrefab("Plazma");
        if (bulletPrefab == null)
            return;

        // [개선] 오브젝트 풀링 사용
        GameObject bullet = ProjectileManager.Instance.Get(bulletPrefab);
        bullet.transform.position = transform.position;

        // 플레이어에게서 멀어지는 방향
        Vector2 awayDir = (
            (Vector2)transform.position
            - (Vector2)PlayerStats.LocalPlayer.gameObject.transform.position
        ).normalized;

        if (bullet.TryGetComponent(out Projectile proj))
        {
            proj.Initialize(
                awayDir,
                new ProjectileInfo
                {
                    damage = (int)stats.AttackDamage.GetValue(),
                    pierceCount = -1, // 관통은 없고 튕기기만 함
                    ricochetCount = 3,
                    homingRange = 0,
                    homingStrength = 0,
                    decelerationRate = 0,
                    scale = 1,
                    speed = 10f, // 플라즈마니까 좀 빨라야겠죠?
                    minSpeed = 0, // 감속을 막기 위해 MinSpeed를 Speed와 동일하게 설정
                },
                stats
            );
        }
    }
}
