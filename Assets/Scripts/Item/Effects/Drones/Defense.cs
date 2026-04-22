using UnityEngine;

namespace Drone
{
    public class Defense : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 적 탄환과 충돌 시 (레이어나 태그 활용)
            if (collision.CompareTag("EnemyProjectile"))
            {
                TriggerShockwave();
                Destroy(collision.gameObject);
            }
        }

        private void TriggerShockwave()
        {
            // 주변 탄환들 검색 (반경 3f)
            Collider2D[] bullets = Physics2D.OverlapCircleAll(
                transform.position,
                3f,
                1 << LayerMask.NameToLayer("EnemyProjectile")
            );
            foreach (var b in bullets)
            {
                Destroy(b.gameObject);
            }
        }
    }
}
