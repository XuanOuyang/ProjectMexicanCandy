using System.Collections;
using UnityEngine;

public class Pinyata : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Visual Feedback")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.15f;
    
    private SpriteRenderer enemyRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    [Header("Gravity Timer Settings")]
    public bool startWithGravity = false; 
    public float gravityDelayTimer = 5f;
    private Rigidbody rb;
    private Animator animator;

    [Header("Skipping Motion Settings")]
    public Transform visualModel;          // Assign 'Pinyata Renderer' child object here
    public float bounceFrequency = 12f;   // Speed of the bounce
    public float bounceHeight = 0.25f;    // Peak height of the bounce
    public float tiltAngle = 15f;         // Max tilt rotation (side to side)
    public bool animateOnlyWhenMoving = false; 

    private Vector3 initialVisualLocalPos;
    private Vector3 lastPosition;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Find SpriteRenderer (handles child or root)
        enemyRenderer = GetComponentInChildren<SpriteRenderer>();
        if (enemyRenderer != null) originalColor = enemyRenderer.color;

        // Auto-assign visualModel if missing
        if (visualModel == null && enemyRenderer != null)
        {
            visualModel = enemyRenderer.transform;
        }

        if (visualModel != null)
        {
            initialVisualLocalPos = visualModel.localPosition;
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = startWithGravity;
            if (!startWithGravity)
            {
                StartCoroutine(GravityTimerRoutine());
            }
        }
        
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
    }

    void Update()
    {
        AnimateSkipping();
    }

    private void AnimateSkipping()
    {
        if (visualModel == null) return;

        // Check if movement is required to skip
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (animateOnlyWhenMoving && speed < 0.1f)
        {
            // Reset to default local position/rotation when stationary
            visualModel.localPosition = Vector3.Lerp(visualModel.localPosition, initialVisualLocalPos, Time.deltaTime * 5f);
            visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            return;
        }

        // Calculate bounce (Absolute sine wave keeps Y exclusively bouncing upward)
        float bounceY = Mathf.Abs(Mathf.Sin(Time.time * bounceFrequency)) * bounceHeight;
        visualModel.localPosition = initialVisualLocalPos + new Vector3(0, bounceY, 0);

        // Calculate side-to-side tilt (Standard sine wave rocks left & right)
        float tiltZ = Mathf.Sin(Time.time * bounceFrequency) * tiltAngle;
        visualModel.localRotation = Quaternion.Euler(0, 0, tiltZ);
    }

    private IEnumerator GravityTimerRoutine()
    {
        yield return new WaitForSeconds(gravityDelayTimer);

        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Projectile>() != null)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (!isFlashing && enemyRenderer != null) StartCoroutine(FlashRedRoutine());
        if (currentHealth <= 0) Die();
    }

    private IEnumerator FlashRedRoutine()
    {
        isFlashing = true;
        enemyRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.color = originalColor;
        isFlashing = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}