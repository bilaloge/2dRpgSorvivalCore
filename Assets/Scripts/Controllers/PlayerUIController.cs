using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider energySlider;

    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {

        // Barlarý baþlat
        healthSlider.maxValue = playerStats.currentMaxHealth;
        manaSlider.maxValue = playerStats.currentMaxMana;
        energySlider.maxValue = playerStats.currentMaxEnergy;

        // Barlarý mevcut deðerler ile baþlat
        healthSlider.value = healthSystem.currentHealth;
        manaSlider.value = healthSystem.currentMana;
        energySlider.value = healthSystem.currentEnergy;

        // HealthSystem eventlerini dinle
        healthSystem.OnHealthChanged += UpdateHealthBar;
        healthSystem.OnManaChanged += UpdateManaBar;
        healthSystem.OnEnergyChanged += UpdateEnergyBar;
    }

    private void UpdateHealthBar(float current, float max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }


    private void UpdateManaBar(float current, float max)
    {
        manaSlider.maxValue = max;
        manaSlider.value = current;
    }


    private void UpdateEnergyBar(float current, float max)
    {
        energySlider.maxValue = max;
        energySlider.value = current;
    }


    private void OnDestroy()
    {
        // Eventleri dinlerken unutulmamalarý için OnDestroy'da eventleri un-subscribe etmek önemlidir.
        healthSystem.OnHealthChanged -= UpdateHealthBar;
        healthSystem.OnManaChanged -= UpdateManaBar;
        healthSystem.OnEnergyChanged -= UpdateEnergyBar;
    }
}
