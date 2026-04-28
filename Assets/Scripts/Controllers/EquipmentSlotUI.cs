using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image           itemIcon;
    [SerializeField] private Image           emptyIcon;
    [SerializeField] private TextMeshProUGUI slotLabel;

    // Inspector'dan her slot objesinde ayrı ayrı ayarlanır
    [SerializeField] private EquipmentSlotType slotType;

    [Inject] private EquipmentManager _equipmentManager;
    [Inject] private InventoryManager _inventoryManager;
    [Inject] private ItemDatabase     _itemDatabase;

    private void Start()
    {
        if (slotLabel != null) slotLabel.text = SlotDisplayName(slotType);
    }

    public void Refresh(string itemID)
    {
        bool isEmpty = string.IsNullOrEmpty(itemID);

        if (emptyIcon != null) emptyIcon.enabled = isEmpty;

        if (isEmpty)
        {
            if (itemIcon != null) itemIcon.enabled = false;
            return;
        }

        ItemData data = _itemDatabase?.Get(itemID);
        if (itemIcon != null)
        {
            itemIcon.sprite  = data?.icon;
            itemIcon.enabled = data?.icon != null;
        }
    }

    // Inventory slotundan sürüklenip bu slota bırakıldığında
    public void OnDrop(PointerEventData e)
    {
        InventorySlotUI from = e.pointerDrag?.GetComponent<InventorySlotUI>();
        if (from == null) return;

        ItemStack stack = _inventoryManager.GetSlot(from.SlotIndex);
        if (stack.IsEmpty) return;

        ItemData data = _itemDatabase.Get(stack.ItemID);
        if (data == null || data.allowedEquipmentSlot != slotType) return;

        _equipmentManager.Equip(from.SlotIndex);
    }

    // Sağ tık ile slottaki itemi inventory'ye geri al
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Right) return;
        _equipmentManager.Unequip(slotType);
    }

    private string SlotDisplayName(EquipmentSlotType type) => type switch
    {
        EquipmentSlotType.Boots  => "Botlar",
        EquipmentSlotType.Head   => "Kafalık",
        EquipmentSlotType.BodyArmor => "Zırh",
        EquipmentSlotType.Ring1     => "Yüzük 1",
        EquipmentSlotType.Ring2     => "Yüzük 2",
        EquipmentSlotType.Pet       => "Pet",
        _                           => ""
    };
}
