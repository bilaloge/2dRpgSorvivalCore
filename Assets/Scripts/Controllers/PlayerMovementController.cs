using DG.Tweening;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    public static PlayerMovementController Instance { get; private set; }

    #region Performance Optimizations (Hash Variables)
    private static readonly int MoveXHash = Animator.StringToHash("moveX");
    private static readonly int MoveYHash = Animator.StringToHash("moveY");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    #endregion

    #region Variables
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Animator animator;
    [SerializeField] public bool _isReadyToMove = true;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private AfterImagePoolScript afterImagePool;

    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private float _distanceBetweenImages = 0.1f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private HealthSystem healthSystem;
    //[SerializeField] private ParticleSystem dirtParticle;

    private Vector2 input;
    private Vector2 _lastInput;
    private bool _isDashing = false;
    private bool _canDash = true;
    private float _dashTimer = 0f;
    private bool _isMoving = false;
    private float _lastImagePosX;
    public float dashEnergyCost = 10;

    private bool _dashRequested = false;
    private Vector2 _dashDirection;
    #endregion
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (afterImagePool == null)
            afterImagePool = GetComponentInChildren<AfterImagePoolScript>();
    }
    private void OnEnable()
    {
        if (Instance != null && Instance != this) return;

        TargetRegistry.Register(this.transform);
        if (moveAction != null) moveAction.action.Enable();
        if (dashAction != null) dashAction.action.Enable();
    }

    private void OnDisable()
    {
        if (Instance != null && Instance != this) return;

        TargetRegistry.Unregister(this.transform);
        if (moveAction != null) moveAction.action.Disable();
        if (dashAction != null) dashAction.action.Disable();
    }

    private void Update()
    {
        if (moveAction != null)
        {
            input = moveAction.action.ReadValue<Vector2>();
        }

        _isMoving = input != Vector2.zero;

        if (input != _lastInput)
        {
            animator.SetBool(IsMovingHash, _isMoving);

            if (_isMoving)
            {
                animator.SetFloat(MoveXHash, input.x);
                animator.SetFloat(MoveYHash, input.y);
            }
            _lastInput = input;
        }

        if (!_canDash)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0) _canDash = true;
        }

        HandleDashInput();
    }
    private void HandleDashInput()
    {
        if (dashAction != null && dashAction.action.WasPressedThisFrame() && !_isDashing && _canDash && input != Vector2.zero)
        {
            if (PlayerDataManager.Instance.currentEnergy >= (int)dashEnergyCost)
            {
                _dashRequested = true;
                _dashDirection = input.normalized;
            }
            else
            {
                Debug.Log("Yeterli enerji yok.");
            }
        }
    }
    private void FixedUpdate()
    {
        if (_isReadyToMove)
        {
            MovePlayer();
        }

        if (_dashRequested)
        {
            _dashRequested = false;
            Dash(_dashDirection);
        }
    }

    private void MovePlayer()
    {
        //rb2D.MovePosition(rb2D.position + playerStats.MoveSpeed * Time.fixedDeltaTime * input);
        rb2D.linearVelocity = input * playerStats.MoveSpeed;
    }

    private void Dash(Vector2 direction)
    {
        _isDashing = true;
        _canDash = false;
        _isReadyToMove = false;
        _dashTimer = _dashCooldown;

        healthSystem.StartTemporaryInvulnerability(dashDuration + 0.1f);
        healthSystem.ReduceEnergy((int)dashEnergyCost);

        Vector2 start = rb2D.position;
        Vector2 target = start + direction * dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(start, direction, dashDistance, obstacleMask);
        if (hit.collider != null)
        {
            target = hit.point;
        }

        // 1. Dash baþladýðý an ilk gölgeyi oluþtur
        SpawnAfterImage();

        // 2. DOMove ile hareketi baþlat
        rb2D.DOMove(target, dashDuration).SetEase(Ease.OutSine)
            .OnUpdate(() => // BU KISIM EKLENDÝ: Hareket sürerken her karede çalýþýr
            {
                // SADECE X deðil, tüm mesafe farkýný (Vector2) kontrol et
                if (Vector2.Distance(rb2D.position, _lastImagePos) >= _distanceBetweenImages)
                {
                    SpawnAfterImage();
                }
            })
            .OnComplete(() =>
            {
                _isDashing = false;
                _isReadyToMove = true;
                healthSystem.SetInvulnerable(false);
            });
    }

    // Kod tekrarýný önlemek için yardýmcý metod
    private Vector2 _lastImagePos; // float yerine Vector2 yaptýk
    private void SpawnAfterImage()
    {
        if (afterImagePool == null)
        {
            afterImagePool = AfterImagePoolScript.Instance;
            if (afterImagePool == null) return; // Yine de yoksa hata verme, sadece çalýþma
        }
        GameObject shadow = afterImagePool.GetFromPool();
        if (shadow != null)
        {
            AfterImageSprite shadowScript = shadow.GetComponent<AfterImageSprite>();
            if (shadowScript != null)
            {
                shadowScript.SetupAfterImage(transform.position, transform.rotation, sr.sprite);
            }
        }
        _lastImagePos = rb2D.position;
    }
    public void Die()
    {
        if (!_isReadyToMove) return;
        Debug.Log("Oyuncu öldü.");
        _isReadyToMove = false;
        animator.Play("Death");
        //play death animation, game over screen, vs.
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            TargetRegistry.Unregister(this.transform);
        }
    }
    public void ResetLastImagePosition()
    {
        _lastImagePos = rb2D.position;
    }

    //private void OnCollisionEnter(Collision collision) !!!!!güzel ayak altý toz bulutu çýkarma bulursam
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        dirtParticle.Play();
    //        isOnGround = true;
    //        doubleJump = false;
    //    }

}
