using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

public class PlayerMovementInput : MonoBehaviour
{   
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        
        // Face the movement direction
        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }
}