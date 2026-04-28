using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Hotbar'daki tek bir slotun görsel bileşeni.
///
/// Hierarchy yapısı (prefab):
///   HotbarSlot (GameObject)
///   ├── Image "SlotBackground"      ← arka plan karesi
///   ├── Image "BorderImage"         ← highlight çerçevesi (ayrı Image bileşeni)
///   ├── Image "ItemIcon"            ← item ikonu
///   ├── TextMeshProUGUI "AmountText"← stack miktarı (x1 ise gizle)
///   └── TextMeshProUGUI "KeyLabel"  ← "1","2",…"9","0" tuş etiketi
/// </summary>
public class HotbarSlotUI : MonoBehaviour
{
    // Hotbar her zaman 10 slottur (0=index, tuş=1–9 ve 0)
    public const int HotbarCount = 10;

    // ── Inspector referansları ────────────────────────────────────────────────
    [SerializeField] private Image            borderImage;
    [SerializeField] private Image            itemIcon;
    [SerializeField] private TextMeshProUGUI  amountText;
    [SerializeField] private TextMeshProUGUI  keyLabel;

    // ── Highlight animasyon ayarları ─────────────────────────────────────────
    [Header("Highlight Animasyon")]
    [Tooltip("Border genişlik/yükseklik artışı (pixel). Küçük değer önerilir: 2-4")]
    [SerializeField] private float pulseSize      = 3f;
    [Tooltip("Nabız hızı (saniye başına döngü)")]
    [SerializeField] private float pulseSpeed     = 2.5f;
    [Tooltip("Minimum alpha çarpanı (0–1 arası). 0.7 önerilir.")]
    [SerializeField] private float pulseMinAlpha  = 0.7f;

    // ── Inject ───────────────────────────────────────────────────────────────
    [Inject] private ItemDatabase _itemDatabase;

    // ── Runtime ──────────────────────────────────────────────────────────────
    public bool IsHighlighted { get; private set; }

    private Color      _highlightColor;
    private Color      _defaultColor;
    private RectTransform _borderRect;
    private Vector2    _borderDefaultSize;

    // ── Init ─────────────────────────────────────────────────────────────────

    /// <summary>PlayerUIController tarafından bir kez çağrılır.</summary>
    public void Init(string keyText, Color defaultBorderColor)
    {
        _defaultColor = defaultBorderColor;

        if (keyLabel != null)
            keyLabel.text = keyText;

        if (borderImage != null)
        {
            borderImage.color = _defaultColor;
            _borderRect       = borderImage.rectTransform;
            _borderDefaultSize = _borderRect.sizeDelta;
        }

        // Başlangıçta icon ve miktar gizli
        SetIconVisible(false);
    }

    // ── Görsel güncelleme ─────────────────────────────────────────────────────

    /// <summary>
    /// Slot içeriği değiştiğinde PlayerUIController tarafından çağrılır.
    /// </summary>
    public void Refresh(ItemStack stack)
    {
        if (stack.IsEmpty)
        {
            SetIconVisible(false);
            return;
        }

        ItemData data = _itemDatabase?.Get(stack.ItemID);
        if (data == null)
        {
            SetIconVisible(false);
            return;
        }

        // İkon
        if (itemIcon != null)
        {
            itemIcon.sprite  = data.icon;
            itemIcon.enabled = data.icon != null;
        }

        // Miktar metni: stacklenemeyenler veya 1 adet ise gizle
        if (amountText != null)
        {
            bool showAmount = data.IsStackable && stack.Amount > 1;
            amountText.gameObject.SetActive(showAmount);
            if (showAmount) amountText.text = stack.Amount.ToString();
        }

        SetIconVisible(true);
    }

    // ── Highlight ─────────────────────────────────────────────────────────────

    /// <summary>PlayerUIController.SelectHotbarSlot() tarafından çağrılır.</summary>
    public void SetHighlight(bool active, Color color)
    {
        IsHighlighted   = active;
        _highlightColor = color;

        if (borderImage == null) return;

        if (!active)
        {
            // Sıfırla
            borderImage.color         = _defaultColor;
            _borderRect.sizeDelta     = _borderDefaultSize;
        }
        // Aktifse Update() içindeki nabız animasyonu devreye girer
    }

    // ── Unity Update (nabız animasyonu) ──────────────────────────────────────

    private void Update()
    {
        if (!IsHighlighted || borderImage == null) return;

        // Alpha nabzı: sin dalgası ile pulseMinAlpha–1 arası
        float alpha = Mathf.Lerp(pulseMinAlpha, 1f,
            (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f);

        borderImage.color = new Color(
            _highlightColor.r,
            _highlightColor.g,
            _highlightColor.b,
            alpha);

        // Boyut nabzı: küçük titreşim
        float sizeOffset = Mathf.Lerp(0f, pulseSize,
            (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f);

        _borderRect.sizeDelta = _borderDefaultSize + Vector2.one * sizeOffset;
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────────

    private void SetIconVisible(bool visible)
    {
        if (itemIcon   != null) itemIcon.enabled = visible;
        if (amountText != null) amountText.gameObject.SetActive(false); // Refresh'te açılır
    }
}
