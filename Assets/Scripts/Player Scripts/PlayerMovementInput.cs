using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

public class PlayerMovementInput : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    //For "playerInput"
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, 0f, moveDirection.z * moveSpeed);
        bool isMoving = moveDirection != Vector3.zero;
        animator.SetBool("IsMoving", isMoving);
        if (isMoving)
        {
            if (moveDirection.z > 0f)
            {
                animator.SetBool("IsFacingBack", true);
            }
            else if (moveDirection.z < 0f)
            {
                animator.SetBool("IsFacingBack", false);
            }

            if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
            {
                // Forward/backward movement
                animator.SetFloat("MoveDirection", moveDirection.z > 0f ? 1f : -1f);
            }
            else
            {
                // Sideways movement
                animator.SetFloat("MoveDirection", 0f);
            }
        }

        if (moveDirection.x != 0f)
        {
            spriteRenderer.flipX = moveDirection.x > 0f;
        }

        // Face the movement direction
        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }
}