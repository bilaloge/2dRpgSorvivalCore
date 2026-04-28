using UnityEngine;

// Her item tipi için bir ScriptableObject asset'i oluşturulur.
// Assets/Data/Items/ klasörü altında tutulması önerilir.
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Kimlik")]
    [Tooltip("Kayıt sisteminde kullanılan benzersiz ID. Asla değiştirilmemeli.")]
    public string itemID;
    public string displayName;
    [TextArea] public string description;

    [Header("Görsel")]
    public Sprite icon;

    [Header("Tip")]
    public ItemType itemType;

    [Header("Stack Kuralları")]
    [Tooltip("Weapon, Armor, Tool, Accessory otomatik olarak 1 kabul edilir. " +
             "Diğer tipler için max 99.")]
    public int maxStackSize = 99;

    [Header("Ekipman Kısıtlaması")]
    [Tooltip("Bu item hangi equipment slotuna takılabilir?")]
    public EquipmentSlotType allowedEquipmentSlot = EquipmentSlotType.None;

    [Header("Değer")]
    public int basePrice = 0;

    // Silah, zırh, alet ve aksesuar tipleri asla stack olmaz.
    public bool IsStackable => itemType != ItemType.Weapon
                            && itemType != ItemType.Armor
                            && itemType != ItemType.Tool
                            && itemType != ItemType.Accessory;

    // Gerçek max stack: stacklanamayan itemler için her zaman 1 döner.
    public int EffectiveMaxStack => IsStackable ? Mathf.Clamp(maxStackSize, 1, 99) : 1;

#if UNITY_EDITOR
    // Inspector'da itemID boş bırakılırsa asset adını otomatik atar.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemID))
            itemID = name;
    }
#endif
}


public enum ItemType
{
    // Stacklenemez
    Weapon,
    Armor,
    Tool,
    Accessory,

    // Stacklenebilir
    Resource,       // Ham madde (odun, taş, maden vb.)
    Consumable,     // Yiyecek, iksir
    Seed,           // Ekim için tohum
    CraftedGoods,   // İşlenmiş ürün (bez, levha vb.)
    QuestItem,      // Hikaye/görev itemi
    Misc            // Diğer
}

public enum EquipmentSlotType
{
    None,           // Ekipman slotuna girmez
    Boots,          // Ayakkabılar
    Head,           // Kafalık
    BodyArmor,      // Tek parça zırh
    Ring1,          // 1. yüzük
    Ring2,          // 2. yüzük
    Pet             // Evcil hayvan
}
