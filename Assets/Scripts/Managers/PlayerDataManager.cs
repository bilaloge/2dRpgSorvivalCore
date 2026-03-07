using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Anlýk Player Deðerleri")]
    public int currentHealth;
    public int currentMana;
    public int currentEnergy;

    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        // Singleton Kontrolü
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // OYUN ÝLK AÇILDIÐINDA Eðer GameManager'da kayýtlý bir veri yoksa, deðerleri fulle.
        // Þimdilik test edebilmen için Awake içinde.
        InitializeStats();
    }
    public void InitializeStats()
    {
        if (playerStats != null)
        {
            currentHealth = playerStats.TotalMaxHealth;
            currentMana = playerStats.TotalMaxMana;
            currentEnergy = playerStats.TotalMaxEnergy;
        }
        else
        {
            Debug.LogError("PlayerDataManager: PlayerStats SO atanmamýþ!");
        }
    }
    public void ResetStatsForNewDay()
    {
        InitializeStats();
    }
}
