using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Zenject;

public class InventoryPanelUI : MonoBehaviour
{
    [Inject] private InventoryManager _inventoryManager;
    [Inject] private EquipmentManager _equipmentManager;
    [Inject] private TimeManager _timeManager;

    [Header("Panel Kökü")]
    [SerializeField] private GameObject panelRoot;

    [Header("Inventory Slotları (sahnede 50 adet sabit)")]
    [SerializeField] private InventorySlotUI[] inventorySlots;

    [Header("Drag İkonu")]
    [SerializeField] private Image dragIcon;

    [Header("Canvas Referansı")]
    [SerializeField] private Canvas rootCanvas;

    [Header("Equipment Slotları")]
    [SerializeField] private EquipmentSlotUI slotMainHand;
    [SerializeField] private EquipmentSlotUI slotOffHand;
    [SerializeField] private EquipmentSlotUI slotBodyArmor;
    [SerializeField] private EquipmentSlotUI slotRing1;
    [SerializeField] private EquipmentSlotUI slotRing2;
    [SerializeField] private EquipmentSlotUI slotPet;

    [Header("Tab Butonları")]
    [SerializeField] private Button tabInventory;
    [SerializeField] private Button tabCraft;
    [SerializeField] private Button tabSkills;
    [SerializeField] private Button tabSocials;
    [SerializeField] private Button tabSettings;

    [Header("Tab Panelleri")]
    [SerializeField] private GameObject panelInventory; // varsayılan açık
    [SerializeField] private GameObject panelCraft;
    [SerializeField] private GameObject panelSkills;
    [SerializeField] private GameObject panelSocials;
    [SerializeField] private GameObject panelSettings;

    [Header("Input")]
    [SerializeField] private InputActionReference inventoryToggleAction;

    private bool _isOpen = false;

    private void Awake()
    {
        InventorySlotUI.SetDragIcon(dragIcon);
        if (dragIcon != null) dragIcon.enabled = false;
        panelRoot.SetActive(false);
    }

    private void Start()
    {
        InitSlots();
        SetupTabButtons();
        SubscribeEvents();
    }

    private void OnEnable()
    {
        if (inventoryToggleAction != null)
        {
            inventoryToggleAction.action.Enable();
            inventoryToggleAction.action.performed += OnInventoryToggle;
        }
    }

    private void OnDisable()
    {
        if (inventoryToggleAction != null)
        {
            inventoryToggleAction.action.performed -= OnInventoryToggle;
            inventoryToggleAction.action.Disable();
        }

        if (_inventoryManager != null)
        {
            _inventoryManager.OnSlotChanged -= OnSlotChanged;
            _inventoryManager.OnCapacityChanged -= OnCapacityChanged;
        }

        if (_equipmentManager != null)
            _equipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
    }

    private void OnInventoryToggle(InputAction.CallbackContext ctx) => Toggle();

    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        _isOpen = true;
        panelRoot.SetActive(true);
        _timeManager?.PauseGameTime();

        // Varsayılan olarak inventory tab'ını aç
        ShowTab(panelInventory);
        RefreshAll();
    }

    public void Close()
    {
        _isOpen = false;
        panelRoot.SetActive(false);
        _timeManager?.ResumeGameTime();
        if (dragIcon != null) dragIcon.enabled = false;
    }

    private void InitSlots()
    {
        if (inventorySlots == null || inventorySlots.Length != InventoryManager.TotalSlots)
        {
            Debug.LogWarning("[InventoryPanelUI] inventorySlots dizisi tam 50 eleman içermeli.");
            return;
        }

        int activeCapacity = _inventoryManager.ActiveCapacity;
        for (int i = 0; i < InventoryManager.TotalSlots; i++)
            inventorySlots[i].Init(i, rootCanvas, i >= activeCapacity);
    }

    private void RefreshAll()
    {
        for (int i = 0; i < InventoryManager.TotalSlots; i++)
            inventorySlots[i].Refresh(_inventoryManager.GetSlot(i));

        RefreshEquipmentSlots();
    }

    private void RefreshEquipmentSlots()
    {
        slotMainHand?.Refresh(_equipmentManager.GetSlotContent(EquipmentSlotType.Boots));
        slotOffHand?.Refresh(_equipmentManager.GetSlotContent(EquipmentSlotType.Head));
        slotBodyArmor?.Refresh(_equipmentManager.GetSlotContent(EquipmentSlotType.BodyArmor));
        slotRing1?.Refresh(_equipmentManager.GetSlotContent(EquipmentSlotType.Ring1));
        slotRing2?.Refresh(_equipmentManager.GetSlotContent(EquipmentSlotType.Ring2));
        slotPet?.Refresh(_equipmentManager.GetSlotContent(EquipmentSlotType.Pet));
    }

    private void OnSlotChanged(int index)
    {
        if (!_isOpen || index >= inventorySlots.Length) return;
        inventorySlots[index].Refresh(_inventoryManager.GetSlot(index));
    }

    private void OnCapacityChanged(int newCapacity)
    {
        for (int i = 0; i < InventoryManager.TotalSlots; i++)
            inventorySlots[i].SetLocked(i >= newCapacity);
    }

    private void OnEquipmentChanged(EquipmentSlotType slot, string itemID)
    {
        if (!_isOpen) return;
        RefreshEquipmentSlots();
    }

    private void SubscribeEvents()
    {
        _inventoryManager.OnSlotChanged += OnSlotChanged;
        _inventoryManager.OnCapacityChanged += OnCapacityChanged;
        _equipmentManager.OnEquipmentChanged += OnEquipmentChanged;
    }

    private void SetupTabButtons()
    {
        tabInventory?.onClick.AddListener(() => ShowTab(panelInventory));
        tabCraft?.onClick.AddListener(() => ShowTab(panelCraft));
        tabSkills?.onClick.AddListener(() => ShowTab(panelSkills));
        tabSocials?.onClick.AddListener(() => ShowTab(panelSocials));
        tabSettings?.onClick.AddListener(() => ShowTab(panelSettings));
    }

    // Seçilen paneli göster, diğerlerini kapat
    // Background ve TabBar bu metoddan etkilenmez, her zaman görünür kalır
    private void ShowTab(GameObject activePanel)
    {
        panelInventory?.SetActive(panelInventory == activePanel);
        panelCraft?.SetActive(panelCraft == activePanel);
        panelSkills?.SetActive(panelSkills == activePanel);
        panelSocials?.SetActive(panelSocials == activePanel);
        panelSettings?.SetActive(panelSettings == activePanel);
    }
}
