using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovementController playerMovementController;

    public bool isInvulnerable;

    public float currentHealth;
    public float currentMana;
    public float currentEnergy;

    // Olaylar (UI ve diğer sistemlerle bağlantı için)
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
    public event Action<float, float> OnEnergyChanged;

    private void Awake()
    {
        currentHealth = playerStats.currentMaxHealth;
        currentMana = playerStats.currentMaxMana;
        currentEnergy = playerStats.currentMaxEnergy;

        NotifyHealthChange();
        NotifyManaChange();
        NotifyEnergyChange();

    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            RegenerateStats();
        }
    }

    private void RegenerateStats()
    {
        if (currentHealth < playerStats.currentMaxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + playerStats.healthRegenRate * Time.deltaTime, playerStats.currentMaxHealth);
            NotifyHealthChange();
        }

        if (currentMana < playerStats.currentMaxMana)
        {
            currentMana = Mathf.Min(currentMana + playerStats.manaRegenRate * Time.deltaTime, playerStats.currentMaxMana);
            NotifyManaChange();
        }

        if (currentEnergy < playerStats.currentMaxEnergy)
        {
            currentEnergy = Mathf.Min(currentEnergy + playerStats.energyRegenRate * Time.deltaTime, playerStats.currentMaxEnergy);
            NotifyEnergyChange();
        }
    }
    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }

    public void TakeDamage(float baseDamage)
    {
        if (isInvulnerable) return;

        float finalDamage = Mathf.Max(0, baseDamage - playerStats.Armor); // Sadece armor uygulanıyor. Direnç eklenecekse burda yap.
        currentHealth -= finalDamage;

        Debug.Log($"Damage: {baseDamage} → Final: {finalDamage}");

        currentHealth = Mathf.Clamp(currentHealth, 0, playerStats.currentMaxHealth);
        NotifyHealthChange();

        if (currentHealth <= 0)
        {
            playerMovementController.Die();
        }
    }

    // Can yenileme, potion vs
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, playerStats.currentMaxHealth);
        NotifyHealthChange();
    }

    // Health değerini UI'ya bildir
    private void NotifyHealthChange()
    {
        OnHealthChanged?.Invoke(currentHealth, playerStats.currentMaxHealth);
    }
    private void NotifyManaChange()
    {
        OnManaChanged?.Invoke(currentMana, playerStats.currentMaxMana);
    }

    private void NotifyEnergyChange()
    {
        OnEnergyChanged?.Invoke(currentEnergy, playerStats.currentMaxEnergy);
    }


    //Gelecekte buff uygulamaları için ai önerisi
    //private System.Collections.IEnumerator TemporaryBuffCoroutine(float extraArmor, float duration)
    //{
    //    playerStats.Armor += extraArmor;
    //    yield return new WaitForSeconds(duration);
    //    playerStats.Armor -= extraArmor;
    //}
    public void UseDash()
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= playerMovementController.dashEnergyCost;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, playerStats.currentMaxEnergy);
            NotifyEnergyChange();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => playerStats.currentMaxHealth;
}
