using UnityEngine;
using System.Collections;

public class MEnemyMovementController : MonoBehaviour
{
    #region Variables
    public EnemyBase enemyData;
    public LayerMask obstacleLayer; // Duvarlar/Engeller için Layer
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float raycastRadius = 0.25f; // Görüş hattı kalınlığı
    [SerializeField] private float targetMemoryTime = 2.0f; // Gözden kaybolunca kaç sn araması gerektiği
    private float lostTargetTimer;

    private Transform currentTarget;
    private float targetCheckTimer;
    private float targetCheckInterval = 0.25f; // Saniyede 4 kez kontrol

    private bool isKnockback;
    private float knockbackTimer;
    private Vector2 spawnPosition;
    private Vector2 wanderTarget;
    private bool isAggro;
    private bool isWandering;
    private float wanderWaitTimer;

    private float currentHealth;
    private float attackCooldown;
    private bool isDead = false;
    private Color originalColor;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spawnPosition = transform.position;
        originalColor = spriteRenderer.color;

        if (enemyData != null)
        {
            currentHealth = enemyData._maxHealth;
        }
    }
    private void Update()
    {
        if (isDead || enemyData == null) return;

        // Hedef bulma zamanlayıcısı
        targetCheckTimer += Time.deltaTime;
        if (targetCheckTimer >= targetCheckInterval)
        {
            targetCheckTimer = 0f;
            FindBestTarget();
        }
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        } 
    }
    private void FixedUpdate()
    {
        if (isDead || enemyData == null) return;

        if (isKnockback)
        {
            HandleKnockbackState();
            return;
        }
        if (isAggro && currentTarget != null)
        {
            HandleChase();
        }
        else
        {
            HandlePatrol();
        }
    }
    private void FindBestTarget()
    {
        float closestSqrDist = enemyData._aggroRange * enemyData._aggroRange;
        Transform bestTarget = null;

        // TargetRegistry üzerinden tüm potansiyel hedefleri dön
        foreach (var t in TargetRegistry.Targets)
        {
            if (t == null) continue;
            Vector2 toTarget = (Vector2)t.position - (Vector2)transform.position;
            float sqrDist = toTarget.sqrMagnitude;

            if (sqrDist < closestSqrDist)
            {
                // Arada engel (obstacleLayer) var mı bakıyoruz, raycast denedim ama bu sefer karakterin götü başı obstacle a girince takibi bırakıyo
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, raycastRadius, toTarget.normalized, Mathf.Sqrt(sqrDist), obstacleLayer);

                if (hit.collider == null)
                {
                    closestSqrDist = sqrDist;
                    bestTarget = t;
                    lostTargetTimer = targetMemoryTime;
                }
            }
        }
        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isAggro = true;
        }
        else if (isAggro) // Eğer şu an agresifse ama hedefi göremiyorsa
        {
            lostTargetTimer -= targetCheckInterval;
            if (lostTargetTimer <= 0)
            {
                currentTarget = null;
                isAggro = false;
            }
        }
    }
    private void HandleChase()
    {
        isWandering = false;
        Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;

        // linearVelocity yerine MovePosition kullan
        // Bu sayede fizik motoru obstacle collider'larını hesaba katar
        Vector2 newPosition = rb.position + direction * enemyData._speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        UpdateAnimator(direction, true);
    }
    private void HandlePatrol()
    {
        if (!isWandering)
        {
            if (wanderWaitTimer > 0)
            {
                wanderWaitTimer -= Time.deltaTime;
                UpdateAnimator(Vector2.zero, false);
                return;
            }
            // perfomrnas amaçlı. sadece lazım oluğunda yol aranıyor ayrı metotla
            wanderTarget = GetValidWanderTarget();
            isWandering = true;
        }
        Vector2 toTarget = wanderTarget - rb.position;
        if (toTarget.magnitude > 0.1f)
        {
            Vector2 direction = toTarget.normalized;
            Vector2 castOrigin = rb.position + direction * 0.1f;
            Vector2 newPosition = rb.position + direction * (enemyData._speed * 0.5f) * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            UpdateAnimator(direction, true);
        }
        else
        {
            isWandering = false;
            wanderWaitTimer = UnityEngine.Random.Range(2f, 4f);
        }
    }
    private Vector2 GetValidWanderTarget()
    {
        float wanderRadius = 3f;
        for (int i = 0; i < 10; i++)
        {
            Vector2 candidate = spawnPosition + (Vector2)UnityEngine.Random.insideUnitCircle * wanderRadius;

            Collider2D hit = Physics2D.OverlapCircle(candidate, raycastRadius, obstacleLayer);
            if (hit == null)
            {
                return candidate; // Geçerli nokta bulundu, döngüden çık (break mantığı)
            }
        }
        // 10 denemede bulunamazsa olduğu yerde kalsın
        return transform.position;
    }
    private void HandleKnockbackState()
    {
        knockbackTimer -= Time.fixedDeltaTime;
        if (knockbackTimer <= 0)
        {
            isKnockback = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
    public void TakeDamage(float amount, Vector2 force)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth > 0)
        {
            ApplyKnockback(force);
        }

        StopCoroutine(FlashRed());
        StartCoroutine(FlashRed());
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void ApplyKnockback(Vector2 force)
    {
        isKnockback = true;
        knockbackTimer = 0.2f; // Knockback süresi. ne kadar çok o kadar uzaga gider

        // Direnci hesaba katarak hızı ayarla
        rb.linearVelocity = force * enemyData._knockbackMultiplier;
    }
    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }
    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Die");

        // geçici olarak öldükten sonra colliderını kapat. daha sonra toza dönüştürme kullanıcam. yada her biri için ayrı ölme animasyonu
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Debug.Log(gameObject.name + " öldü.");
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Player") && attackCooldown <= 0)
        {
            HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(enemyData._attackPower);
                attackCooldown = 1f / enemyData._attackSpeed;
            }
        }
    }
    private void UpdateAnimator(Vector2 direction, bool isMoving)
    {
        if (anim == null) return;
        anim.SetBool("isMoving", isMoving);
        if (direction != Vector2.zero)
        {
            anim.SetFloat("moveX", direction.x);
            anim.SetFloat("moveY", direction.y);
        }
    }
}
