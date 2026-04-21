using UnityEngine;
using System.Collections.Generic;
using Player;

namespace Core
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        // 모든 업그레이드 데이터 리스트
        public List<PlayerUpgradeData> allUpgrades;

        // 현재 각 업그레이드의 레벨 저장 (ID, Level)
        private Dictionary<string, int> _upgradeLevels = new Dictionary<string, int>();

        private void Awake()
        {
            Instance = this;
            LoadUpgrades();
        }

        public int GetLevel(string id) => _upgradeLevels.ContainsKey(id) ? _upgradeLevels[id] : 0;

        public void PurchaseUpgrade(PlayerUpgradeData data)
        {
            int currentLevel = GetLevel(data.id);
            if (currentLevel >= 3) return; // 최대 레벨 제한

            int cost = data.costs[currentLevel];

            // 여기에 로비 크레딧 체크 로직 추가
            // if (LobbyManager.Instance.Credits >= cost) { ... }

            _upgradeLevels[data.id] = currentLevel + 1;
            SaveUpgrades();
        }

        private void SaveUpgrades()
        {
            foreach (var upgrade in allUpgrades)
                PlayerPrefs.SetInt($"Meta_{upgrade.id}", _upgradeLevels[upgrade.id]);
            PlayerPrefs.Save();
        }

        private void LoadUpgrades()
        {
            foreach (var upgrade in allUpgrades)
                _upgradeLevels[upgrade.id] = PlayerPrefs.GetInt($"Meta_{upgrade.id}", 0);
        }
    }
}
