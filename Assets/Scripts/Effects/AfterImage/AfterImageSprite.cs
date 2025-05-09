using UnityEngine;

public class AfterImageSprite : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 3f;       // silikleþeme hýzý
    [SerializeField] private float maxLifetime = 2f;     // failsafe
    [SerializeField] private float initialAlpha = 0.6f;

    private SpriteRenderer afterSR;
    private Transform player;
    private SpriteRenderer playerSR;

    private Color color;
    private float alpha;
    private float timeActivated;

    private void OnEnable()
    {

        if (afterSR == null)
            afterSR = GetComponent<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerSR == null)
            playerSR = player.GetComponent<SpriteRenderer>();

        transform.position = player.position;
        transform.rotation = player.rotation;
        afterSR.sprite = playerSR.sprite;
        afterSR.flipX = playerSR.flipX;

        alpha = initialAlpha;
        color = new Color(1f, 1f, 1f, alpha);
        afterSR.color = color;

        timeActivated = Time.time;

    }
    private void Update()
    {
        alpha -= fadeSpeed * Time.deltaTime;
        color.a = alpha;
        afterSR.color = color;

        if (alpha <= 0.01f || Time.time >= timeActivated + maxLifetime)
        {
            AfterImagePoolScript.Instance.AddToPool(gameObject);
        }
    }
}
