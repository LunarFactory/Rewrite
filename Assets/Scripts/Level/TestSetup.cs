using UnityEngine;
using Core;

namespace Level
{
    public class TestSetup : MonoBehaviour
    {
        [Header("Prefabs to Spawn")]
        public GameObject playerPrefab;
        public GameObject hudPrefab;
        public GameObject crosshairPrefab;
        
        public bool runSetup = true;

        void Start()
        {
            if (!runSetup) return;

            // 1. 플레이어 소환 (이미 모든 컴포넌트와 자식이 세팅된 상태)
            if (GameObject.FindWithTag("Player") == null)
            {
                Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            }

            // 2. HUD 및 크로스헤어 소환 (싱글톤 체크는 각 프리팹 내부에서 수행)
            if (FindAnyObjectByType<UI.GameHUD>() == null) Instantiate(hudPrefab);
            if (FindAnyObjectByType<Player.Crosshair>() == null) Instantiate(crosshairPrefab);
        }
    }
}