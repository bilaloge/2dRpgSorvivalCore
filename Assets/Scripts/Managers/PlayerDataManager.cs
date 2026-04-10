using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public class PlayerDataManager : MonoBehaviour
{
    [Inject] private GameManager _gameManager;

    [Header("Runtime Player Stats")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;

    [Header("Last Known Location")]
    public string lastSceneName;
    public string lastSpawnID;

    [Header("Runtime Weapon Stats")]
    public int customWeaponLevel = 1;
    public float currentWeaponDurability = 100f;
    public string weaponEnhancement = "None";

    public List<NpcRelationshipData> npcRelationships = new List<NpcRelationshipData>();

    [Header("Referanslar")]
    [SerializeField] private PlayerStats playerStats;

    private string _savePath;

    private void Awake()
    {
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
    // Yataða yatýldýðýnda çaðýracaðýmýz metod
    public void UpdateLastLocation(string sceneName, string spawnID)
    {
        lastSceneName = sceneName;
        lastSpawnID = spawnID;
    }
    public void ResetToDefaultStats()//baþlangýç ayarlarý
    {
        if (this.playerStats != null)
        {
            currentHealth = this.playerStats.TotalMaxHealth;
            currentMana = this.playerStats.TotalMaxMana;
            currentEnergy = this.playerStats.TotalMaxEnergy;
        }

        lastSceneName = "StartZone";
        lastSpawnID = "Default";

        // Geri kalan her þey (Silah seviyesi vb.) zaten statik sayýlar
        customWeaponLevel = 1;
        currentWeaponDurability = 100f;
        weaponEnhancement = "None";

        // HealthSystem sahne yüklendiðinde oluþacaðý için null-check þart
        _gameManager?.HealthSystem?.NotifyAll();
    }
    public void SaveCharacter()
    {
        CharacterSaveData data = new CharacterSaveData
        {
            currentHealth = this.currentHealth,
            currentMana = this.currentMana,
            currentEnergy = this.currentEnergy,
            lastSceneName = this.lastSceneName,
            lastSpawnID = this.lastSpawnID,
            infectionLevel = _gameManager.PlayerStats.infectionLevel,
            npcRelationships = new List<NpcRelationshipData>(npcRelationships)
        };
        data.customWeapon.weaponLevel = customWeaponLevel;
        data.customWeapon.currentDurability = (int)currentWeaponDurability;
        data.customWeapon.enhancementType = weaponEnhancement;

        File.WriteAllText(_savePath, JsonUtility.ToJson(data, true));
    }
    public void LoadCharacter()
    {
        if (!File.Exists(_savePath)) return;
        string json = File.ReadAllText(_savePath);
        CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

        this.currentHealth = data.currentHealth;
        this.currentMana = data.currentMana;
        this.currentEnergy = data.currentEnergy;

        this.lastSceneName = data.lastSceneName;
        this.lastSpawnID = data.lastSpawnID;

        // Enfeksiyonu runtime stats'a aktar
        if (_gameManager != null && _gameManager.PlayerStats != null)
            _gameManager.PlayerStats.infectionLevel = data.infectionLevel;

        this.customWeaponLevel = data.customWeapon.weaponLevel;
        this.currentWeaponDurability = data.customWeapon.currentDurability;
        this.weaponEnhancement = data.customWeapon.enhancementType;
        this.npcRelationships = data.npcRelationships;

        _gameManager?.HealthSystem?.NotifyAll();
    }
}
