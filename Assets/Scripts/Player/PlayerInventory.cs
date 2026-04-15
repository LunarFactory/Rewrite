using System;

[Serializable]
public class InventorySlot
{
    public Item.PassiveItemData item;

    public InventorySlot(Item.PassiveItemData item)
    {
        this.item = item;
    }
}