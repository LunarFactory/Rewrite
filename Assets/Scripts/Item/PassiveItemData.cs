using UnityEngine;

namespace Items
{
    public enum ItemTier { Common, Uncommon, Rare, Boss }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class PassiveItemData : ScriptableObject
    {
        public string itemName;
        [TextArea] public string description;
        public ItemTier tier;
        public Sprite icon;

        public GameObject behaviorPrefab;
    }
}