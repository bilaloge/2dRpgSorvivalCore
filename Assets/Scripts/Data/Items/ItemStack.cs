using System;
using UnityEngine;

[Serializable]
public struct ItemStack
{
    public string ItemID;
    public int Amount;

    public bool IsEmpty => string.IsNullOrEmpty(ItemID) || Amount <= 0;

    public ItemStack(string itemID, int amount)
    {
        ItemID = itemID;
        Amount = Mathf.Max(0, amount);
    }

    public static readonly ItemStack Empty = new ItemStack(null, 0);

    public override string ToString() =>
        IsEmpty ? "[Empty]" : $"[{ItemID} x{Amount}]";
}
