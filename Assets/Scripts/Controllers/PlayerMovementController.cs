using DG.Tweening;
using System;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    #region Variables
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Animator animator;
    [SerializeField] public bool _isReadyToMove = true;
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private KeyCode dashKey = KeyCode.Mouse0;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private byte _dashCooldown = 1;
    [SerializeField] private float _distanceBetweenImages = 0.1f;
    [SerializeField] private PlayerStats playerStats;
    //[SerializeField] private ParticleSystem dirtParticle;

    private Vector2 input;
    private bool _isDashing = false;
    private bool _canDash = true;
    private bool _isMoving = false;
    private float _lastImagePosX;
    #endregion

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();

        _isMoving = input != Vector2.zero;

        animator.SetBool("isMoving", _isMoving);

        if (_isMoving)
        {
            if (input.x != animator.GetFloat("moveX") || input.y != animator.GetFloat("moveY"))
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
            }
        }

        if (Input.GetKeyDown(dashKey) && !_isDashing && _canDash && input != Vector2.zero)
        {
            Dash(input.normalized);
        }

    }
    private void FixedUpdate()
    {
        if (_isReadyToMove)
        {
            MovePlayer();
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

        Vector2 start = rb2D.position;
        Vector2 target = start + direction * dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(start, direction, dashDistance, obstacleMask);
        if (hit.collider != null)
        {
            target = hit.point;
        }

        AfterImagePoolScript.Instance.GetFromPool();
        _lastImagePosX = transform.position.x;

        rb2D.DOMove(target , dashDuration).SetEase(Ease.OutSine).OnComplete(() =>
        {
            if (Mathf.Abs(transform.position.x - _lastImagePosX) >= _distanceBetweenImages)
            {
                AfterImagePoolScript.Instance.GetFromPool();
                _lastImagePosX = transform.position.x;
            }

            _isDashing = false;
            _isReadyToMove = true;
            Invoke(nameof(ResetDash), _dashCooldown);

        });
    }
    private void ResetDash()
    {
        _canDash = true;
    }

    // Ölüm olayýný tetikler
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
