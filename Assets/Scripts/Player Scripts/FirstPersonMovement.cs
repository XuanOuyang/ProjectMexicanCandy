using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class FirstPersonMovement : MonoBehaviour 
{
    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    public float groundDrag;
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatisGround;
    bool grounded;

    [Header("Shooting")]
    public GameObject projectilePrefab; // Drop your bullet prefab here in the inspector
    public Transform firePoint;          // Where the bullet spawns (optional, can use player position)
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    private void Update()
    {
        // Check if grounded
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatisGround);

        MyInput();
        SpeedControl();
        // 2. Read Shooting Input (Spacebar)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           Shoot();
        }
        //handle drag
        if(grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    public void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
    void Shoot()
    {
        // Determine where to spawn the projectile (use firePoint if assigned, otherwise player position)
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + transform.forward;

        // Spawn the projectile matching the player's current position and rotation
        Instantiate(projectilePrefab, spawnPosition, transform.rotation);
    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}
