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
        if (isDead || isDowned)
            return;

        currentHearts -= amount;
        if (currentHearts < 0)
            currentHearts = 0;

        UpdateHeartsUI();

        if (currentHearts <= 0)
        {
            Downed();
        }
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
}