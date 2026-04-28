using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tüm ItemData asset'lerinin merkezi kayıt defteri.
/// Assets/Data/ altında tek bir asset olarak oluşturulur.
/// Zenject'e ScriptableObjectInstaller üzerinden bağlanır.
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> _items = new List<ItemData>();

    private Dictionary<string, ItemData> _lookup;

    /// <summary>
    /// Lookup sözlüğünü oluşturur. ScriptableObjectInstaller.InstallBindings()
    /// içinde ya da ilk Get() çağrısında otomatik çalışır.
    /// </summary>
    public void Initialize()
    {
        _lookup = new Dictionary<string, ItemData>(_items.Count);
        foreach (var item in _items)
        {
            if (item == null) continue;
            if (_lookup.ContainsKey(item.itemID))
            {
                Debug.LogError($"[ItemDatabase] Mükerrer itemID: '{item.itemID}'. " +
                               $"Her item benzersiz bir ID'ye sahip olmalı.");
                continue;
            }
            _lookup[item.itemID] = item;
        }
    }

    /// <summary>
    /// itemID'ye göre ItemData döner. Bulunamazsa null + uyarı.
    /// </summary>
    public ItemData Get(string itemID)
    {
        if (_lookup == null) Initialize(); // ilk çağrıda lazy init

        if (_lookup.TryGetValue(itemID, out ItemData data))
            return data;

        Debug.LogWarning($"[ItemDatabase] '{itemID}' bulunamadı.");
        return null;
    }

    public bool Contains(string itemID)
    {
        if (_lookup == null) Initialize();
        return _lookup.ContainsKey(itemID);
    }

    public IReadOnlyList<ItemData> AllItems => _items;
}
