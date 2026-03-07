using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Died
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PlayerMovementController movementController;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private PlayerStats playerStats;

    public PlayerMovementController MovementController => movementController;
    public HealthSystem HealthSystem => healthSystem;
    public PlayerStats PlayerStats => playerStats;

    public GameState CurrentState { get; private set; }

    private readonly Dictionary<string, float> sceneLastVisitedTime = new();

    public event Action<GameState> OnGameStateChanged;
    public event Action<string> OnSceneChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeGame();
    }

    private void InitializeGame()
    {
        //SetGameState(GameState.MainMenu); ilk oyun açýldýðýnda ana menu gelecek bu kodu iþleme koyunca
        SetGameState(GameState.Playing);//deneme amaçlý bu dursun. oyun açýlýr açýlmaz playing olup saat iþletiyor
        LoadSavedData(); // þu an boþta
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        // Eventler temizleniyor, bunlarda çok gerekmiyor þu an. ama ilerde lazým olabilir.
        OnGameStateChanged = null;
        OnSceneChanged = null;
    }
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Sahne yüklendi: {scene.name}");

        OnSceneChanged?.Invoke(scene.name);
        SaveSceneTime(scene.name);
    }
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return; // Avoid unnecessary state changes

        CurrentState = newState;
        if (newState == GameState.Playing)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
        OnGameStateChanged?.Invoke(newState);
    }
    public void SaveSceneTime(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // TimeManager'dan güncel global zamaný çekiyoruz
        if (TimeManager.Instance != null)
            sceneLastVisitedTime[sceneName] = TimeManager.Instance.TotalPlayTime;

        SaveGameData();
    }
    public float GetSceneElapsedTime(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !sceneLastVisitedTime.ContainsKey(sceneName))
            return 0f;

        if (TimeManager.Instance == null) return 0f;
        return TimeManager.Instance.TotalPlayTime - sceneLastVisitedTime[sceneName];
    }
    private void OnApplicationQuit()
    {
        SaveGameData();
    }
    private void SaveGameData()
    {
        // biþiler biþiler (Playerstats fln)
    }
    private void LoadSavedData()
    {
        // biþiler biþiler
    }
}