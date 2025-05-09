using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float baseMaxHealth;
    public float currentMaxHealth;
    public float currentHealth;

    public float baseMaxMana;
    public float currentMaxMana;
    public float currentMana;

    public float baseMaxEnergy;
    public float currentMaxEnergy;
    public float currentEnergy;

    public float Armor;
    public float MagicResist;
    public float AttackPower;
    public float MoveSpeed;

    private void Awake()
    {
        LoadStats(); // Save sisteminden çekebilir

        currentMaxHealth = baseMaxHealth;
        currentHealth = currentMaxHealth;

        currentMaxMana = baseMaxMana;
        currentMana = currentMaxMana;

        currentMaxEnergy = baseMaxEnergy;
        currentEnergy = currentMaxEnergy;

    }

    public void IncreaseMaxHealth(float amount)
    {
        currentMaxHealth += amount;
    }
    public void IncreaseMaxMana(float amount)
    {
        currentMaxMana += amount;
    }
    public void IncreaseMaxEnergy(float amount)
    {
        currentMaxEnergy += amount;
    }
    public void LevelUpStats(float levelIncrease)
    {
        IncreaseMaxHealth(levelIncrease);
        IncreaseMaxMana(levelIncrease);
        IncreaseMaxEnergy(levelIncrease);
    }

    public void LoadStats()
    {
        // Save sisteminden veri çek
    }

    public void SaveStats()
    {
        // Save sistemine veri yaz
    }
}
