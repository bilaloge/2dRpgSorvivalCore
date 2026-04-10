using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;
using System;

public class PlayerUIController : MonoBehaviour
{
    [Inject] private GameManager _gameManager;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float healthLerpSpeed = 5f;

    [Header("Mana UI")]
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private float manaLerpSpeed = 5f;

    [Header("Enerji UI")]
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private float energyLerpSpeed = 10f;

    private HealthSystem _activeHealthSystem;

    private Coroutine _healthCorr;
    private Coroutine _manaCorr;
    private Coroutine _energyCorr;

    private void Start()
    {
        if (_gameManager != null)
        {
            _gameManager.OnPlayerRegistered += InitializeUIWithPlayer;
            InitializeUIWithPlayer();
        }
    }
    private void OnDisable()
    {
        if (_gameManager != null)
            _gameManager.OnPlayerRegistered -= InitializeUIWithPlayer;
        UnsubscribeFromHealth();
        // Eventleri dinlerken unutulmamalarý için OnDestroy'da eventleri un-subscribe etmek önemlidir.
    }
    private void InitializeUIWithPlayer()
    {
        UnsubscribeFromHealth();

        _activeHealthSystem = _gameManager.HealthSystem;

        if (_activeHealthSystem != null)
        {
            _activeHealthSystem.OnHealthChanged += UpdateHealthBar;
            _activeHealthSystem.OnManaChanged += UpdateManaBar;
            _activeHealthSystem.OnEnergyChanged += UpdateEnergyBar;

            // Barlarý mevcut deðerlere eþitle
            _activeHealthSystem.NotifyAll();
        }
    }
    private void UnsubscribeFromHealth()
    {
        if (_activeHealthSystem != null)
        {
            _activeHealthSystem.OnHealthChanged -= UpdateHealthBar;
            _activeHealthSystem.OnManaChanged -= UpdateManaBar;
            _activeHealthSystem.OnEnergyChanged -= UpdateEnergyBar;
        }
    }

    private void UpdateHealthBar(int current, int max)
    {
        {
            healthSlider.maxValue = max;
            if (healthText != null) healthText.text = $"{current}/{max}";

            if (_healthCorr != null) StopCoroutine(_healthCorr);
            _healthCorr = StartCoroutine(SmoothUpdate(healthSlider, current, healthLerpSpeed));
        }
    }


    private void UpdateManaBar(int current, int max)
    {
        manaSlider.maxValue = max;
        if (manaText != null) manaText.text = $"{current}/{max}";

        if (_manaCorr != null) StopCoroutine(_manaCorr);
        _manaCorr = StartCoroutine(SmoothUpdate(manaSlider, current, manaLerpSpeed));
    }


    private void UpdateEnergyBar(int current, int max)
    {
        energySlider.maxValue = max;
        if (energyText != null) energyText.text = $"{current}/{max}";

        if (_energyCorr != null) StopCoroutine(_energyCorr);
        _energyCorr = StartCoroutine(SmoothUpdate(energySlider, current, energyLerpSpeed));
    }
    private IEnumerator SmoothUpdate(Slider slider, float targetValue, float speed)
    {
        while (Mathf.Abs(slider.value - targetValue) > 0.05f)
        {
            slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * speed);
            yield return null;
        }
        slider.value = targetValue;
    }
}
