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
        
        private Renderer enemyRenderer;
        private Color originalColor;
        private bool isFlashing = false;

        // --- NEW: GRAVITY TIMER VARIABLES ---
        [Header("Gravity Timer Settings")]
        public bool startWithGravity = false; 
        public float gravityDelayTimer = 5f; // How many seconds to wait before gravity kicks in
        private Rigidbody rb;
        private Animator animator;

        void Start()
        {
            currentHealth = maxHealth;
            enemyRenderer = GetComponent<Renderer>();
            if (enemyRenderer != null) originalColor = enemyRenderer.material.color;

            // 1. Get the Rigidbody and set up initial gravity state
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = startWithGravity;
                
                // If we don't want gravity right away, start the countdown timer!
                if (!startWithGravity)
                {
                    StartCoroutine(GravityTimerRoutine());
                }
            }
            animator = GetComponent<Animator>();
        }

        // 2. The Timer Coroutine
        private IEnumerator GravityTimerRoutine()
        {
            // Wait in the background for X seconds
            yield return new WaitForSeconds(gravityDelayTimer);

            // Timer is finished! Turn gravity back on
            if (rb != null)
            {
                rb.useGravity = true;
                
                // OPTIONAL: If you froze the Y-axis constraints earlier to keep them flat, 
                // we must unfreeze Y so they can physically fall down!
                rb.constraints &= ~RigidbodyConstraints.FreezePositionY; 
                
                Debug.Log("Enemy gravity activated by timer!");
            }
        }

        // --- Keep your existing OnTriggerEnter, TakeDamage, and Flash routines below ---
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
            enemyRenderer.material.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            enemyRenderer.material.color = originalColor;
            isFlashing = false;
        }

        void Die()
        {
            Destroy(gameObject);
        }
    }