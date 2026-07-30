using System.Collections;
using UnityEngine;

public class PinataSpawner : MonoBehaviour
{
    [Header("Prefab")] [SerializeField] private GameObject pinataPrefab;

    [Header("Spawn Timing")] [SerializeField]
    private float minSpawnDelay = 5f;

    [SerializeField] private float maxSpawnDelay = 10f;

    [Header("Spawn Validation")] [SerializeField]
    private float spawnCheckRadius = 1f;

    [SerializeField] private float raycastHeight = 10f;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private int maxSpawnAttempts = 20;
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
            TrySpawnPinata();
        }
    }

    private void TrySpawnPinata()
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
                    Vector3 airSpawn = spawnPosition + Vector3.up * raycastHeight;
                    Instantiate(pinataPrefab, airSpawn, Quaternion.identity);
                    return;
                }
            }
        }

        Debug.LogWarning("No valid spawn location.");
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
        }

        return true;
    }
}