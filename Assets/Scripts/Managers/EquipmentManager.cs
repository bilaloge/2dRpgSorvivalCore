using System;
using System.Threading;
using UnityEngine;
using Zenject;

/// <summary>
/// Karakter ekipman sistemi. Inventory menüsü açıldığında gösterilecek slotlar:
///   • MainHand   — ana el silahı
///   • OffHand    — ikinci el silah / kalkan
///   • BodyArmor  — tek parça zırh
///   • Ring1      — 1. yüzük
///   • Ring2      — 2. yüzük
///   • Pet        — evcil hayvan slotu
///
/// Kurallar:
///   - Slota yalnızca allowedEquipmentSlot eşleşen item takılabilir.
///   - Takılan item inventory'den çıkar; çıkarılan geri döner.
///   - Inventory doluysa çıkarma işlemi bloklanır.
/// </summary>
public class EquipmentManager : IInitializable
{
    [Inject] private InventoryManager _inventory;
    [Inject] private ItemDatabase     _itemDatabase;

    // Her slot bir itemID tutar (boşsa null/empty)
    private string _boots = string.Empty;
    private string _head = string.Empty;
    private string _bodyArmor  = string.Empty;
    private string _ring1      = string.Empty;
    private string _ring2      = string.Empty;
    private string _pet        = string.Empty;

    /// <summary>Herhangi bir equipment slotu değiştiğinde tetiklenir.</summary>
    public event Action<EquipmentSlotType, string> OnEquipmentChanged;

    public void Initialize() { /* başlangıç değerleri zaten boş */ }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inventory'deki slotIndex'teki itemi ilgili equipment slotuna takar.
    /// Slotta zaten bir item varsa swap yapar (eski item inventory'ye döner).
    /// </summary>
    public bool Equip(int inventorySlotIndex)
    {
        ItemStack stack = _inventory.GetSlot(inventorySlotIndex);
        if (stack.IsEmpty) return false;

        ItemData data = _itemDatabase.Get(stack.ItemID);
        if (data == null) return false;

        EquipmentSlotType target = data.allowedEquipmentSlot;
        if (target == EquipmentSlotType.None)
        {
            Debug.LogWarning($"[EquipmentManager] '{data.displayName}' hiçbir " +
                              "equipment slotuna takılamaz.");
            return false;
        }

        string currentInSlot = GetSlotContent(target);

        // Slotta item varsa önce geri inventory'e almayı dene
        if (!string.IsNullOrEmpty(currentInSlot))
        {
            if (!_inventory.CanAddItem(currentInSlot, 1))
            {
                Debug.LogWarning("[EquipmentManager] Inventory dolu, swap yapılamıyor.");
                return false;
            }
            _inventory.AddItem(currentInSlot, 1);
        }

        // Inventory'den çıkar, slota yaz
        _inventory.RemoveAt(inventorySlotIndex, 1);
        SetSlotContent(target, stack.ItemID);
        OnEquipmentChanged?.Invoke(target, stack.ItemID);
        return true;
    }

    /// <summary>
    /// Equipment slotundaki itemi çıkarıp inventory'e geri koyar.
    /// </summary>
    public bool Unequip(EquipmentSlotType slot)
    {
        string itemID = GetSlotContent(slot);
        if (string.IsNullOrEmpty(itemID)) return false;

        if (!_inventory.CanAddItem(itemID, 1))
        {
            Debug.LogWarning("[EquipmentManager] Inventory dolu, çıkarma yapılamıyor.");
            return false;
        }

        _inventory.AddItem(itemID, 1);
        SetSlotContent(slot, string.Empty);
        OnEquipmentChanged?.Invoke(slot, string.Empty);
        return true;
    }

    public string GetSlotContent(EquipmentSlotType slot) => slot switch
    {
        EquipmentSlotType.Boots => _boots,
        EquipmentSlotType.Head => _head,
        EquipmentSlotType.BodyArmor => _bodyArmor,
        EquipmentSlotType.Ring1     => _ring1,
        EquipmentSlotType.Ring2     => _ring2,
        EquipmentSlotType.Pet       => _pet,
        _                           => string.Empty
    };

    // ── Kayıt / Yükleme ───────────────────────────────────────────────────────

    public void WriteSaveData(CharacterSaveData data)
    {
        data.equippedBoots     = _boots;
        data.equippedHead      = _head;
        data.equippedBodyArmor = _bodyArmor;
        data.equippedRing1     = _ring1;
        data.equippedRing2     = _ring2;
        data.equippedPet       = _pet;
    }

    public void ReadSaveData(CharacterSaveData data)
    {
        _boots = data.equippedBoots ?? string.Empty;
        _head = data.equippedHead ?? string.Empty;
        _bodyArmor = data.equippedBodyArmor ?? string.Empty;
        _ring1     = data.equippedRing1     ?? string.Empty;
        _ring2     = data.equippedRing2     ?? string.Empty;
        _pet       = data.equippedPet       ?? string.Empty;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void SetSlotContent(EquipmentSlotType slot, string itemID)
    {
        switch (slot)
        {
            case EquipmentSlotType.Boots: _boots = itemID; break;
            case EquipmentSlotType.Head: _head = itemID; break;
            case EquipmentSlotType.BodyArmor: _bodyArmor = itemID; break;
            case EquipmentSlotType.Ring1:     _ring1     = itemID; break;
            case EquipmentSlotType.Ring2:     _ring2     = itemID; break;
            case EquipmentSlotType.Pet:       _pet       = itemID; break;
        }
    }
}
