using Entity;
using Mono.Cecil;
using Player;
using UnityEngine;

public class MekaAntiAccessLine : MonoBehaviour
{
    public int damage = 5;
    public float knockbackForce = 15f;

    private SpriteRenderer sr;

    public void Init(Sprite sprite)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent(out PlayerStats pStats))
            {
                pStats.TakeDamage(GetComponentInParent<EntityStats>(), damage);

                // 밀어내기 방향 계산
                Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                if (collision.gameObject.TryGetComponent(out Rigidbody2D pRb))
                {
                    pRb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
