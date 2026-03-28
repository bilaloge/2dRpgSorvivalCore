using System.Collections.Generic;

// Unity'nin JSON sisteminin alt sýnýflarý okuyabilmesi için bu etiket þarttýr.
[System.Serializable]
public class NpcRelationshipData
{
    public string npcID; // Örn: "Blacksmith_Gorn"
    public int relationshipLevel;
}

[System.Serializable]
public class CustomWeaponSaveData
{
    public int weaponLevel = 1;
    public int currentDurability = 100; // Sadece anlýk kýrýlma durumu kaydedilir
    public string enhancementType = "None"; // "Fire", "Poison", "Ice" vb.
}

[System.Serializable]
public class CharacterSaveData
{
    public string characterName;
    public int characterLevel;

    [UnityEngine.Header("Anlýk Statlar")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;

    [UnityEngine.Header("Kalýcý Limitler")]
    public int currentMaxHealth;
    public int currentMaxMana;
    public int currentMaxEnergy;

    [UnityEngine.Header("Durumlar")]
    public int infectionLevel;

    [UnityEngine.Header("Özel Silah Verisi")]
    public CustomWeaponSaveData customWeapon = new CustomWeaponSaveData();

    [UnityEngine.Header("NPC Ýliþkileri")]
    public List<NpcRelationshipData> npcRelationships = new List<NpcRelationshipData>();

    [UnityEngine.Header("Envanter")]
    public List<string> inventoryItemIDs = new List<string>();
    public List<int> inventoryItemAmounts = new List<int>();

    public string lastSceneName; // Örn: "StartZone"
    public string lastSpawnID; // Örn: "Home_Bed_Spawn"
}