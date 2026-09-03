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

    [Header("VFX")]
    public GameObject piruliVFX;
    public GameObject damageVFX;

    private Vector3 initialVisualLocalPos;
    private Vector3 lastPosition;

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

        // Disable physics dynamics so physics doesn't conflict with the agent
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; 
        }

        // Snap down onto the NavMesh and enable the agent
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
        if (other.GetComponent<Projectile>() != null)
        {
            TakeDamage(1);
        }

        PiercingProjectile projectile = other.GetComponent<PiercingProjectile>();

        if (projectile != null)
        {
            ParticleSystem piruliFX = Instantiate(piruliVFX.GetComponent<ParticleSystem>(), transform.position, transform.rotation);
            piruliFX.Play();
            Debug.Log("vfx played");
            //Audio
            GameObject audioObject = GameObject.Find("stab");
            AudioSource collectAudio = audioObject.GetComponent<AudioSource>();

            collectAudio.Play();
        }
        else
        {
            ParticleSystem projectileFX = Instantiate(damageVFX.GetComponent<ParticleSystem>(), transform.position, transform.rotation);
            projectileFX.Play();

            GameObject audioObject = GameObject.Find("PinataHit");
            AudioSource hitSound = audioObject.GetComponent<AudioSource>();

            hitSound.Play();
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