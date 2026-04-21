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
            if (allUpgrades == null || allUpgrades.Count == 0)
            {
                allUpgrades = new List<PlayerUpgradeData>(Resources.LoadAll<PlayerUpgradeData>("Upgrades"));
            }
            LoadUpgrades();
        }

        public int GetLevel(string id) => _upgradeLevels.ContainsKey(id) ? _upgradeLevels[id] : 0;

        public int GetCredits() => PlayerPrefs.GetInt("LobbyCredits", 0);

        public void SetCredits(int amount)
        {
            PlayerPrefs.SetInt("LobbyCredits", Mathf.Max(0, amount));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 크레딧을 소비하여 업그레이드를 구매합니다.
        /// </summary>
        /// <returns>구매 성공 여부</returns>
        public bool PurchaseUpgrade(PlayerUpgradeData data)
        {
            int currentLevel = GetLevel(data.id);
            if (currentLevel >= 3) return false; // 최대 레벨 제한

            int cost = data.costs[currentLevel];
            int credits = GetCredits();

            if (credits < cost) return false; // 크레딧 부족

            // 크레딧 차감 및 레벨 증가
            SetCredits(credits - cost);
            _upgradeLevels[data.id] = currentLevel + 1;
            SaveUpgrades();
            return true;
        }

        private void SaveUpgrades()
        {
            foreach (var upgrade in allUpgrades)
            {
                if (_upgradeLevels.ContainsKey(upgrade.id))
                    PlayerPrefs.SetInt($"Meta_{upgrade.id}", _upgradeLevels[upgrade.id]);
            }
            PlayerPrefs.Save();
        }

        private void LoadUpgrades()
        {
            if (allUpgrades == null) return;
            foreach (var upgrade in allUpgrades)
                _upgradeLevels[upgrade.id] = PlayerPrefs.GetInt($"Meta_{upgrade.id}", 0);
        }
    }
}
