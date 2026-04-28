using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Kilitli Durum")]
    [SerializeField] private Color unlockedColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color lockedColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);

    [Inject] private ItemDatabase _itemDatabase;
    [Inject] private InventoryManager _inventoryManager;

    public int SlotIndex { get; private set; }
    public bool IsLocked { get; private set; }

    private Canvas _rootCanvas;
    private static Image _dragIcon;
    private static int _dragFromIndex = -1;

    public static void SetDragIcon(Image icon) => _dragIcon = icon;

    public void Init(int slotIndex, Canvas rootCanvas, bool locked)
    {
        SlotIndex = slotIndex;
        _rootCanvas = rootCanvas;
        SetLocked(locked);
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;

        if (background != null)
            background.color = locked ? lockedColor : unlockedColor;

        if (itemIcon != null) itemIcon.enabled = !locked;
        if (amountText != null) amountText.enabled = !locked;
    }

    public void Refresh(ItemStack stack)
    {
        if (IsLocked) return;

        if (stack.IsEmpty)
        {
            SetIconVisible(false);
            return;
        }

        ItemData data = _itemDatabase?.Get(stack.ItemID);
        if (data == null) { SetIconVisible(false); return; }

        if (itemIcon != null)
        {
            itemIcon.sprite = data.icon;
            itemIcon.enabled = data.icon != null;
        }

        if (amountText != null)
        {
            bool show = data.IsStackable && stack.Amount > 1;
            amountText.gameObject.SetActive(show);
            if (show) amountText.text = stack.Amount.ToString();
        }
    }

    // Sol týk ile item'ý al veya býrak
    public void OnPointerClick(PointerEventData e)
    {
        if (IsLocked || e.button != PointerEventData.InputButton.Left) return;

        // Sürükleme varsa týklamayý yoksay
        if (e.dragging) return;

        // Taþýnan item varsa bu slota býrak
        if (_dragFromIndex >= 0 && _dragFromIndex != SlotIndex)
        {
            _inventoryManager.SwapSlots(_dragFromIndex, SlotIndex);
            _dragFromIndex = -1;
            if (_dragIcon != null) _dragIcon.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (IsLocked) return;

        ItemStack stack = _inventoryManager.GetSlot(SlotIndex);
        if (stack.IsEmpty) return;

        _dragFromIndex = SlotIndex;

        if (_dragIcon != null)
        {
            ItemData data = _itemDatabase.Get(stack.ItemID);
            _dragIcon.sprite = data?.icon;
            _dragIcon.enabled = true;
            _dragIcon.transform.SetAsLastSibling();
        }

        // Sürüklenen slotun ikonunu soluk göster
        if (itemIcon != null) itemIcon.color = new Color(1, 1, 1, 0.3f);
    }

    public void OnDrag(PointerEventData e)
    {
        if (_dragIcon == null || !_dragIcon.enabled) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.transform as RectTransform,
            e.position,
            _rootCanvas.worldCamera,
            out Vector2 pos);

        _dragIcon.rectTransform.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (itemIcon != null) itemIcon.color = Color.white;

        // Herhangi bir slota býrakýlmadýysa drag iptal
        if (_dragFromIndex == SlotIndex)
        {
            _dragFromIndex = -1;
            if (_dragIcon != null) _dragIcon.enabled = false;
        }
    }

    public void OnDrop(PointerEventData e)
    {
        if (IsLocked) return;

        InventorySlotUI from = e.pointerDrag?.GetComponent<InventorySlotUI>();
        if (from == null || from.SlotIndex == SlotIndex) return;

        _inventoryManager.SwapSlots(from.SlotIndex, SlotIndex);
        _dragFromIndex = -1;

        if (_dragIcon != null) _dragIcon.enabled = false;
        if (from.itemIcon != null) from.itemIcon.color = Color.white;
    }

    private void SetIconVisible(bool visible)
    {
        if (itemIcon != null) itemIcon.enabled = visible;
        if (amountText != null) amountText.gameObject.SetActive(false);
    }
}
