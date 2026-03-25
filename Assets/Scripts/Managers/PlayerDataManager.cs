using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Anlýk Player Deðerleri")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;

    [Header("Referanslar")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private HealthSystem healthSystem;

    [Header("Özel Silah Verileri")]
    public int customWeaponLevel = 1;
    public float currentWeaponDurability = 100f;
    public string weaponEnhancement = "None";

    [Header("Sosyal Ýliþkiler")]
    public List<NpcRelationshipData> npcRelationships = new List<NpcRelationshipData>();

    private string _savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _savePath = Path.Combine(Application.persistentDataPath, "Character_Hero.json");

        // Baþlangýçta verileri hazýrla
        InitializeStats();
    }
    public void InitializeStats()
    {
        // Eðer kayýt varsa oradan yükle, yoksa SO'dan fulle
        if (File.Exists(_savePath))
        {
            LoadCharacter();
        }
        else
        {
            ResetToDefaultStats();//eðer kayýt bulunmazsa baþlangýç ayarlarý
        }
    }
    public void ResetToDefaultStats()//baþlangýç ayarlarý
    {
        currentHealth = playerStats.TotalMaxHealth;
        currentMana = playerStats.TotalMaxMana;
        currentEnergy = playerStats.TotalMaxEnergy;
        customWeaponLevel = 1;
        currentWeaponDurability = 100f;
        weaponEnhancement = "None";
        playerStats.infectionLevel = 0;
        healthSystem.NotifyAll();
    }
    public void SaveCharacter()
    {
        CharacterSaveData data = new CharacterSaveData();

        // Temel Statlar
        data.currentHealth = this.currentHealth;
        data.currentMana = this.currentMana;
        data.currentEnergy = this.currentEnergy;
        data.infectionLevel = playerStats.infectionLevel;

        // Özel Silah
        data.customWeapon.weaponLevel = customWeaponLevel;
        data.customWeapon.currentDurability = (int)currentWeaponDurability;
        data.customWeapon.enhancementType = weaponEnhancement;

        // NPC Ýliþkileri
        data.npcRelationships = new List<NpcRelationshipData>(npcRelationships);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
        Debug.Log("Karakter Verileri Kaydedildi.");
    }
    public void LoadCharacter()
    {
        if (!File.Exists(_savePath)) return;

        string json = File.ReadAllText(_savePath);
        CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

        this.currentHealth = data.currentHealth;
        this.currentMana = data.currentMana;
        this.currentEnergy = data.currentEnergy;

        playerStats.infectionLevel = data.infectionLevel;

        this.customWeaponLevel = data.customWeapon.weaponLevel;
        this.currentWeaponDurability = data.customWeapon.currentDurability;
        this.weaponEnhancement = data.customWeapon.enhancementType;

        this.npcRelationships = data.npcRelationships;

        healthSystem.NotifyAll(); // UI'ý yeni verilerle güncelle
        Debug.Log("Karakter, Enfeksiyon ve Silah verileri baþarýyla yüklendi.");
    }
}
