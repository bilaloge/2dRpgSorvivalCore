using System;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    #region Self Variables
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Animator animator;
    [SerializeField] private bool _isReadyToMove;
    [SerializeField] private float _moveSpeed;

    private Vector2 input;
    #endregion
    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();


        animator.SetFloat("moveX", input.x);
        animator.SetFloat("moveY", input.y);
        animator.SetBool("isMoving", input != Vector2.zero);
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
        rb2D.linearVelocity = input * _moveSpeed;
    }
}
