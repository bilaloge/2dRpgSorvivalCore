using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public class PlayerDataManager : MonoBehaviour
{
    // ── Inject ───────────────────────────────────────────────────────────────
    [Inject] private GameManager       _gameManager;
    [Inject] private InventoryManager  _inventoryManager;
    [Inject] private EquipmentManager  _equipmentManager;

    // ── Runtime Player Stats ─────────────────────────────────────────────────
    [Header("Runtime Player Stats")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;

    [Header("Last Known Location")]
    public string lastSceneName;
    public string lastSpawnID;

    [Header("Runtime Weapon Stats")]
    public int    customWeaponLevel       = 1;
    public float  currentWeaponDurability = 100f;
    public string weaponEnhancement       = "None";

    public List<NpcRelationshipData> npcRelationships = new List<NpcRelationshipData>();

    [Header("Referanslar")]
    [SerializeField] private PlayerStats playerStats;

    private string _savePath;

    // ── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "Character_Hero.json");
        InitializeStats();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void InitializeStats()
    {
        if (File.Exists(_savePath))
            LoadCharacter();
        else
            ResetToDefaultStats();
    }

    public void UpdateLastLocation(string sceneName, string spawnID)
    {
        lastSceneName = sceneName;
        lastSpawnID   = spawnID;
    }

    public void ResetToDefaultStats()
    {
        if (playerStats != null)
        {
            currentHealth = playerStats.TotalMaxHealth;
            currentMana   = playerStats.TotalMaxMana;
            currentEnergy = playerStats.TotalMaxEnergy;
        }

        lastSceneName         = "StartZone";
        lastSpawnID           = "Default";
        customWeaponLevel     = 1;
        currentWeaponDurability = 100f;
        weaponEnhancement     = "None";

        _gameManager?.HealthSystem?.NotifyAll();
    }

    /// <summary>
    /// Gün sonu (yatak / gece 02:00) çağrılır. Tüm sistem verilerini JSON'a yazar.
    /// </summary>
    public void SaveCharacter()
    {
        CharacterSaveData data = new CharacterSaveData
        {
            currentHealth  = this.currentHealth,
            currentMana    = this.currentMana,
            currentEnergy  = this.currentEnergy,
            lastSceneName  = this.lastSceneName,
            lastSpawnID    = this.lastSpawnID,
            infectionLevel = _gameManager.PlayerStats.infectionLevel,
            npcRelationships = new List<NpcRelationshipData>(npcRelationships)
        };

        data.customWeapon.weaponLevel       = customWeaponLevel;
        data.customWeapon.currentDurability = (int)currentWeaponDurability;
        data.customWeapon.enhancementType   = weaponEnhancement;

        // ── Envanter ve ekipman verisini yaz ──────────────────────────────
        _inventoryManager.WriteSaveData(data);
        _equipmentManager.WriteSaveData(data);

        File.WriteAllText(_savePath, JsonUtility.ToJson(data, true));
        Debug.Log("[PlayerDataManager] Karakter kaydedildi.");
    }

    /// <summary>
    /// Oyun başlangıcında ya da sahne yüklendiğinde JSON'dan okur.
    /// </summary>
    public void LoadCharacter()
    {
        if (!File.Exists(_savePath)) return;

        string json = File.ReadAllText(_savePath);
        CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

        this.currentHealth  = data.currentHealth;
        this.currentMana    = data.currentMana;
        this.currentEnergy  = data.currentEnergy;
        this.lastSceneName  = data.lastSceneName;
        this.lastSpawnID    = data.lastSpawnID;

        if (_gameManager?.PlayerStats != null)
            _gameManager.PlayerStats.infectionLevel = data.infectionLevel;

        this.customWeaponLevel      = data.customWeapon.weaponLevel;
        this.currentWeaponDurability = data.customWeapon.currentDurability;
        this.weaponEnhancement      = data.customWeapon.enhancementType;
        this.npcRelationships       = data.npcRelationships;

        // ── Envanter ve ekipman verisini oku ──────────────────────────────
        _inventoryManager.ReadSaveData(data);
        _equipmentManager.ReadSaveData(data);

        _gameManager?.HealthSystem?.NotifyAll();
        Debug.Log("[PlayerDataManager] Karakter yüklendi.");
    }
}
