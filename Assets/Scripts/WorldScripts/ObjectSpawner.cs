using System.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Prefab")] 
    [SerializeField] private GameObject prefab;

    [Header("Spawn Timing")] 
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 10f;

    [Header("Spawn Validation")] 
    [SerializeField] private float spawnCheckRadius = 1f;
    [SerializeField] private int maxSpawnAttempts = 20;

    [Header("Raycast")] 
    [SerializeField] private float raycastHeight = 10f;
    [SerializeField] private float raycastDistance = 20f;

    [Header("Air Spawn")] 
    [SerializeField] private bool spawnInAir = false;
    [SerializeField] private float airSpawnHeight = 10f;
    [SerializeField] private float airHoldTime = 3f;

    private BoxCollider spawnArea;

    private void Awake()
    {
        spawnArea = GetComponent<BoxCollider>();
        spawnArea.isTrigger = true;
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
            TrySpawnObject();
        }
    }

    private void TrySpawnObject()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPosition = GetRandomPosition();
            if (Physics.Raycast(
                    randomPosition + Vector3.up * raycastHeight,
                    Vector3.down,
                    out RaycastHit hit,
                    raycastDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 spawnPosition = hit.point;

                if (IsSpawnLocationValid(spawnPosition))
                {
                    GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);

                    Collider col = obj.GetComponent<Collider>();

                    if (col != null && !spawnInAir)
                    {
                        float offset = hit.point.y - col.bounds.min.y;
                        obj.transform.position += Vector3.up * offset;
                    }

                    if (spawnInAir)
                    {
                        obj.transform.position += Vector3.up * airSpawnHeight;
                        
                        // Handle air hold logic if time > 0
                        if (airHoldTime > 0f)
                        {
                            StartCoroutine(HoldInAirRoutine(obj, airHoldTime));
                        }
                    }

                    return;
                }
            }
        }

        Debug.LogWarning("No valid spawn location.");
    }

    private IEnumerator HoldInAirRoutine(GameObject obj, float holdTime)
    {
        if (obj == null) yield break;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        
        bool originalUseGravity = false;
        bool originalIsKinematic = false;

        // Freeze movement while floating
        if (rb != null)
        {
            originalUseGravity = rb.useGravity;
            originalIsKinematic = rb.isKinematic;

            rb.useGravity = false;
            rb.isKinematic = true; // Prevents velocity buildup or collisions pushing it down
        }

        yield return new WaitForSeconds(holdTime);

        // Restore physics after airHoldTime
        if (obj != null && rb != null)
        {
            rb.isKinematic = originalIsKinematic;
            rb.useGravity = originalUseGravity;
        }
    }

    private Vector3 GetRandomPosition()
    {
        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, bounds.max.y, z);
    }

    private bool IsSpawnLocationValid(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, spawnCheckRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Wall")) return false;
            if (col.CompareTag("Player")) return false;
            if (col.CompareTag("Enemy")) return false;
            if (col.CompareTag("Candy")) return false;
        }

        return true;
    }
}