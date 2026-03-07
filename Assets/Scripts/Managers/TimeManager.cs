using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Zaman Ayarlarý")]
    [SerializeField] private float gameTimeScale = 60f;
    [SerializeField] private int startHour = 6;  // Uyanýþ
    [SerializeField] private int eveningHour = 18; // Akþam baþlangýcý
    [SerializeField] private int endHour = 2;    // Bayýlma/Günü bitirme

    public float TotalPlayTime { get; private set; }
    public int Hour => (int)(TotalPlayTime / 3600f) % 24;
    public int Minute => (int)((TotalPlayTime % 3600f) / 60f);
    public int CurrentDay => (int)(TotalPlayTime / 86400f) + 1;

    private int _lastMinute = -1;
    private bool _isEveningFired = false;
    private bool _isDayEndFired = false;

    public event Action<int, int> OnTimeChanged;
    public event Action OnNightStarted;// saat 18 de
    public event Action OnDayStarted;//saat 6 da
    public event Action OnDayEnded;// saat 2 de
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        TotalPlayTime = startHour * 3600f;
    }
    private void Update()
    {
        TotalPlayTime += Time.deltaTime * gameTimeScale;
        // Sadece dakika deðiþtiðinde kurallarý kontrol et (PERFORMANS ÝÇÝN)
        if (Minute != _lastMinute)
        {
            _lastMinute = Minute;
            OnTimeChanged?.Invoke(Hour, Minute);
            CheckGameRules(); // Senin sildiðim kurallarýn burada!
        }
    }
    private void CheckGameRules()
    {
        if (Hour == startHour && Minute == 0)
        {
            _isEveningFired = false; // Yeni gün için reset
            _isDayEndFired = false;
            OnDayStarted?.Invoke();
            Debug.Log("Gün Baþladý! Günaydýn.");
        }
        if (Hour == eveningHour && !_isEveningFired)
        {
            _isEveningFired = true;
            OnNightStarted?.Invoke();
            Debug.Log("Akþam oldu, ýþýklar deðiþsin.");
        }
        if (Hour == endHour && !_isDayEndFired)
        {
            _isDayEndFired = true;
            OnDayEnded?.Invoke();
            Debug.Log("Saat 02:00! Oyuncu bayýldý veya gün bitti.");
        }
    }
    // Yeni güne atlama metodu (Yatakta uyunca çaðrýlýr)
    public void SkipToNextDay()
    {
        // Toplam süreyi bir sonraki günün startHour'ýna yuvarla
        float secondsInDay = 86400f;
        float nextDayStart = (CurrentDay) * secondsInDay + (startHour * 3600f);
        TotalPlayTime = nextDayStart;

        _isEveningFired = false;
        _isDayEndFired = false;

        OnDayStarted?.Invoke();
    }
}
