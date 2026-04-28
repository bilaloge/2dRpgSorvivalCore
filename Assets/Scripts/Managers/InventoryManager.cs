using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class InventoryManager : IInitializable
{
    public const int TotalSlots = 50;  // sahnede her zaman 50 slot objesi var
    public const int DefaultActive = 20; // başlangıçta açık slot sayısı
    public const int MaxActive = 50;
    public const int MaxStack = 99;

    [Inject] private ItemDatabase _itemDatabase;

    private ItemStack[] _slots;

    // Kaç slot aktif (kilit açık), geri kalanı kilitli
    private int _activeCapacity;

    public int ActiveCapacity => _activeCapacity;

    // Olaylar
    public event Action<int> OnSlotChanged;
    public event Action<int> OnCapacityChanged; // yeni kapasite değeri ile tetiklenir

    public bool IsFull
    {
        get
        {
            for (int i = 0; i < _activeCapacity; i++)
                if (_slots[i].IsEmpty) return false;
            return true;
        }
    }

    public void Initialize()
    {
        _slots = new ItemStack[TotalSlots];
        _activeCapacity = DefaultActive;

        for (int i = 0; i < TotalSlots; i++)
            _slots[i] = ItemStack.Empty;
    }

    // Item ekleme — sığmayan miktarı döner (0 = hepsi eklendi)
    public int AddItem(string itemID, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemID) || amount <= 0) return amount;

        ItemData data = _itemDatabase.Get(itemID);
        if (data == null) return amount;

        int remaining = amount;

        if (data.IsStackable)
            remaining = FillExistingStacks(itemID, data.EffectiveMaxStack, remaining);

        if (remaining > 0)
            remaining = FillEmptySlots(itemID, data.EffectiveMaxStack, remaining);

        return remaining;
    }

    public bool CanAddItem(string itemID, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemID) || amount <= 0) return false;
        ItemData data = _itemDatabase.Get(itemID);
        if (data == null) return false;

        int space = 0;
        for (int i = 0; i < _activeCapacity; i++)
        {
            if (_slots[i].IsEmpty)
                space += data.EffectiveMaxStack;
            else if (data.IsStackable && _slots[i].ItemID == itemID)
                space += data.EffectiveMaxStack - _slots[i].Amount;

            if (space >= amount) return true;
        }
        return false;
    }

    public bool RemoveAt(int index, int amount = 1)
    {
        if (!IsValidActive(index) || _slots[index].IsEmpty) return false;

        int removeAmount = amount < 0 ? _slots[index].Amount : amount;

        if (removeAmount >= _slots[index].Amount)
            _slots[index] = ItemStack.Empty;
        else
            _slots[index] = new ItemStack(_slots[index].ItemID, _slots[index].Amount - removeAmount);

        OnSlotChanged?.Invoke(index);
        return true;
    }

    public void SwapSlots(int from, int to)
    {
        if (!IsValidActive(from) || !IsValidActive(to)) return;
        (_slots[from], _slots[to]) = (_slots[to], _slots[from]);
        OnSlotChanged?.Invoke(from);
        OnSlotChanged?.Invoke(to);
    }

    public ItemStack GetSlot(int index)
        => index >= 0 && index < TotalSlots ? _slots[index] : ItemStack.Empty;

    public int GetTotalAmount(string itemID)
    {
        int total = 0;
        for (int i = 0; i < _activeCapacity; i++)
            if (_slots[i].ItemID == itemID) total += _slots[i].Amount;
        return total;
    }

    // Kapasite artırma — geliştirme sistemi tarafından çağrılır
    public bool ExpandCapacity(int extraSlots)
    {
        int newCapacity = _activeCapacity + extraSlots;
        if (newCapacity > MaxActive)
        {
            Debug.LogWarning("[InventoryManager] Maksimum kapasiteye ulaşıldı.");
            return false;
        }

        _activeCapacity = newCapacity;
        OnCapacityChanged?.Invoke(_activeCapacity);
        return true;
    }

    // Kayıt / Yükleme
    public void WriteSaveData(CharacterSaveData data)
    {
        data.inventoryItemIDs.Clear();
        data.inventoryItemAmounts.Clear();
        data.inventoryCapacity = _activeCapacity;

        for (int i = 0; i < TotalSlots; i++)
        {
            data.inventoryItemIDs.Add(_slots[i].ItemID ?? string.Empty);
            data.inventoryItemAmounts.Add(_slots[i].Amount);
        }
    }

    public void ReadSaveData(CharacterSaveData data)
    {
        _activeCapacity = Mathf.Clamp(
            data.inventoryCapacity > 0 ? data.inventoryCapacity : DefaultActive,
            DefaultActive, MaxActive);

        _slots = new ItemStack[TotalSlots];
        int count = Mathf.Min(data.inventoryItemIDs.Count, TotalSlots);

        for (int i = 0; i < count; i++)
        {
            string id = data.inventoryItemIDs[i];
            int amt = i < data.inventoryItemAmounts.Count ? data.inventoryItemAmounts[i] : 0;
            _slots[i] = string.IsNullOrEmpty(id) ? ItemStack.Empty : new ItemStack(id, amt);
        }

        for (int i = count; i < TotalSlots; i++)
            _slots[i] = ItemStack.Empty;
    }

    private int FillExistingStacks(string itemID, int maxStack, int remaining)
    {
        for (int i = 0; i < _activeCapacity && remaining > 0; i++)
        {
            if (_slots[i].IsEmpty || _slots[i].ItemID != itemID) continue;
            int space = maxStack - _slots[i].Amount;
            int toAdd = Mathf.Min(space, remaining);
            if (toAdd <= 0) continue;
            _slots[i] = new ItemStack(itemID, _slots[i].Amount + toAdd);
            remaining -= toAdd;
            OnSlotChanged?.Invoke(i);
        }
        return remaining;
    }

    private int FillEmptySlots(string itemID, int maxStack, int remaining)
    {
        for (int i = 0; i < _activeCapacity && remaining > 0; i++)
        {
            if (!_slots[i].IsEmpty) continue;
            int toAdd = Mathf.Min(maxStack, remaining);
            _slots[i] = new ItemStack(itemID, toAdd);
            remaining -= toAdd;
            OnSlotChanged?.Invoke(i);
        }
        return remaining;
    }

    private bool IsValidActive(int i) => i >= 0 && i < _activeCapacity;
}
