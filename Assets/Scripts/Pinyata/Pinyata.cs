using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

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

    [Header("VFX Settings")]
    public GameObject piruliVFX;
    public GameObject damageVFX;
    public GameObject burningVFXPrefab;  // Continuous effect when hit by Pica Fresa (DoT)
    public GameObject deathVFXPrefab;    // VFX spawned when Piñata dies
    public float deathVFXLifetime = 3f;   // Time before death VFX is destroyed

    private Vector3 initialVisualLocalPos;
    private Vector3 lastPosition;
    private GameObject activeBurnEffect;

    void Start()
    {
        currentHealth = maxHealth;
        
        enemyRenderer = GetComponentInChildren<SpriteRenderer>();
        if (enemyRenderer != null) originalColor = enemyRenderer.color;

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
        }

        StartCoroutine(GravityTimerRoutine());
        
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

        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (animateOnlyWhenMoving && speed < 0.1f)
        {
            visualModel.localPosition = Vector3.Lerp(visualModel.localPosition, initialVisualLocalPos, Time.deltaTime * 5f);
            visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            return;
        }

        float bounceY = Mathf.Abs(Mathf.Sin(Time.time * bounceFrequency)) * bounceHeight;
        visualModel.localPosition = initialVisualLocalPos + new Vector3(0, bounceY, 0);

        float tiltZ = Mathf.Sin(Time.time * bounceFrequency) * tiltAngle;
        visualModel.localRotation = Quaternion.Euler(0, 0, tiltZ);
    }

    private IEnumerator GravityTimerRoutine()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        if (!startWithGravity)
        {
            yield return new WaitForSeconds(gravityDelayTimer);
        }

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; 
        }

        if (agent != null)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 15f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Cache component references
        Projectile baseProjectile = other.GetComponent<Projectile>();
        PiercingProjectile piercingProjectile = other.GetComponent<PiercingProjectile>();
        DoTProjectile dotProjectile = other.GetComponent<DoTProjectile>();

        if (baseProjectile == null && piercingProjectile == null && dotProjectile == null) return;

        TakeDamage(1);

        // 1. Handle Pica Fresa (DoT Projectile) Burn Effect
        if (dotProjectile != null)
        {
            if (burningVFXPrefab != null && activeBurnEffect == null)
            {
                // Instantiate attached to this transform so it stays with the enemy while moving
                activeBurnEffect = Instantiate(burningVFXPrefab, transform.position, Quaternion.identity, transform);
            }
        }
        // 2. Handle Piercing Projectile Effects
        else if (piercingProjectile != null)
        {
            if (piruliVFX != null && piruliVFX.TryGetComponent<ParticleSystem>(out var psPrefab))
            {
                ParticleSystem piruliFX = Instantiate(psPrefab, transform.position, transform.rotation);
                piruliFX.Play();
            }

            GameObject audioObject = GameObject.Find("stab");
            if (audioObject != null && audioObject.TryGetComponent<AudioSource>(out var collectAudio))
            {
                collectAudio.Play();
            }
        }
        // 3. Handle Standard Projectile Effects
        else
        {
            if (damageVFX != null && damageVFX.TryGetComponent<ParticleSystem>(out var psPrefab))
            {
                ParticleSystem projectileFX = Instantiate(psPrefab, transform.position, transform.rotation);
                projectileFX.Play();
            }

            GameObject audioObject = GameObject.Find("PinataHit");
            if (audioObject != null && audioObject.TryGetComponent<AudioSource>(out var hitSound))
            {
                hitSound.Play();
            }
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
        // Spawn Death VFX and destroy it after specified delay
        if (deathVFXPrefab != null)
        {
            GameObject deathFX = Instantiate(deathVFXPrefab, transform.position, transform.rotation);
            Destroy(deathFX, deathVFXLifetime);
        }

        Destroy(gameObject);
    }
}