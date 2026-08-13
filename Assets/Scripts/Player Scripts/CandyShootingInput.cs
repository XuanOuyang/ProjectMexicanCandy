using System;
using UnityEngine;
using UnityEngine.InputSystem;

// ─────────────────────────────────────────────────────────────────────────────
// Configure each candy type right inside the Unity Inspector.
// Every stat (arc, speed, power, ammo) is independent per candy.
// ─────────────────────────────────────────────────────────────────────────────
[System.Serializable]
public class CandyType
{
    [Tooltip("Display name shown in debug / UI")]
    public string candyName = "Candy";

    [Tooltip("Prefab that gets spawned when fired")]
    public GameObject projectilePrefab;

    [Tooltip("Upward arc added to the launch direction (higher = more lobbed)")]
    public float arcForce = 0.5f;

    [Tooltip("Force applied when the button is just tapped")]
    public float minLaunchForce = 5f;

    [Tooltip("Force applied when the button is held for the full charge time")]
    public float maxLaunchForce = 20f;

    [Tooltip("Seconds to go from min to max launch force")]
    public float chargeTime = 1.5f;

    [Tooltip("Maximum number of shots before a pickup is required")]
    public int maxAmmo = 5;

    // ── Runtime (not shown in Inspector) ─────────────────────────────────────
    [HideInInspector] public int currentAmmo;
}

