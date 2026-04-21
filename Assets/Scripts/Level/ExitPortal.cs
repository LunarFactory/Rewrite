using UnityEngine;
using Player; // PlayerStats가 있는 네임스페이스

namespace Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ExitPortal : InteractableBase
    {
        public override string GetInteractPrompt()
        {
            if (Core.WaveManager.Instance != null && Core.RunManager.Instance != null)
            {
                if (Core.WaveManager.Instance.CurrentWave < 9)
                {
                    return $"다음 웨이브 진행 [{Core.WaveManager.Instance.CurrentWave + 1} 웨이브]";
                }
                else return $"다음 층 [{Core.RunManager.Instance.CurrentFloor + 1} 층]";
            }
            return "웨이브 데이터가 없습니다.";
        }

        public override void OnInteract(GameObject interactEntity)
        {
            if (Core.WaveManager.Instance != null)
            {
                ClearFieldObjects();
                Core.WaveManager.Instance.CompleteCurrentWave();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("웨이브 데이터가 없습니다.");
            }
        }
        private void ClearFieldObjects()
        {
            // 필드에 남아있는 모든 아이템(상점 아이템 포함)을 찾아 파괴
            FieldItem[] remainingItems = Object.FindObjectsByType<FieldItem>(FindObjectsInactive.Exclude);
            foreach (var item in remainingItems)
            {
                Destroy(item.gameObject);
            }

            // 필드에 남아있는 모든 회복 키트를 찾아 파괴
            Healkit[] remainingKits = Object.FindObjectsByType<Healkit>(FindObjectsInactive.Exclude);
            foreach (var kit in remainingKits)
            {
                Destroy(kit.gameObject);
            }

            Debug.Log($"[ExitPortal] 필드 정리 완료: 아이템 {remainingItems.Length}개, 회복키트 {remainingKits.Length}개 제거");
        }
    }
}