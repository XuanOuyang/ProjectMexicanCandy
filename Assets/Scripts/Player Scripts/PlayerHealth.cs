using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 5;
    public int currentHearts;

    public bool isDowned = false;
    public bool isDead = false;
    private bool hasBeenRevived = false; // Prevents infinite downs

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
    public Collider playerCollider;

    [Header("Combat")]
    public MonoBehaviour attackScript;

    [Header("New Downed UI")]
    public TextMeshProUGUI statusText;
    [HideInInspector] public bool isBeingRevived = false;

    [Header("In-Game Radius Visualizer")]
    [Tooltip("Drag your Line Renderer component here.")]
    public LineRenderer circleRenderer;
    [Tooltip("Match this to your physical invisible Sphere Collider radius!")]
    public float reviveRadius = 3f;
    [Tooltip("How smooth the circle looks. 40 is a great balance.")]
    public int circleSegments = 40;

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();

        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;

        if (statusText != null)
            statusText.text = "";

        // Hide circle at start of game
        if (circleRenderer != null)
            circleRenderer.enabled = false;
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

            // Continuously redraw the circle in case the player gets pushed or moved
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

        UpdateHeartsUI();

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

        if (playerCollider != null) playerCollider.isTrigger = true;
        if (playerRenderer != null) playerRenderer.material.color = downedColor;

        // ACTIVATES AND DRAWS THE CIRCLE
        if (circleRenderer != null)
        {
            circleRenderer.positionCount = circleSegments;
            circleRenderer.enabled = true;
            DrawCircleInGame();
        }
    }

    // Mathematical formula creating a flat ring on the floor plane around the player
    void DrawCircleInGame()
    {
        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angleRad = Mathf.Deg2Rad * (i * angleStep);

            float x = transform.position.x + Mathf.Cos(angleRad) * reviveRadius;
            float z = transform.position.z + Mathf.Sin(angleRad) * reviveRadius;

            // Placed slightly above the exact ground (0.1f) so it doesn't clip through textures
            float y = transform.position.y + 0.1f;

            circleRenderer.SetPosition(i, new Vector3(x, y, z));
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

        // HIDE THE CIRCLE
        if (circleRenderer != null)
            circleRenderer.enabled = false;

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

        // HIDE THE CIRCLE
        if (circleRenderer != null)
            circleRenderer.enabled = false;

        gameObject.SetActive(false);
    }
}