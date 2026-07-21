using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CandyShooting : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject projectilePrefab; // Drop your projectile prefab here
    public Transform firePoint;          // Where the bullet spawns
    public float arcForce = 0.5f;
    public float minLaunchForce = 5f;
    public float maxLaunchForce = 20f;
    private Animator animator;
 
    [Tooltip("How many seconds it takes to reach maximum launch force")]
    public float chargeTime = 1.5f;        
 
    public float currentLaunchForce;
    private bool isCharging = false;
 
    [Header("Line Trajectory")]
    public LineRenderer LineRenderer;
 
    [Header("Line Display")]
    public int LinePoints = 30;           // Number of points tracked along the arc
    public float TimeBetweenPoints = 0.05f; // Time interval between points

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        // Safe check: Use firePoint position if assigned, otherwise forward of player
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position + transform.forward;
 
        // Force the direction forward and up based on the arc modifier
        Vector3 launchDirection = (transform.forward + transform.up * arcForce).normalized;
 
        // Key Just Pressed
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isCharging = true;
            currentLaunchForce = minLaunchForce;
        }
 
        // Key Being Held
        if (isCharging && Keyboard.current.spaceKey.isPressed)
        {
            if (chargeTime > 0)
            {
                currentLaunchForce += ((maxLaunchForce - minLaunchForce) / chargeTime) * Time.deltaTime;
            }
            currentLaunchForce = Mathf.Min(currentLaunchForce, maxLaunchForce);
 
            DrawProjection(startPos, launchDirection);
        }
        else if (!isCharging)
        {
            // Make sure the line disappears when not aiming/charging
            if (LineRenderer != null) LineRenderer.enabled = false;
        }
 
        // Key Released -> Fire!
        if (Keyboard.current.spaceKey.wasReleasedThisFrame && isCharging)
        {
            isCharging = false;
            Shoot(startPos, launchDirection * currentLaunchForce);
        }
    }
 
    void Shoot(Vector3 spawnPosition, Vector3 velocity)
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, transform.rotation);
        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        
        if (projRb != null)
        {
            projRb.linearVelocity = velocity;
        }
        else
        {
            Debug.LogWarning("Projectile Prefab is missing a Rigidbody component!");
        }
    }
 
    void DrawProjection(Vector3 startPosition, Vector3 launchDirection)
    {
        if (LineRenderer == null) return;
 
        LineRenderer.enabled = true;
        LineRenderer.useWorldSpace = true;
        LineRenderer.positionCount = LinePoints;
 
        Vector3 startingVelocity = launchDirection * currentLaunchForce;
        Vector3 gravity = Physics.gravity;

        for (int i = 0; i < LinePoints; i++)
        {
            // Calculate time elapsed at this specific point index
            float time = i * TimeBetweenPoints;
            
            // Mathematical formula for projectile motion: displacement = (v0 * t) + (0.5 * g * t^2)
            Vector3 pointPosition = startPosition + (startingVelocity * time) + (0.5f * gravity * time * time);
            
            LineRenderer.SetPosition(i, pointPosition);
        }
    }
}