// ─────────────────────────────────────────────────────────────────────────────
// Attach this to the player GameObject alongside a PlayerInput component.
//
// Candy switching is subscribed to in code via Awake/OnEnable/OnDisable,
// so it works regardless of which PlayerInput Behavior mode you have set
// (Send Messages, Invoke Unity Events, or C# Events). No Inspector wiring
// required for the Choose Candy actions.
//
// OnShoot still uses the public callback method — keep it wired however
// it was already working for you.
// ─────────────────────────────────────────────────────────────────────────────
public class CandyShootingInput : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Candy Types  (index 0 = slot 1, index 1 = slot 2, index 2 = slot 3)")]
    public CandyType[] candyTypes = new CandyType[3];

    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Trajectory Preview")]
    public LineRenderer lineRenderer;
    public int   linePoints        = 30;
    public float timeBetweenPoints = 0.05f;

    // ── Private State ─────────────────────────────────────────────────────────
    private int   selectedCandyIndex = 0;
    private float currentLaunchForce = 0f;
    private bool  isCharging         = false;
    private bool  isHoldingShoot     = false;
    private Animator animator;
    
    public event Action<int> OnCandySelected;

    // ── Direct input action references (bypasses PlayerInput Behavior mode) ───
    private InputAction _cc1Action, _cc2Action, _cc3Action;

    // Stored delegates so OnDisable can unsubscribe the exact same reference
    private System.Action<InputAction.CallbackContext> _onCC1, _onCC2, _onCC3;

    // ── Convenience ───────────────────────────────────────────────────────────
    private CandyType CurrentCandy => candyTypes[selectedCandyIndex];

    // ── Public Read-only Info (useful for UI scripts) ─────────────────────────
    public int    SelectedIndex    => selectedCandyIndex;
    public int    CurrentAmmo      => CurrentCandy.currentAmmo;
    public int    CurrentMaxAmmo   => CurrentCandy.maxAmmo;
    public string CurrentCandyName => CurrentCandy.candyName;
    public float  ChargePercent    =>
        CurrentCandy.maxLaunchForce > CurrentCandy.minLaunchForce
            ? (currentLaunchForce - CurrentCandy.minLaunchForce) /
              (CurrentCandy.maxLaunchForce - CurrentCandy.minLaunchForce)
            : 0f;

    // ══════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        animator = GetComponent<Animator>();
        PlayerInput pi = GetComponent<PlayerInput>();
        if (pi == null)
        {
            Debug.LogError("[CandyShooter] No PlayerInput component found on this GameObject!");
            return;
        }

        // Look up actions by the exact name from your Input Actions asset.
        // throwIfNotFound: true gives a clear error if the name is wrong.
        _cc1Action = pi.actions.FindAction("Choose Candy 1", throwIfNotFound: true);
        _cc2Action = pi.actions.FindAction("Choose Candy 2", throwIfNotFound: true);
        _cc3Action = pi.actions.FindAction("Choose Candy 3", throwIfNotFound: true);

        // Store delegates so the same reference is used on subscribe AND unsubscribe
        _onCC1 = _ => SelectCandy(0);
        _onCC2 = _ => SelectCandy(1);
        _onCC3 = _ => SelectCandy(2);
    }

    private void OnEnable()
    {
        // Subscribe to the performed phase: fires once when the button is pressed
        if (_cc1Action != null) _cc1Action.performed += _onCC1;
        if (_cc2Action != null) _cc2Action.performed += _onCC2;
        if (_cc3Action != null) _cc3Action.performed += _onCC3;
    }

    private void OnDisable()
    {
        if (_cc1Action != null) _cc1Action.performed -= _onCC1;
        if (_cc2Action != null) _cc2Action.performed -= _onCC2;
        if (_cc3Action != null) _cc3Action.performed -= _onCC3;
    }

    // ══════════════════════════════════════════════════════════════════════════
    private void Start()
    {
        // Give every candy its full starting ammo
        foreach (CandyType candy in candyTypes)
            candy.currentAmmo = candy.maxAmmo;

        currentLaunchForce = CurrentCandy.minLaunchForce;
        OnCandySelected?.Invoke(selectedCandyIndex);
    }

    // ══════════════════════════════════════════════════════════════════════════
    private void Update()
    {
        Vector3 startPos = firePoint != null
                               ? firePoint.position
                               : transform.position + transform.forward;

        Vector3 launchDirection = (transform.forward + transform.up * CurrentCandy.arcForce)
                                  .normalized;

        if (isCharging && isHoldingShoot)
        {
            // Ramp up launch force over chargeTime
            if (CurrentCandy.chargeTime > 0f)
            {
                float ratePerSecond = (CurrentCandy.maxLaunchForce - CurrentCandy.minLaunchForce)
                                      / CurrentCandy.chargeTime;
                currentLaunchForce += ratePerSecond * Time.deltaTime;
            }

            currentLaunchForce = Mathf.Min(currentLaunchForce, CurrentCandy.maxLaunchForce);
            DrawProjection(startPos, launchDirection);
        }
        else if (!isCharging)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // INPUT CALLBACK  –  keep wired however OnShoot was already working
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Handles the "Shoot" action (Space / Enter / Button East).</summary>
    public void OnShoot(InputAction.CallbackContext context)
    {
        // ── Button pressed ────────────────────────────────────────────────────
        if (context.started)
        {
            if (CurrentCandy.currentAmmo <= 0)
            {
                Debug.Log($"[CandyShooter] No {CurrentCandy.candyName} ammo left!");
                return;
            }

            isCharging         = true;
            isHoldingShoot     = true;
            currentLaunchForce = CurrentCandy.minLaunchForce;
            animator.SetTrigger("ThrowStarted");
        }

        // ── Button released ───────────────────────────────────────────────────
        else if (context.canceled)
        {
            isHoldingShoot = false;


            if (isCharging)
            {
                isCharging = false;

                Vector3 startPos = firePoint != null
                                       ? firePoint.position
                                       : transform.position + transform.forward;

                Vector3 launchDirection = (transform.forward + transform.up * CurrentCandy.arcForce)
                                          .normalized;
                animator.SetTrigger("ThrowReleased");
                FireCandy(startPos, launchDirection * currentLaunchForce);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AMMO RESTORATION  –  called by CandyBoxPickup
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Restores ammo for one candy slot, or all slots when candyIndex is -1.
    /// </summary>
    public void RestoreAmmo(int candyIndex, int amount)
    {
        if (candyIndex == -1)
        {
            foreach (CandyType candy in candyTypes)
                candy.currentAmmo = Mathf.Min(candy.currentAmmo + amount, candy.maxAmmo);

            Debug.Log($"[CandyShooter] Restored {amount} ammo to all candies.");
        }
        else if (candyIndex >= 0 && candyIndex < candyTypes.Length)
        {
            candyTypes[candyIndex].currentAmmo =
                Mathf.Min(candyTypes[candyIndex].currentAmmo + amount,
                          candyTypes[candyIndex].maxAmmo);

            Debug.Log($"[CandyShooter] Restored {amount} {candyTypes[candyIndex].candyName} ammo. " +
                      $"({candyTypes[candyIndex].currentAmmo}/{candyTypes[candyIndex].maxAmmo})");
        }
        else
        {
            Debug.LogWarning($"[CandyShooter] RestoreAmmo: invalid candyIndex {candyIndex}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Switches the active candy slot and resets any in-progress charge.</summary>
    private void SelectCandy(int index)
    {
        if (index < 0 || index >= candyTypes.Length) return;

        // Cancel any in-progress charge cleanly before switching
        isCharging     = false;
        isHoldingShoot = false;
        if (lineRenderer != null) lineRenderer.enabled = false;

        selectedCandyIndex = index;
        currentLaunchForce = CurrentCandy.minLaunchForce;
        OnCandySelected?.Invoke(selectedCandyIndex);

        Debug.Log($"[CandyShooter] Switched to slot {index + 1}: " +
                  $"{CurrentCandy.candyName} " +
                  $"({CurrentCandy.currentAmmo}/{CurrentCandy.maxAmmo} ammo)");
    }

    /// <summary>Spawns the projectile and decrements ammo.</summary>
    private void FireCandy(Vector3 spawnPosition, Vector3 velocity)
    {
        // Double-check ammo (could have been drained between press and release)
        if (CurrentCandy.currentAmmo <= 0)
        {
            Debug.Log($"[CandyShooter] Fire canceled – no {CurrentCandy.candyName} ammo.");
            return;
        }

        if (CurrentCandy.projectilePrefab == null)
        {
            Debug.LogWarning($"[CandyShooter] {CurrentCandy.candyName} has no projectile prefab!");
            return;
        }

        CurrentCandy.currentAmmo--;

        GameObject projectile = Instantiate(CurrentCandy.projectilePrefab,
                                            spawnPosition,
                                            transform.rotation);

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.linearVelocity = velocity;
        }
        else
        {
            Debug.LogWarning($"[CandyShooter] '{CurrentCandy.projectilePrefab.name}' " +
                             "is missing a Rigidbody!");
        }

        Debug.Log($"[CandyShooter] Fired {CurrentCandy.candyName}! " +
                  $"Ammo: {CurrentCandy.currentAmmo}/{CurrentCandy.maxAmmo}");
    }

    /// <summary>Draws the predicted arc using kinematic equations.</summary>
    private void DrawProjection(Vector3 startPosition, Vector3 launchDirection)
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled       = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = linePoints;
        lineRenderer.enabled = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = linePoints;

        Vector3 startingVelocity = launchDirection * currentLaunchForce;
        Vector3 gravity = Physics.gravity;

        Vector3 previousPoint = startPosition;

        for (int i = 0; i < linePoints; i++)
        {
            float   t             = i * timeBetweenPoints;
            // s = v₀t + ½gt²
            Vector3 pointPosition = startPosition
                                  + (startingVelocity * t)
                                  + (0.5f * gravity * t * t);

            if (Physics.Linecast(previousPoint, pointPosition, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Wall")  ||
                    hit.collider.CompareTag("Floor") ||
                    hit.collider.CompareTag("Enemy"))
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }

            lineRenderer.SetPosition(i, pointPosition);
            lineRenderer.SetPosition(i, pointPosition);
            previousPoint = pointPosition;
        }
    }

    /*void OnDisable()
    {
        CancelShooting();
    }
    */
}