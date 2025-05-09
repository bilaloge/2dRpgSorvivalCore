using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovementController playerMovementController;

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

    // Hasar alırken zırh ve direnç dikkate alınır.
    public void TakeDamage(float baseDamage)
    {
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

    // Can yenileme regeneration, potion vs
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

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => playerStats.currentMaxHealth;
}
