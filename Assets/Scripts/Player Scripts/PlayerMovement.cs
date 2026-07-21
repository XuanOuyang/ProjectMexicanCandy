using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{   
    
    private float verticalInput;
    private float horizontalInput;
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Read Movement Input
        if (Keyboard.current != null)
        {
            horizontalInput = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;

            verticalInput = 0f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) verticalInput = -1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) verticalInput = 1f;
        }
    }

    void FixedUpdate() 
    {
        Vector3 moveDirection = (Vector3.forward * verticalInput) + (Vector3.right * horizontalInput);
        moveDirection.Normalize(); 

        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, 0f, moveDirection.z * moveSpeed);
        
        // Face the movement direction so the projectile shoots forward relative to where we look
        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }
}