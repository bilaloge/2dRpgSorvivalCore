using UnityEngine;

public class AfterImageSprite : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 3f;       // silikleþeme hýzý
    [SerializeField] private float maxLifetime = 2f;     // failsafe
    [SerializeField] private float initialAlpha = 0.6f;

    private SpriteRenderer sr;
    private float alpha;
    private float lifetime;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public void SetupAfterImage(Vector3 position, Quaternion rotation, Sprite playerSprite)
    {
        // 1. Koordinatlarý ve yönü ayarla
        transform.position = position;
        transform.rotation = rotation;

        // 2. Player'ýn anlýk görüntüsünü kopyala
        sr.sprite = playerSprite;

        // 3. Görünürlük ve yaþam süresini sýfýrla
        alpha = initialAlpha;
        lifetime = maxLifetime;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }
    private void Update()
    {
        //solma
        alpha -= fadeSpeed * Time.deltaTime;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f || alpha <= 0.1f)
        {
            // Ömrü dolduðunda havuza geri dön
            if (AfterImagePoolScript.Instance != null)
            {
                AfterImagePoolScript.Instance.AddToPool(gameObject);
            }
            else
            {
                // Eðer havuz bulunamazsa (sahne geçiþi aný vb.), objeyi yok et ki birikmesin
                Destroy(gameObject);
            }
        }
    }
}
