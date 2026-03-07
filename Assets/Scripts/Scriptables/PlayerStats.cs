using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "PlayerStats/Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Base Stats (Level 1)")]
    public int baseMaxHealth = 100;
    public int baseMaxMana = 50;
    public int baseMaxEnergy = 100;

    [Header("Current Limits (Upgraded)")]
    // Bunlar seviye aldýkça artacak olan "tavan" deðerler
    public int currentMaxHealth = 100;
    public int currentMaxMana = 50;
    public int currentMaxEnergy = 100;

    [Header("Temporary/Equipment Modifiers")]
    // Pot içince veya zýrh giyince burayý artýracaðýz
    [HideInInspector] public int modifiedMaxHealth;
    [HideInInspector] public int modifiedArmor;
    [HideInInspector] public int modifiedMaxMana;
    [HideInInspector] public int modifiedMaxEnergy;
    
    public int TotalMaxHealth => baseMaxHealth + modifiedMaxHealth;
    public int TotalArmor => Armor + modifiedArmor;
    public int TotalMaxMana => baseMaxMana + modifiedMaxMana;
    public int TotalMaxEnergy => baseMaxEnergy + modifiedMaxEnergy;

    [Header("Deffence Stats")]
    public int Armor;
    public int MagicResist;
    
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

    [Header("Infection System")]
    public int infectionLevel;

    // Level atladýðýnda sýnýrlarý güncellemek için metod
    public void UpdateMaxStats(int healthBonus, int manaBonus)
    {
        currentMaxHealth += healthBonus;
        currentMaxMana += manaBonus;
    }
}