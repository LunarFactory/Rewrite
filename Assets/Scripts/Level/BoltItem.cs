using UnityEngine;
using Player;

namespace Level
{
    /// <summary>
    /// 적 처치 시 드롭되는 통화(Bolts) 아이템입니다.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class BoltItem : MonoBehaviour
    {
        [Header("Settings")]
        public int value = 1;
        public float pickupRange = 1.0f;
        public float moveSpeed = 5.0f;
        
        private Transform playerTransform;
        private bool isBeingPickedUp = false;
        private SpriteRenderer spriteRenderer;
        private float bobTimer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            GetComponent<CircleCollider2D>().isTrigger = true;
            
            // 공중에 떠 있는 느낌을 위한 초기 타이머 랜덤 설정
            bobTimer = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            HandleVisual();
            HandlePickup();
        }

        private void HandleVisual()
        {
            // 위아래로 부유하는 애니메이션
            if (spriteRenderer != null)
            {
                bobTimer += Time.deltaTime * 3f;
                float offset = Mathf.Sin(bobTimer) * 0.15f;
                spriteRenderer.transform.localPosition = new Vector3(0, offset, 0);
                
                // 살짝 회전
                spriteRenderer.transform.Rotate(Vector3.forward, 90f * Time.deltaTime);
            }
        }

        private void HandlePickup()
        {
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // 자석 효과: 일정 거리 안에 들어오면 플레이어에게 빨려 들어감
            if (distance < pickupRange || isBeingPickedUp)
            {
                isBeingPickedUp = true;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

                if (distance < 0.2f)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            if (playerTransform.TryGetComponent(out PlayerStats stats))
            {
                stats.AddBolts(value);
            }
            
            // TODO: 사운드나 파티클 효과 추가 가능
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isBeingPickedUp = true;
            }
        }
    }
}
