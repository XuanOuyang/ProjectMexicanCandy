using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 5;
    public int currentHearts;

    public bool isDowned = false;
    public bool isDead = false;

    public float downedTime = 10f;
    private float downedTimer;

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
    public MonoBehaviour attackScript; // drag your attack script here

    [Header("Audio")]
    public AudioSource hitSound;

    [Header("Backwards Hit")]
    public float height = 2f;
    public float backDistance = 5f;

    [Header("VFX")]
    public ImpactFX impact;

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();

        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }

        if (isDowned && !isDead)
        {
            downedTimer -= Time.deltaTime;
            if (downedTimer <= 0f)
            {
                Die();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || isDowned || isInvincible)
            return;

        currentHearts -= amount;
        if (currentHearts < 0)
            currentHearts = 0;

        hitSound.Play();
        impact.PlayVFX();
        UpdateHeartsUI();
        HitByPinata();

        if (currentHearts <= 0)
        {
            Downed();
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

        if (movementScript != null)
            movementScript.enabled = false;

        if (attackScript != null)
            attackScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (playerRenderer != null)
            playerRenderer.material.color = downedColor;
    }

    public void Revive()
    {
        if (!isDowned || isDead)
            return;

        isDowned = false;
        currentHearts = 1;
        UpdateHeartsUI();

        if (movementScript != null)
            movementScript.enabled = true;

        if (attackScript != null)
            attackScript.enabled = true;

        if (rb != null)
            rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (playerRenderer != null)
            playerRenderer.material.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        isDowned = false;
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