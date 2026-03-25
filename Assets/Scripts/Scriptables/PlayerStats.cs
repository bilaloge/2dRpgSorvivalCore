using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "PlayerStats/Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Kalýcý Statlar (Kayýt Dosyasýna Yazýlacaklar)")]
    public int baseMaxHealth = 100;
    public int baseMaxMana = 50;
    public int baseMaxEnergy = 100;

    [Header("Seviye atlanýnca artacak miktar)")]
    public int currentMaxHealth = 10;
    public int currentMaxMana = 10;
    public int currentMaxEnergy = 10;

    public int infectionLevel;

    [Header("Defense Stats (Base)")]
    public int baseArmor;
    public int baseMagicResist;

    [Header("Movement")]
    public float MoveSpeed = 5f;

    [Header("Combat Stats")]
    public int AttackPower;
    public int CritChance;
    public float CritDamageMultiplier = 1.5f;
    public int Piercing;

    [Header("Regeneration Rates")]
    public sbyte healthRegenRate = 1;
    public sbyte manaRegenRate = 1;
    public sbyte energyRegenRate = 1;

    [Header("Geçici Statlar: EKÝPMAN (Kayýt Edilmeyecek)")]
    // Zýrh ve silahlardan gelen bonuslar. Karakter zýrh giydiðinde EquipmentManager burayý günceller.
    [HideInInspector] public int equipmentMaxHealthBonus;
    [HideInInspector] public int equipmentArmorBonus;
    [HideInInspector] public int equipmentMaxEnergyBonus;
    [HideInInspector] public int equipmentMaxManaBonus;

    [Header("Geçici Statlar: POT / BUFF (Kayýt Edilmeyecek)")]
    // Ýksirlerden veya zehirlerden gelen süreli etkiler. Uyunulduðunda sýfýrlanacak kýsým burasý.
    [HideInInspector] public int buffMaxHealthBonus;
    [HideInInspector] public int buffArmorBonus;
    [HideInInspector] public int buffMaxEnergyBonus;
    [HideInInspector] public int buffMaxManaBonus;

    [Header("Toplam Hesaplamalar (Sistemler Bunlarý Okur)")]
    // HealthSystem ve Hasar formülleri sadece bu Total deðerleri okumalýdýr.
    public int TotalMaxHealth => currentMaxHealth + equipmentMaxHealthBonus + buffMaxHealthBonus;
    public int TotalMaxMana => currentMaxMana + equipmentMaxManaBonus + buffMaxManaBonus;
    public int TotalMaxEnergy => currentMaxEnergy + equipmentMaxEnergyBonus + buffMaxEnergyBonus;
    public int TotalArmor => baseArmor + equipmentArmorBonus + buffArmorBonus;

    //Kalýcý Geliþim (Level Up vb.)
    public void AddPermanentStats(int healthBonus, int manaBonus)
    {
        currentMaxHealth += healthBonus;
        currentMaxMana += manaBonus;
    }
}