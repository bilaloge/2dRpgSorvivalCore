using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
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

    //evren saati
    [SerializeField] private float _globalGameTime = 0f;
    public float GlobalGameTime => _globalGameTime;
    
    //gün sayacý
    private int _currentDay = 0;
    public int CurrentDay => _currentDay;

    //clock ui için deðiþkenler
    private float _timeUpdateTimer = 0f;
    private const float _timeUpdateInterval = 1f; // global oyun saatinin Güncellenme sýklýðý. bunu 10 fln da yapabilirim

    public float gameTimeScale = 60f; //oyunda geçen zaman ile gerçek hayatta geçen süre. anda ayný gerçek hayatta 1 saniye oyunda 60 sn

    private const float secondsInADay = 24 * 60 * 60; //oyun zamanýnýnda 1 dk 1 saate eþ yapabilirim. þimdilik kalsýn


    private readonly Dictionary<string, float> sceneLastVisitedTime = new();

    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnGlobalTimeUpdated; //Fýrýnlar, bitki büyümesi vb.için
    public event Action<int, int, float> OnDayNightCycleUpdated;

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
        //SetGameState(GameState.MainMenu);
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
    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        _globalGameTime += Time.deltaTime * gameTimeScale;
        _timeUpdateTimer += Time.deltaTime;

        if (_timeUpdateTimer >= _timeUpdateInterval)
        {
            _timeUpdateTimer = 0f;
            OnGlobalTimeUpdated?.Invoke(_globalGameTime);// Fýrýnlar fln dinliycek
            float currentDayTime = _globalGameTime % secondsInADay; // global zamandan 24 saatlik döngü hesaplama zýmbýrtýlarý. internetten kopya
            int hours = (int)(currentDayTime / 3600f);
            int minutes = (int)((currentDayTime % 3600f) / 60f);
            float normalizedDayTime = currentDayTime / secondsInADay;

            OnDayNightCycleUpdated?.Invoke(hours, minutes, normalizedDayTime); //Gece/gündüz postprocessing ve saat UI ý için
        }

        if (_globalGameTime >= (_currentDay + 1) * secondsInADay)//gün sayacý
        {
            _currentDay++;// belki OnNewDayStarted?.Invoke(_currentDay); yazýp yeni güne baþlayýnca gün sayacýný artýrýrým. Muhtemelen öyle yaparým
        }

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
        OnGameStateChanged?.Invoke(newState);
    }

    public void SaveSceneTime(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        sceneLastVisitedTime[sceneName] = _globalGameTime;
        SaveGameData();
    }

    public float GetSceneElapsedTime(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !sceneLastVisitedTime.ContainsKey(sceneName))
            return 0f;

        return _globalGameTime - sceneLastVisitedTime[sceneName];
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