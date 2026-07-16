using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public float TimeBetweenPoints = 0.05f; // Simulation step accuracy (lower = smoother line)

    // Physics Simulation Scene Handles
    private Scene predictionScene;
    private PhysicsScene physicsScene;

    void Start()
    {
        // Setup the isolated background physics environment
        CreatePredictionScene();
        animator = GetComponent<Animator>();
    }

    void CreatePredictionScene()
    {
        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        predictionScene = SceneManager.CreateScene("TrajectoryPrediction", parameters);
        physicsScene = predictionScene.GetPhysicsScene();
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
        if (LineRenderer == null || projectilePrefab == null) return;
 
        LineRenderer.enabled = true;
        LineRenderer.useWorldSpace = true;
        LineRenderer.positionCount = LinePoints;
 
        // 1. Instantiate the invisible "ghost" projectile
        GameObject ghostObj = Instantiate(projectilePrefab, startPosition, transform.rotation);
        
        // Hide its renderers immediately so it's invisible to players
        Renderer[] renderers = ghostObj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;

        // Move it into our isolated prediction world
        SceneManager.MoveGameObjectToScene(ghostObj, predictionScene);

        Rigidbody ghostRb = ghostObj.GetComponent<Rigidbody>();
        if (ghostRb != null)
        {
            ghostRb.isKinematic = false;
            ghostRb.WakeUp();
            ghostRb.linearVelocity = launchDirection * currentLaunchForce;
        }

        // Set the starting point
        LineRenderer.SetPosition(0, startPosition);
 
        // 2. Clock physics forward step-by-step and capture coordinates
        for (int i = 1; i < LinePoints; i++)
        {
            physicsScene.Simulate(TimeBetweenPoints);
            
            if (ghostObj != null)
            {
                LineRenderer.SetPosition(i, ghostObj.transform.position);
            }
            else
            {
                // If the ghost collided and was destroyed prematurely inside the simulation scene, 
                // lock remaining trail points to the last known position.
                LineRenderer.SetPosition(i, LineRenderer.GetPosition(i - 1));
            }
        }
 
        // 3. Destroy the ghost object to keep the prediction scene empty
        Destroy(ghostObj);
    }
}