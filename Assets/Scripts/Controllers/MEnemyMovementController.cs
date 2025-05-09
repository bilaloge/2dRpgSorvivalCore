using UnityEngine;

public class MEnemyMovementController : MonoBehaviour
{
    #region Variables
    public EnemyBase enemyData; // ScriptableObject Inspector'dan atanacak
    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private Vector2 spawnPosition;
    private bool isAggro;
    private float attackCooldown;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnPosition = rb.position;
    }

    private void Update()
    {
        if (player == null || enemyData == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAggro && distanceToPlayer <= enemyData._aggroRange)
            isAggro = true;
        else if (isAggro && distanceToPlayer > enemyData._maxAggroRange)
            isAggro = false;

        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null || enemyData == null) return;

        if (isAggro)
            HandleChase();
        else
            ReturnToSpawn();
    }

    private void HandleChase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * enemyData._speed * Time.deltaTime);
        UpdateAnimator(direction, true);
    }

    private void ReturnToSpawn()
    {
        Vector2 toSpawn = spawnPosition - rb.position;
        if (toSpawn.magnitude > 0.1f)
        {
            Vector2 direction = toSpawn.normalized;
            rb.MovePosition(rb.position + direction * enemyData._speed * Time.deltaTime);
            UpdateAnimator(direction, true);
        }
        else
        {
            UpdateAnimator(Vector2.zero, false);
            rb.MovePosition(spawnPosition);
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
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.CompareTag("Player") && attackCooldown <= 0)
        {
            HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                float damage = enemyData._attackPower;
                healthSystem.TakeDamage(damage); // Zırh/direnç burada dikkate alınabilir
                Debug.Log(damage + " hasar verildi (çarpışma ile)");
                attackCooldown = 1f / enemyData._attackSpeed;
            }
        }
    }
}
