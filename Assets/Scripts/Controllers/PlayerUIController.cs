using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using Zenject;

public class PlayerUIController : MonoBehaviour
{
    [Inject] private GameManager _gameManager;
    [Inject] private InventoryManager _inventoryManager;

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

    [Header("Hotbar UI")]
    [SerializeField] private HotbarSlotUI[] hotbarSlots;
    [SerializeField] private Color selectedHighlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color defaultBorderColor = new Color(1f, 1f, 1f, 0.25f);

    [Header("Input Actions")]
    [SerializeField] private InputActionReference hotbarSlot1;
    [SerializeField] private InputActionReference hotbarSlot2;
    [SerializeField] private InputActionReference hotbarSlot3;
    [SerializeField] private InputActionReference hotbarSlot4;
    [SerializeField] private InputActionReference hotbarSlot5;
    [SerializeField] private InputActionReference hotbarSlot6;
    [SerializeField] private InputActionReference hotbarSlot7;
    [SerializeField] private InputActionReference hotbarSlot8;
    [SerializeField] private InputActionReference hotbarSlot9;
    [SerializeField] private InputActionReference hotbarSlot0;
    [SerializeField] private InputActionReference hotbarScroll;

    private HealthSystem _activeHealthSystem;
    private Coroutine _healthCorr;
    private Coroutine _manaCorr;
    private Coroutine _energyCorr;
    private int _selectedHotbarIndex = 0;
    private InputActionReference[] _hotbarSlotActions;

    private void Awake()
    {
        _hotbarSlotActions = new InputActionReference[]
        {
            hotbarSlot1, hotbarSlot2, hotbarSlot3, hotbarSlot4, hotbarSlot5,
            hotbarSlot6, hotbarSlot7, hotbarSlot8, hotbarSlot9, hotbarSlot0
        };
    }

    private void Start()
    {
        if (_gameManager != null)
        {
            _gameManager.OnPlayerRegistered += InitializeUIWithPlayer;
            InitializeUIWithPlayer();
        }

        if (_inventoryManager != null)
            _inventoryManager.OnSlotChanged += OnInventorySlotChanged;

        InitHotbarVisuals();
    }

    private void OnEnable()
    {
        for (int i = 0; i < _hotbarSlotActions?.Length; i++)
            _hotbarSlotActions[i]?.action.Enable();

        hotbarScroll?.action.Enable();
    }

    private void OnDisable()
    {
        if (_gameManager != null)
            _gameManager.OnPlayerRegistered -= InitializeUIWithPlayer;

        if (_inventoryManager != null)
            _inventoryManager.OnSlotChanged -= OnInventorySlotChanged;

        UnsubscribeFromHealth();

        for (int i = 0; i < _hotbarSlotActions?.Length; i++)
            _hotbarSlotActions[i]?.action.Disable();

        hotbarScroll?.action.Disable();
    }

    private void Update()
    {
        HandleHotbarKeyInput();
        HandleHotbarScrollInput();
    }

    // Health / Mana / Energy

    private void InitializeUIWithPlayer()
    {
        UnsubscribeFromHealth();
        _activeHealthSystem = _gameManager.HealthSystem;

        if (_activeHealthSystem != null)
        {
            _activeHealthSystem.OnHealthChanged += UpdateHealthBar;
            _activeHealthSystem.OnManaChanged += UpdateManaBar;
            _activeHealthSystem.OnEnergyChanged += UpdateEnergyBar;
            _activeHealthSystem.NotifyAll();
        }
    }

    private void UnsubscribeFromHealth()
    {
        if (_activeHealthSystem == null) return;
        _activeHealthSystem.OnHealthChanged -= UpdateHealthBar;
        _activeHealthSystem.OnManaChanged -= UpdateManaBar;
        _activeHealthSystem.OnEnergyChanged -= UpdateEnergyBar;
    }

    private void UpdateHealthBar(int current, int max)
    {
        healthSlider.maxValue = max;
        if (healthText != null) healthText.text = $"{current}/{max}";
        if (_healthCorr != null) StopCoroutine(_healthCorr);
        _healthCorr = StartCoroutine(SmoothUpdate(healthSlider, current, healthLerpSpeed));
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

    // Hotbar Input

    private void HandleHotbarKeyInput()
    {
        for (int i = 0; i < _hotbarSlotActions.Length; i++)
        {
            if (_hotbarSlotActions[i] != null &&
                _hotbarSlotActions[i].action.WasPressedThisFrame())
            {
                SelectHotbarSlot(i);
                return;
            }
        }
    }

    private void HandleHotbarScrollInput()
    {
        if (hotbarScroll == null) return;

        float scroll = hotbarScroll.action.ReadValue<float>();
        if (Mathf.Approximately(scroll, 0f)) return;

        int direction = scroll < 0f ? 1 : -1;
        int newIndex = (_selectedHotbarIndex + direction + HotbarSlotUI.HotbarCount)
                        % HotbarSlotUI.HotbarCount;
        SelectHotbarSlot(newIndex);
    }

    // Hotbar Seçim ve Güncelleme

    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= HotbarSlotUI.HotbarCount) return;

        hotbarSlots[_selectedHotbarIndex].SetHighlight(false, defaultBorderColor);
        _selectedHotbarIndex = index;
        hotbarSlots[_selectedHotbarIndex].SetHighlight(true, selectedHighlightColor);
    }

    public int SelectedHotbarInventoryIndex => _selectedHotbarIndex;

    public ItemStack GetSelectedHotbarItem()
        => _inventoryManager?.GetSlot(_selectedHotbarIndex) ?? ItemStack.Empty;

    // Hotbar Görsel

    private void InitHotbarVisuals()
    {
        if (hotbarSlots == null || hotbarSlots.Length != HotbarSlotUI.HotbarCount)
        {
            Debug.LogWarning("[PlayerUIController] hotbarSlots dizisi tam 10 eleman içermeli.");
            return;
        }

        for (int i = 0; i < HotbarSlotUI.HotbarCount; i++)
        {
            // index 0 → "1", index 8 → "9", index 9 → "0"
            string keyLabel = i < 9 ? (i + 1).ToString() : "0";
            hotbarSlots[i].Init(keyLabel, defaultBorderColor);
            RefreshHotbarSlot(i);
        }

        SelectHotbarSlot(0);
    }

    private void OnInventorySlotChanged(int slotIndex)
    {
        if (slotIndex < HotbarSlotUI.HotbarCount)
            RefreshHotbarSlot(slotIndex);
    }

    private void RefreshHotbarSlot(int index)
    {
        if (_inventoryManager == null) return;
        hotbarSlots[index].Refresh(_inventoryManager.GetSlot(index));
    }
}