using UnityEngine;
using System;
using UnityEngine.SceneManagement;

// ÇIKARILDI: MainMenu
public enum GameState { Playing, Paused, Died }
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Session References")]
    public PlayerMovementController MovementController { get; private set; }
    public HealthSystem HealthSystem { get; private set; }
    public PlayerStats PlayerStats { get; private set; }
    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;
    public event Action OnPlayerRegistered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(PlayerMovementController mc, HealthSystem hs, PlayerStats ps)
    {
        MovementController = mc;
        HealthSystem = hs;
        PlayerStats = ps;

        Debug.Log("GameManager: Aktif oyuncu referanslarý baðlandý.");
        OnPlayerRegistered?.Invoke();
    }
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        // Oyun durduysa veya karakter öldüyse zamaný durdur, oynanýyorsa 1 yap
        Time.timeScale = (newState == GameState.Playing) ? 1f : 0f;

        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"Oyun Durumu Deðiþti: {newState}");
    }
}