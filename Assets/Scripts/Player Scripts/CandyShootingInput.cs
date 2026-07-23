using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CandyShootingInput : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float arcForce = 0.5f;
    public float minLaunchForce = 5f;
    public float maxLaunchForce = 20f;
 
    [Tooltip("How many seconds it takes to reach maximum launch force")]
    public float chargeTime = 1.5f;        
 
    public float currentLaunchForce;
    private bool isCharging = false;
    private bool isHoldingShoot = false;
 
    [Header("Line Trajectory")]
    public LineRenderer LineRenderer;
 
    [Header("Line Display")]
    public int LinePoints = 30;
    public float TimeBetweenPoints = 0.05f;

    void Update()
    {
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position + transform.forward;
        Vector3 launchDirection = (transform.forward + transform.up * arcForce).normalized;
 
        // Key Being Held
        if (isCharging && isHoldingShoot)
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
            if (LineRenderer != null) LineRenderer.enabled = false;
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

        public void OnShoot(InputAction.CallbackContext context)
    {
        // Button just pressed
        if (context.started)
        {
            isCharging = true;
            isHoldingShoot = true;
            currentLaunchForce = minLaunchForce;
        }
        // Button just released
        else if (context.canceled)
        {
            isHoldingShoot = false;
            
            if (isCharging)
            {
                isCharging = false;
                Vector3 startPos = firePoint != null ? firePoint.position : transform.position + transform.forward;
                Vector3 launchDirection = (transform.forward + transform.up * arcForce).normalized;
                Shoot(startPos, launchDirection * currentLaunchForce);
            }
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

        Vector3 previousPoint = startPosition;

        for (int i = 0; i < LinePoints; i++)
        {
            // Calculate time elapsed at this specific point index
            float time = i * TimeBetweenPoints;

            // Mathematical formula for projectile motion: displacement = (v0 * t) + (0.5 * g * t^2)
            Vector3 pointPosition = startPosition + (startingVelocity * time) + (0.5f * gravity * time * time);

            if (Physics.Linecast(previousPoint, pointPosition, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Wall") ||
                    hit.collider.CompareTag("Floor") ||
                    hit.collider.CompareTag("Enemy"))
                {
                    LineRenderer.positionCount = i + 1;
                    LineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }

            LineRenderer.SetPosition(i, pointPosition);
            previousPoint = pointPosition;
        }
    }
}