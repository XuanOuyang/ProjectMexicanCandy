using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Force Unity to add a Rigidbody to this GameObject
public class Projectile : MonoBehaviour
{
    public float lifetime = 10f; // Safeguard destruction timer

    [Header("Combat Settings")]
    public int damage = 1;  
    
    void Start()
    {
        // Destroy the bullet automatically after 'lifetime' seconds
        Destroy(gameObject, lifetime);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Ensure gravity is checked so it drops into a natural arc
            rb.useGravity = true; 
        }
    }          

    void Update()
    {
        // Let the Rigidbody handle linear movement naturally.
        
        // Dynamic Polish: Rotate the projectile to face its travel heading
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    // Handles trigger zone overlapping (if your projectile or target is a trigger)
    private void OnTriggerEnter(Collider collision)
    {
        HandleImpact(collision.gameObject);
    }

    // Handles hard physical impacts
    private void OnCollisionEnter(Collision collision)
    {
        HandleImpact(collision.gameObject);
    }

    private void HandleImpact(GameObject hitObject)
    {
        // Try getting the Pinyata component
        Pinyata pinyata = hitObject.GetComponent<Pinyata>();

        if (pinyata != null)
        {
            pinyata.TakeDamage(damage); 
            Destroy(gameObject);
            return; 
        }
        
        // Destructive logic on walls/floors
        if (hitObject.CompareTag("Wall") || hitObject.CompareTag("Floor"))
        {
            Destroy(gameObject);
            Debug.Log("Hit environment object: " + hitObject.name);
        }
    }
}