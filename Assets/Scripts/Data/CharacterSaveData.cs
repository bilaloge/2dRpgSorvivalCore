using System.Collections.Generic;

// Unity'nin JSON sisteminin alt sınıfları okuyabilmesi için bu etiket şarttır.
[System.Serializable]
public class NpcRelationshipData
{
    public string npcID;            // Örn: "Blacksmith_Gorn"
    public int    relationshipLevel;
}

[System.Serializable]
public class CustomWeaponSaveData
{
    public int    weaponLevel       = 1;
    public int    currentDurability = 100;
    public string enhancementType   = "None"; // "Fire", "Poison", "Ice" vb.
}

[System.Serializable]
public class CharacterSaveData
{
    public string characterName;
    public int    characterLevel;

    [UnityEngine.Header("Anlık Statlar")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;

    [UnityEngine.Header("Kalıcı Limitler")]
    public int currentMaxHealth;
    public int currentMaxMana;
    public int currentMaxEnergy;

    [UnityEngine.Header("Durumlar")]
    public int infectionLevel;

    [UnityEngine.Header("Özel Silah Verisi")]
    public CustomWeaponSaveData customWeapon = new CustomWeaponSaveData();

    [UnityEngine.Header("NPC İlişkileri")]
    public List<NpcRelationshipData> npcRelationships = new List<NpcRelationshipData>();

    [UnityEngine.Header("Envanter")]
    // Her index bir slotu temsil eder.
    // Boş slot → inventoryItemIDs[i] = "" , inventoryItemAmounts[i] = 0
    public List<string> inventoryItemIDs     = new List<string>();
    public List<int>    inventoryItemAmounts = new List<int>();

    // Genişletilmiş kapasite kaydedilir; yükleme sırasında slot listesi
    // bu kapasiteye göre yeniden oluşturulur.
    public int inventoryCapacity = 20;

    [UnityEngine.Header("Ekipman")]
    public string equippedBoots  = string.Empty;
    public string equippedHead   = string.Empty;
    public string equippedBodyArmor = string.Empty;
    public string equippedRing1     = string.Empty;
    public string equippedRing2     = string.Empty;
    public string equippedPet       = string.Empty;

    [UnityEngine.Header("Konum")]
    public string lastSceneName = "StartZone";
    public string lastSpawnID   = "Default";
}
