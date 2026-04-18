using System;
using Item;

[Serializable]
public class InventorySlot
{
    public PassiveItemData item;

    public InventorySlot(PassiveItemData item)
    {
        this.item = item;
    }
}