using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    // EVENTS
    public event Action<int> OnCandySelected;
    public event Action<int, int> OnAmmoChanged; // Passes: (slotIndex, newAmmoCount)

    // ── Direct input action references ────────────────────────────────────────
    private InputAction _cc1Action, _cc2Action, _cc3Action, _rotateCandyAction;
    private System.Action<InputAction.CallbackContext> _onCC1, _onCC2, _onCC3, _onRotateCandy;

    // ── Convenience ───────────────────────────────────────────────────────────
    private CandyType CurrentCandy => candyTypes[selectedCandyIndex];

    // ── Public Read-only Info ──────────────────────────────────────────────────
    public int    SelectedIndex    => selectedCandyIndex;
    public int    CurrentAmmo      => CurrentCandy.currentAmmo;
    public int    CurrentMaxAmmo   => CurrentCandy.maxAmmo;
    public string CurrentCandyName => CurrentCandy.candyName;
    public float  ChargePercent    =>
        CurrentCandy.maxLaunchForce > CurrentCandy.minLaunchForce
            ? (currentLaunchForce - CurrentCandy.minLaunchForce) /
              (CurrentCandy.maxLaunchForce - CurrentCandy.minLaunchForce)
            : 0f;

    public void InitializeInput(PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            Debug.LogError("[CandyShooter] InitializeInput received null PlayerInput");
            return;
        }

        // Unbind previous callbacks if re-initializing
        UnbindInputs();

        _cc1Action = playerInput.actions.FindAction("Choose Candy 1", throwIfNotFound: true);
        _cc2Action = playerInput.actions.FindAction("Choose Candy 2", throwIfNotFound: true);
        _cc3Action = playerInput.actions.FindAction("Choose Candy 3", throwIfNotFound: true);
        _rotateCandyAction = playerInput.actions.FindAction("Rotate Candy", throwIfNotFound: true);

        _onCC1 = _ => SelectCandy(0);
        _onCC2 = _ => SelectCandy(1);
        _onCC3 = _ => SelectCandy(2);
        _onRotateCandy = _ => RotateCandy();

        BindInputs();
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        PlayerInput pi = GetComponent<PlayerInput>();
        if (pi != null)
        {
            InitializeInput(pi);
        }
    }

    private void OnEnable()
    {
        BindInputs();
    }

    private void OnDisable()
    {
        UnbindInputs();
    }

    private void BindInputs()
    {
        if (_cc1Action != null && _onCC1 != null) _cc1Action.performed += _onCC1;
        if (_cc2Action != null && _onCC2 != null) _cc2Action.performed += _onCC2;
        if (_cc3Action != null && _onCC3 != null) _cc3Action.performed += _onCC3;
        if (_rotateCandyAction != null && _onRotateCandy != null) _rotateCandyAction.performed += _onRotateCandy;
    }

    private void UnbindInputs()
    {
        if (_cc1Action != null && _onCC1 != null) _cc1Action.performed -= _onCC1;
        if (_cc2Action != null && _onCC2 != null) _cc2Action.performed -= _onCC2;
        if (_cc3Action != null && _onCC3 != null) _cc3Action.performed -= _onCC3;
        if (_rotateCandyAction != null && _onRotateCandy != null) _rotateCandyAction.performed -= _onRotateCandy;
    }

    private void Start()
    {
        for (int i = 0; i < candyTypes.Length; i++)
        {
            candyTypes[i].currentAmmo = candyTypes[i].maxAmmo;
        }

        currentLaunchForce = CurrentCandy.minLaunchForce;
        OnCandySelected?.Invoke(selectedCandyIndex);
    }

    private void Update()
    {
        Vector3 startPos = firePoint != null
                               ? firePoint.position
                               : transform.position + transform.forward;

        Vector3 launchDirection = (transform.forward + transform.up * CurrentCandy.arcForce).normalized;

        if (isCharging && isHoldingShoot)
        {
            if (CurrentCandy.chargeTime > 0f)
            {
                float ratePerSecond = (CurrentCandy.maxLaunchForce - CurrentCandy.minLaunchForce) / CurrentCandy.chargeTime;
                currentLaunchForce += ratePerSecond * Time.deltaTime;
            }

            currentLaunchForce = Mathf.Min(currentLaunchForce, CurrentCandy.maxLaunchForce);
            DrawProjection(startPos, launchDirection);
        }
        else if (!isCharging)
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
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
            if (animator != null) animator.SetTrigger("ThrowStarted");
        }
        else if (context.canceled)
        {
            isHoldingShoot = false;

            if (isCharging)
            {
                isCharging = false;

                Vector3 startPos = firePoint != null
                                       ? firePoint.position
                                       : transform.position + transform.forward;

                Vector3 launchDirection = (transform.forward + transform.up * CurrentCandy.arcForce).normalized;
                if (animator != null) animator.SetTrigger("ThrowReleased");
                FireCandy(startPos, launchDirection * currentLaunchForce);
            }
        }
    }

    public void RestoreAmmo(int candyIndex, int amount)
    {
        if (candyIndex == -1)
        {
            for (int i = 0; i < candyTypes.Length; i++)
            {
                candyTypes[i].currentAmmo = Mathf.Min(candyTypes[i].currentAmmo + amount, candyTypes[i].maxAmmo);
                OnAmmoChanged?.Invoke(i, candyTypes[i].currentAmmo);
            }
            Debug.Log($"[CandyShooter] Restored {amount} ammo to all candies.");
        }
        else if (candyIndex >= 0 && candyIndex < candyTypes.Length)
        {
            candyTypes[candyIndex].currentAmmo = Mathf.Min(candyTypes[candyIndex].currentAmmo + amount, candyTypes[candyIndex].maxAmmo);
            OnAmmoChanged?.Invoke(candyIndex, candyTypes[candyIndex].currentAmmo);
            Debug.Log($"[CandyShooter] Restored {amount} {candyTypes[candyIndex].candyName} ammo.");
        }
        else
        {
            Debug.LogWarning($"[CandyShooter] RestoreAmmo: invalid candyIndex {candyIndex}");
        }
    }

    private void RotateCandy()
    {
        if (candyTypes.Length == 0) return;
        int nextIndex = (selectedCandyIndex + 1) % candyTypes.Length;
        SelectCandy(nextIndex);
    }

    private void SelectCandy(int index)
    {
        if (index < 0 || index >= candyTypes.Length) return;

        isCharging     = false;
        isHoldingShoot = false;
        if (lineRenderer != null) lineRenderer.enabled = false;

        selectedCandyIndex = index;
        currentLaunchForce = CurrentCandy.minLaunchForce;
        OnCandySelected?.Invoke(selectedCandyIndex);
    }

    private void FireCandy(Vector3 spawnPosition, Vector3 velocity)
    {
        if (CurrentCandy.currentAmmo <= 0) return;

        if (CurrentCandy.projectilePrefab == null)
        {
            Debug.LogWarning($"[CandyShooter] {CurrentCandy.candyName} has no projectile prefab!");
            return;
        }

        CurrentCandy.currentAmmo--;
        OnAmmoChanged?.Invoke(selectedCandyIndex, CurrentCandy.currentAmmo);

        GameObject projectile = Instantiate(CurrentCandy.projectilePrefab, spawnPosition, transform.rotation);

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.linearVelocity = velocity;
        }
    }

    private void DrawProjection(Vector3 startPosition, Vector3 launchDirection)
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled       = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = linePoints;

        Vector3 startingVelocity = launchDirection * currentLaunchForce;
        Vector3 gravity = Physics.gravity;
        Vector3 previousPoint = startPosition;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeBetweenPoints;
            Vector3 pointPosition = startPosition + (startingVelocity * t) + (0.5f * gravity * t * t);

            if (Physics.Linecast(previousPoint, pointPosition, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Wall")  ||
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
}