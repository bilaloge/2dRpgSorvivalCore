using UnityEngine;
using System;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private CombatRules combatRules; // Kural Kitabı
    [SerializeField] private PlayerMovementController playerMovementController;

    [Header("Savaş Ayarları")]
    public float damageGracePeriod = 1f; // Hasar aldıktan sonraki kısa ölümsüzlük süresi
    private bool isInvulnerable = false;
    private Coroutine _invulnCoroutine;

    //UI Eventleri (current, max)
    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnManaChanged;
    public event Action<int, int> OnEnergyChanged;
    public event Action OnPlayerDied;

    // Yenilenme (Regeneration) için zamanlayıcılar
    private float _hpAccumulator;
    private float _manaAccumulator;
    private float _energyAccumulator;
    private void Awake()
    {
        // EKLENDİ: Instance ataması
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
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
    //SAVAŞ VE HASAR SİSTEMİ
    public void TakeDamage(float baseDamage)
    {
        if (isInvulnerable) return;

        int finalDamage = combatRules.CalculateDamage(baseDamage, playerStats.TotalArmor);

        var data = PlayerDataManager.Instance;
        data.currentHealth = Mathf.Max(0, data.currentHealth - finalDamage);

        _hpAccumulator = 0;// Hasar alınca regen duruyor
        NotifyHealthChange();

        Debug.Log($"Damage: {baseDamage} → Final: {finalDamage}");

        StartTemporaryInvulnerability(damageGracePeriod);

        if (data.currentHealth <= 0)
        {
            playerMovementController.Die();
            OnPlayerDied?.Invoke();
        }
    }
    public void TakeEffectDamage(int amount)//zırhtan bağımsız zehir hasarı, aynı zamanda ölümsüzlük süresini de tetiklemez.
    {
        var data = PlayerDataManager.Instance;
        data.currentHealth = Mathf.Max(0, data.currentHealth - amount);

        _hpAccumulator = 0; // Zehir can yenilenmesini durdurur
        NotifyHealthChange();

        Debug.Log($"Efekt Hasarı (Zehir): {amount}. Kalan Can: {data.currentHealth}");

        if (data.currentHealth <= 0)
        {
            playerMovementController.Die();
            OnPlayerDied?.Invoke();
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
                _hpAccumulator -= gain;
                NotifyHealthChange();
            }
        }
        if (data.currentMana < playerStats.TotalMaxMana)
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
        if (data.currentEnergy < playerStats.TotalMaxEnergy)
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
    // Can yenileme, potion vs
    public void Heal(int amount)
    {
        var data = PlayerDataManager.Instance;
        data.currentHealth = Mathf.Clamp(data.currentHealth + amount, 0, playerStats.TotalMaxHealth);
        NotifyHealthChange();
    }
    public void ReduceEnergy(int amount)
    {
        var data = PlayerDataManager.Instance;
        data.currentEnergy = Mathf.Max(0, data.currentEnergy - amount);
        NotifyEnergyChange();
    }
    //GÜN SONU / UYKU SİSTEMİ
    public void SleepAndRestore(bool sleptInBed)
    {
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.ClearAllBuffs();
        }

        var data = PlayerDataManager.Instance;

        data.currentHealth = playerStats.TotalMaxHealth;
        data.currentMana = playerStats.TotalMaxMana;

        //Enfeksiyonun yayılma Mekaniği
        if (playerStats.infectionLevel >= 1)
        {
            playerStats.infectionLevel++;
            Debug.Log($"Enfeksiyon yayıldı. Seviye: {playerStats.infectionLevel}");
        }

        NotifyAll();

        Debug.Log(sleptInBed ? "Yatakta güvenle uyanıldı." : "Dışarıda sızıp kalındı, gün bitti.");
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
    public void NotifyAll()
    {
        NotifyHealthChange();
        NotifyManaChange();
        NotifyEnergyChange();
    }
}
