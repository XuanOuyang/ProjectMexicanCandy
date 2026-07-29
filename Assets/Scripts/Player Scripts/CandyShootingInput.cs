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
    public LineRenderer lineRenderer;

    [Header("Line Display")]
    public int linePoints = 30;
    public float timeBetweenPoints = 0.05f;

    private PlayerHealth health;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (!CanShoot())
        {
            CancelShooting();
            return;
        }

        Vector3 startPos = firePoint != null ? firePoint.position : transform.position + transform.forward;
        Vector3 launchDirection = (transform.forward + transform.up * arcForce).normalized;

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
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!CanShoot())
        {
            CancelShooting();
            return;
        }

        if (context.started)
        {
            isCharging = true;
            isHoldingShoot = true;
            currentLaunchForce = minLaunchForce;
        }
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

    bool CanShoot()
    {
        if (health == null)
            return true;

        return !health.isDowned && !health.isDead;
    }

    void CancelShooting()
    {
        isCharging = false;
        isHoldingShoot = false;
        currentLaunchForce = minLaunchForce;

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    void Shoot(Vector3 spawnPosition, Vector3 velocity)
    {
        if (!CanShoot())
            return;

        if (projectilePrefab == null)
            return;

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
        if (lineRenderer == null)
            return;

        lineRenderer.enabled = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = linePoints;

        Vector3 startingVelocity = launchDirection * currentLaunchForce;
        Vector3 gravity = Physics.gravity;

        Vector3 previousPoint = startPosition;

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;

            Vector3 pointPosition =
                startPosition +
                startingVelocity * time +
                0.5f * gravity * time * time;

            if (Physics.Linecast(previousPoint, pointPosition, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Wall") ||
                    hit.collider.CompareTag("Floor") ||
                    hit.collider.CompareTag("Enemy"))
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }

            lineRenderer.SetPosition(i, pointPosition);
            previousPoint = pointPosition;
        }
    }

    void OnDisable()
    {
        CancelShooting();
    }
}