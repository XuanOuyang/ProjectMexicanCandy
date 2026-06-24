using UnityEngine;

public class Projectile : MonoBehaviour
{
    // maybe add an arc to the object so it drops?
    public float speed = 20f;
    public float lifetime = 5f; // Destroys the bullet after 3 seconds so your game doesn't lag

    [Header("Combat Settings")]
    public int damage = 1;  
    void Start()
    {
        // Destroy the bullet automatically after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }          

    void Update()
    {
        // Move the bullet forward dynamically every frame
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Optional: Destroy the bullet if it hits a wall
    private void OnTriggerEnter(Collider collision)
    {
        Pinyata pinyata = collision.GetComponent<Pinyata>();

        if (pinyata != null)
        {
            // 3. Deal damage directly to that specific enemy!
            pinyata.TakeDamage(damage);
            
            // Destroy the bullet after it deals damage
            Destroy(gameObject);
            return; // Exit the function early so it doesn't look for walls
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
                Destroy(gameObject);
                Debug.Log("Hit " + collision.gameObject.name);
        }
    }
}