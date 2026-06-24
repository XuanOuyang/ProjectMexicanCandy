using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{   
    [Header("Movement")]
    public float verticalInput;
    public float horizontalInput;
    public float moveSpeed = 5f;
    private Rigidbody rb;

    [Header("Shooting")]
    public GameObject projectilePrefab; // Drop your bullet prefab here in the inspector
    public Transform firePoint;          // Where the bullet spawns (optional, can use player position)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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

            // 2. Read Shooting Input (Spacebar)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Shoot();
            }
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

    void Shoot()
    {
        // Determine where to spawn the projectile (use firePoint if assigned, otherwise player position)
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + transform.forward;
        
        // Spawn the projectile matching the player's current position and rotation
        Instantiate(projectilePrefab, spawnPosition, transform.rotation);
    }
}