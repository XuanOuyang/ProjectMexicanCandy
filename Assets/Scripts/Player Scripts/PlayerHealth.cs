using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    // ... Keeping all your existing variables ...
    public int maxHearts = 5;
    public int currentHearts;
    public bool isDowned = false;
    public bool isDead = false;
    private bool hasBeenRevived = false;

    public float downedTime = 10f;
    [HideInInspector] public float downedTimer;

    public float invincibilityDuration = 2f;
    private float invincibilityTimer;
    private bool isInvincible = false;

    public Image[] hearts;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("Visuals")]
    public Renderer playerRenderer;
    public Color downedColor = Color.red;
    private Color originalColor;

    [Header("Movement")]
    public MonoBehaviour movementScript;
    public Rigidbody rb;

    [Header("Collision")]
    public Collider playerCollider; // This is your main Capsule Collider

    [Header("Combat")]
    public MonoBehaviour attackScript;

    [Header("New Downed UI")]
    public TextMeshProUGUI statusText; 
    [HideInInspector] public bool isBeingRevived = false;

    [Header("In-Game Radius Visualizer")]
    public LineRenderer circleRenderer;
    public float reviveRadius = 3f;
    public int circleSegments = 40;

[Header("Audio")]
public AudioSource hitSound;

[Header("Backwards Hit")]
public float height = 2f;
public float backDistance = 5f;

[Header("VFX")]
public ImpactFX impact;

[Header("New Trigger Settings")]
[Tooltip("Drag the large invisible Sphere Collider component here.")]
public SphereCollider reviveSphereTrigger; // ADD THIS VARIABLE

[Tooltip("Set this to your Ground/Environment layer so enemies don't distort the circle.")]
public LayerMask floorLayer;

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();

        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
            
        if (statusText != null)
            statusText.text = "";

        if (circleRenderer != null)
            circleRenderer.enabled = false;

        // DISABLE the large trigger sphere when the game starts
        if (reviveSphereTrigger != null)
            reviveSphereTrigger.enabled = false; 
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f) isInvincible = false;
        }

        if (isDowned && !isDead)
        {
            downedTimer -= Time.deltaTime;
            if (downedTimer <= 0f)
            {
                Die();
                return;
            }

            if (statusText != null && !isBeingRevived)
            {
                statusText.text = "Downed: " + Mathf.CeilToInt(downedTimer) + "s";
            }

            if (circleRenderer != null && circleRenderer.enabled)
            {
                DrawCircleInGame();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || isDowned || isInvincible) return;

        currentHearts -= amount;
        if (currentHearts < 0) currentHearts = 0;

        hitSound.Play();
        impact.PlayVFX();
        UpdateHeartsUI();
        HitByPinata();

        if (currentHearts <= 0)
        {
            if (hasBeenRevived) Die();
            else Downed();
        }
        else
        {
            TriggerInvincibility();
        }
    }

    void TriggerInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = (i < currentHearts) ? fullHeartSprite : emptyHeartSprite;
            hearts[i].enabled = true;
        }
    }

    void Downed()
    {
        isDowned = true;
        downedTimer = downedTime;

        if (movementScript != null) movementScript.enabled = false;
        if (attackScript != null) attackScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // ENABLE the revive zone ONLY when player falls down
        if (reviveSphereTrigger != null)
            reviveSphereTrigger.enabled = true; 

        if (playerCollider != null) playerCollider.isTrigger = true;
        if (playerRenderer != null) playerRenderer.material.color = downedColor;

        if (circleRenderer != null)
        {
            circleRenderer.positionCount = circleSegments;
            circleRenderer.enabled = true;
            DrawCircleInGame();
        }
    }

    void DrawCircleInGame()
    {
        // NEW LOGIC: Automatically grab the exact radius being used by the physics trigger engine
        float actualPhysicsRadius = reviveRadius;
        if (reviveSphereTrigger != null)
        {
            // This multiplies the collider radius by the player's world scale to get the true radius size
            actualPhysicsRadius = reviveSphereTrigger.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        }

        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angleRad = Mathf.Deg2Rad * (i * angleStep);

            // FIXED LINES: Swapped 'reviveRadius' variable for 'actualPhysicsRadius' 
            float x = transform.position.x + Mathf.Cos(angleRad) * actualPhysicsRadius;
            float z = transform.position.z + Mathf.Sin(angleRad) * actualPhysicsRadius;

            float rayStartHeight = transform.position.y + 1f;
            Vector3 rayOrigin = new Vector3(x, rayStartHeight, z);
            float finalY = transform.position.y;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 5f, floorLayer))
            {
                if (hit.point.y > transform.position.y + 0.5f)
                {
                    finalY = transform.position.y + 0.02f;
                }
                else
                {
                    finalY = hit.point.y + 0.02f;
                }
            }
            else
            {
                finalY = transform.position.y + 0.02f;
            }

            circleRenderer.SetPosition(i, new Vector3(x, finalY, z));
        }
    }
    public void Revive()
    {
        if (!isDowned || isDead) return;

        isDowned = false;
        hasBeenRevived = true;
        currentHearts = maxHearts;
        UpdateHeartsUI();

        if (statusText != null) statusText.text = "";

        if (circleRenderer != null)
            circleRenderer.enabled = false;

        // DISABLE the revive zone sphere when they stand back up
        if (reviveSphereTrigger != null)
            reviveSphereTrigger.enabled = false; 

        if (movementScript != null) movementScript.enabled = true;
        if (attackScript != null) attackScript.enabled = true;
        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (playerCollider != null) playerCollider.isTrigger = false;
        if (playerRenderer != null) playerRenderer.material.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        isDowned = false;
        if (statusText != null) statusText.text = "";
        
        if (circleRenderer != null)
            circleRenderer.enabled = false;

        // DISABLE the revive zone if they bleed out completely
        if (reviveSphereTrigger != null)
            reviveSphereTrigger.enabled = false; 
            
        gameObject.SetActive(false);
    }

    void HitByPinata()
    {
            Vector2 targetPos = (Vector2)transform.position + Vector2.left * backDistance;
            SendBackwards(targetPos, height);
    }

    /// Launches the character toward a target position in an arc.

    public void SendBackwards(Vector2 targetPos, float arcHeight)
    {
        // Calculate displacement
        Vector2 displacement = targetPos - (Vector2)transform.position;

        // Split into horizontal and vertical distances
        float displacementY = displacement.y;
        Vector2 displacementXZ = new Vector2(displacement.x, 0);

        // Calculate initial vertical velocity
        float velocityY = Mathf.Sqrt(2 * arcHeight);

        // Time to go from peak to target
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(0, arcHeight - displacementY));

        float totalTime = velocityY + timeDown;

        // Calculate horizontal velocity
        Vector2 velocityXZ = displacementXZ / totalTime;

        // Combine velocities
        Vector2 launchVelocity = velocityXZ + Vector2.up * velocityY;

        // Apply velocity
        rb.linearVelocity = launchVelocity;
    }

}