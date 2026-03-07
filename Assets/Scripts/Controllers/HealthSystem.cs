using UnityEngine;
using System;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private CombatRules combatRules; // Kural Kitabı
    [SerializeField] private PlayerMovementController playerMovementController;

    public bool isInvulnerable;
    [SerializeField] private float damageGracePeriod = 0.5f;

    private float _hpAccumulator;
    private float _manaAccumulator;
    private float _energyAccumulator;
    private Coroutine _invulnCoroutine;


    // Olaylar (UI ve diğer sistemlerle bağlantı için)
    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnManaChanged;
    public event Action<int, int> OnEnergyChanged;

    private void Start()
    {
        NotifyAll();
    }
    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            RegenerateStats();
        }
    }
    public void StartTemporaryInvulnerability(float duration)
    {
        if (_invulnCoroutine != null) StopCoroutine(_invulnCoroutine);
        _invulnCoroutine = StartCoroutine(InvulnerabilityRoutine(duration));
    }
    private IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        _invulnCoroutine = null;
    }
    public void TakeDamage(float baseDamage)
    {
        if (isInvulnerable) return;

        int finalDamage = combatRules.CalculateDamage(baseDamage, playerStats.TotalArmor);
        PlayerDataManager.Instance.currentHealth = Mathf.Max(0, PlayerDataManager.Instance.currentHealth - finalDamage);

        _hpAccumulator = 0;
        NotifyHealthChange();

        Debug.Log($"Damage: {baseDamage} → Final: {finalDamage}");

        StartTemporaryInvulnerability(damageGracePeriod);

        if (PlayerDataManager.Instance.currentHealth <= 0)
            playerMovementController.Die();
    }
    private void RegenerateStats()
    {
        var data = PlayerDataManager.Instance;

        if (data.currentHealth < playerStats.TotalMaxHealth)
        {
            _hpAccumulator += playerStats.healthRegenRate * Time.deltaTime;

            if (_hpAccumulator >= 1f)
            {
                int gain = Mathf.FloorToInt(_hpAccumulator);
                data.currentHealth = Mathf.Min(data.currentHealth + gain, playerStats.TotalMaxHealth);
                _hpAccumulator -= gain; // Kalan küsüratı bir sonraki kareye aktar
                NotifyHealthChange();
            }
        }

        if (data.currentMana < playerStats.currentMaxMana)
        {
            _manaAccumulator += playerStats.manaRegenRate * Time.deltaTime;

            if (_manaAccumulator >= 1f)
            {
                int gain = Mathf.FloorToInt(_manaAccumulator);
                data.currentMana = Mathf.Min(data.currentMana + gain, playerStats.TotalMaxMana);
                _manaAccumulator -= gain;
                NotifyManaChange();
            }
        }

        if (data.currentEnergy < playerStats.TotalMaxHealth)
        {
            _energyAccumulator += playerStats.energyRegenRate * Time.deltaTime;

            if (_energyAccumulator >= 1f)
            {
                int gain = Mathf.FloorToInt(_energyAccumulator);
                data.currentEnergy = Mathf.Min(data.currentEnergy + gain, playerStats.TotalMaxEnergy);
                _energyAccumulator -= gain;
                NotifyEnergyChange();
            }
        }
    }
    public void SetInvulnerable(bool value)
    {
        // Eğer bir Coroutine çalışıyorsa ve biz manuel 'false' çekiyorsak, çakışma olmaması için Coroutine'i durduruyoruz.
        if (value == false && _invulnCoroutine != null)
        {
            StopCoroutine(_invulnCoroutine);
            _invulnCoroutine = null;
        }
        isInvulnerable = value;
    }
    // Can yenileme, potion vs
    public void Heal(int amount)
    {
        var data = PlayerDataManager.Instance;
        data.currentHealth = Mathf.Clamp(data.currentHealth + amount, 0, playerStats.TotalMaxHealth);

        // Can değiştiği için UI'ı bilgilendiriyoruz.
        NotifyHealthChange();
    }
    public void ReduceEnergy(int amount)
    {
        PlayerDataManager.Instance.currentEnergy = Mathf.Max(0, PlayerDataManager.Instance.currentEnergy - amount);
        NotifyEnergyChange();
    }

    // Health değerini UI'ya bildir
    public void NotifyHealthChange()
    {
        OnHealthChanged?.Invoke(PlayerDataManager.Instance.currentHealth, playerStats.TotalMaxHealth);
    }
    public void NotifyManaChange()
    {
        OnManaChanged?.Invoke(PlayerDataManager.Instance.currentMana, playerStats.TotalMaxMana);
    }

    public void NotifyEnergyChange()
    {
        OnEnergyChanged?.Invoke(PlayerDataManager.Instance.currentEnergy, playerStats.TotalMaxEnergy);
    }
    //Gelecekte buff uygulamaları için ai önerisi
    //private System.Collections.IEnumerator TemporaryBuffCoroutine(float extraArmor, float duration)
    //{
    //    playerStats.Armor += extraArmor;
    //    yield return new WaitForSeconds(duration);
    //    playerStats.Armor -= extraArmor;
    //}
    public void NotifyAll()
    {
        NotifyHealthChange();
        NotifyManaChange();
        NotifyEnergyChange();
    }
}
