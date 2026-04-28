using System;
using UnityEngine;
using Zenject;

public class TimeManager : MonoBehaviour
{
    [Inject] GameDataManager _gameDataManager;
    [Header("Zaman Ayarlarý")]
    [SerializeField] private float gameTimeScale = 60f;
    [SerializeField] private int startHour = 6;  // Uyanýþ
    [SerializeField] private int eveningHour = 18; // Akþam baþlangýcý
    [SerializeField] private int endHour = 2;    // Bayýlma/Günü bitirme

    public float TotalPlayTime { get; private set; }
    public int Hour => (int)(TotalPlayTime / 3600f) % 24;
    public int Minute => (int)((TotalPlayTime % 3600f) / 60f);
    public int CurrentDay => (int)((TotalPlayTime - (startHour * 3600f)) / 86400f) + 1;

    public bool IsGameTimePaused { get; private set; } = false;

    private int _lastMinute = -1;
    private bool _isEveningFired = false;
    private bool _isDayEndFired = false;

    public event Action<int, int> OnTimeChanged;
    public event Action OnNightStarted;// saat 18 de
    public event Action OnDayStarted;//saat 6 da
    public event Action OnDayEnded;// saat 2 de
    private void Awake()
    {
        TotalPlayTime = startHour * 3600f;
    }
    private void Update()
    {
        if (IsGameTimePaused) return;

        TotalPlayTime += Time.deltaTime * gameTimeScale;

        if (Minute != _lastMinute)
        {
            _lastMinute = Minute;
            OnTimeChanged?.Invoke(Hour, Minute);
            CheckGameRules();
        }
    }
    public void PauseGameTime()
    {
        IsGameTimePaused = true;
    }

    public void ResumeGameTime()
    {
        IsGameTimePaused = false;
    }
    private void CheckGameRules()
    {
        // GÜN BAÞLANGICI (06:00)
        if (Hour == startHour && Minute == 0 && _isDayEndFired)
        {
            ResetDayFlags();
            OnDayStarted?.Invoke();
        }

        // AKÞAM BAÞLANGICI (18:00)
        if (Hour == eveningHour && !_isEveningFired)
        {
            _isEveningFired = true;
            OnNightStarted?.Invoke();
        }

        // BAYILMA / GÜN BÝTÝÞÝ (02:00)
        if (Hour == endHour && !_isDayEndFired)
        {
            _isDayEndFired = true;
            OnDayEnded?.Invoke();

            // KRÝTÝK: GameDataManager'ý tetikle (Sýzma senaryosu)
            // Oyuncu yataða gitmediði için false gönderiyoruz.
            if (_gameDataManager != null)
                _gameDataManager.EndDayAndSave(false);

            // Günü otomatik olarak bir sonraki sabaha atlat
            SkipToNextDay();
        }
    }
    // Yeni güne atlama metodu (Yatakta uyunca çaðrýlýr)
    public void SkipToNextDay()
    {
        float secondsInDay = 86400f;
        // Mevcut günün baþlangýcýný bul ve üzerine 1 gün + startHour ekle
        float currentDayStart = (CurrentDay - 1) * secondsInDay;
        float nextDayStart = currentDayStart + secondsInDay + (startHour * 3600f);

        if (TotalPlayTime >= nextDayStart)
            nextDayStart += secondsInDay;

        TotalPlayTime = nextDayStart;

        ResetDayFlags();
        _lastMinute = -1;

        OnDayStarted?.Invoke();

        Debug.Log($"Yeni güne geçildi: Gün {CurrentDay}, Saat {Hour:00}:{Minute:00}");
    }
    private void ResetDayFlags()
    {
        _isEveningFired = false;
        _isDayEndFired = false;
    }
    public float GetActiveDayPercentage()
    {
        float startSec = startHour * 3600f;
        float endSec = (24 + endHour) * 3600f;
        float currentSec = (TotalPlayTime % 86400f);
        // Eðer gece yarýsýndan sonraysak (00:00 - 02:00), bunu 24:00+ olarak hesapla
        if (currentSec < startSec) currentSec += 86400f;

        float percentage = (currentSec - startSec) / (endSec - startSec);
        return Mathf.Clamp01(percentage);
    }
}
