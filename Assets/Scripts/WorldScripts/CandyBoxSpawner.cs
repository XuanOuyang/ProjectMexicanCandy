using UnityEngine;

/// <summary>
/// Child spawner specifically designed for spawning CandyBoxPickup prefabs.
/// Selects a random prefab from an array and assigns random ammo amounts.
/// </summary>
public class CandyBoxSpawner : ObjectSpawner
{
    [Header("Candy Box Prefabs")]
    [Tooltip("Assign 4 prefabs here (e.g., Index 0, 1, 2, and -1 for All Slots).")]
    [SerializeField] private CandyBoxPickup[] candyBoxPrefabs;

    [Header("Ammo Settings")]
    [Tooltip("If true, ammo amounts will be randomized between min and max.")]
    [SerializeField] private bool randomizeAmmo = true;
    [SerializeField] private int minAmmo = 3;
    [SerializeField] private int maxAmmo = 10;

    /// <summary>
    /// Overrides the base spawn behavior to handle multiple candy box prefabs and random ammo setup.
    /// </summary>
    protected override void TrySpawnObject()
    {
        if (candyBoxPrefabs == null || candyBoxPrefabs.Length == 0)
        {
            Debug.LogWarning("CandyBoxSpawner: No candy box prefabs assigned!");
            return;
        }

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
                    // 1. Pick a random candy box prefab
                    CandyBoxPickup selectedPrefab = candyBoxPrefabs[Random.Range(0, candyBoxPrefabs.Length)];

                    // 2. Instantiate the chosen prefab
                    CandyBoxPickup spawnedBox = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

                    // 3. Configure random ammo if enabled
                    if (randomizeAmmo)
                    {
                        spawnedBox.ammoAmount = Random.Range(minAmmo, maxAmmo + 1);
                    }

                    // 4. Handle ground collider offset positioning
                    Collider col = spawnedBox.GetComponent<Collider>();
                    if (col != null && !spawnInAir)
                    {
                        float offset = hit.point.y - col.bounds.min.y;
                        spawnedBox.transform.position += Vector3.up * offset;
                    }

                    // 5. Handle air spawning routine
                    if (spawnInAir)
                    {
                        spawnedBox.transform.position += Vector3.up * airSpawnHeight;

                        if (airHoldTime > 0f)
                        {
                            StartCoroutine(HoldInAirRoutine(spawnedBox.gameObject, airHoldTime));
                        }
                    }

                    return;
                }
            }
        }

        Debug.LogWarning("CandyBoxSpawner: No valid spawn location found.");
    }
}