using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FastFallRigidbody : MonoBehaviour
{
    [Header("Falling Physics")]
    public float fallMultiplier = 2.5f; // Multiplies default gravity on descent

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Check if player is falling downwards
        if (rb.velocity.y < 0)
        {
            // Apply extra gravity force continuously on falling frames
            Vector3 extraGravity = Vector3.up * Physics.gravity.y * (fallMultiplier - 1);
            rb.AddForce(extraGravity, ForceMode.Acceleration);
        }
    }
}