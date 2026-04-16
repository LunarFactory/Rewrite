using UnityEngine;
using Core;

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WaveTransitionTrigger : MonoBehaviour, IInteractable
    {
        public string customPrompt = "다음 웨이브로 이동 (E)";

        public string GetInteractPrompt()
        {
            return customPrompt;
        }

        public void OnInteract(GameObject interactEntity)
        {
            Debug.Log("[WaveTransitionTrigger] 다음 웨이브를 시작합니다.");
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.CompleteCurrentWave();
            }

            // 상호작용 후 제거
            Destroy(gameObject);
        }

        private void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            // 간단한 부유 애니메이션
            float newY = Mathf.Sin(Time.time * 2f) * 0.15f;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }
}
