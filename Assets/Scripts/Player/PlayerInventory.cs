using System;

[Serializable]
public class InventorySlot
{
    public Items.ItemData item;

    public InventorySlot(Items.ItemData item)
    {
        this.item = item;
    }
}