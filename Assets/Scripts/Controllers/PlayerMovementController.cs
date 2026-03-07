using DG.Tweening;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    #region Performance Optimizations (Hash Variables)
    private static readonly int MoveXHash = Animator.StringToHash("moveX");
    private static readonly int MoveYHash = Animator.StringToHash("moveY");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    #endregion

    #region Variables
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Animator animator;
    [SerializeField] public bool _isReadyToMove = true;

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

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (dashAction != null) dashAction.action.Enable();
    }

    private void OnDisable()
    {
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
        rb2D.MovePosition(rb2D.position + playerStats.MoveSpeed * Time.fixedDeltaTime * input);
        //rb2D.linearVelocity = input * _moveSpeed; bu da bi ihtimal. eðer hýza baðlý bir aksiyon istiyorsan!!!
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

        AfterImagePoolScript.Instance.GetFromPool();
        _lastImagePosX = transform.position.x;

        rb2D.DOMove(target, dashDuration).SetEase(Ease.OutSine).OnComplete(() =>
        {
            if (Mathf.Abs(transform.position.x - _lastImagePosX) >= _distanceBetweenImages)
            {
                AfterImagePoolScript.Instance.GetFromPool();
                _lastImagePosX = transform.position.x;
            }

            _isDashing = false;
            _isReadyToMove = true;
            healthSystem.SetInvulnerable(false);
        });
    }
    public void Die()
    {
        if (!_isReadyToMove) return;
        Debug.Log("Oyuncu öldü.");
        _isReadyToMove = false;
        animator.Play("Death");
        //play death animation, game over screen, vs.
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
