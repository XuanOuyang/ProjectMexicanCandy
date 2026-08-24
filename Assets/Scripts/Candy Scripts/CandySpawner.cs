using UnityEngine;

public class CandySpawner : MonoBehaviour
{
    [Header("Candy Prefabs")]
    public GameObject[] candyPrefabs;

    [Header("Spawn Area")]
    public MeshCollider spawnArea;

    [Header("Spawn Settings")]
    public float spawnInterval = 10f;
    public int maxCandyOnMap = 10;
    public float raycastHeight = 10f;
    public float maxFloorAngle = 30f;

    [Header("Layer Mask")]
    public LayerMask floorMask;

    private float spawnTimer;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnCandy();
        }
    }

    void SpawnCandy()
    {
        if (candyPrefabs == null || candyPrefabs.Length == 0)
            return;

        if (spawnArea == null)
            return;

        if (GameObject.FindGameObjectsWithTag("Candy").Length >= maxCandyOnMap)
            return;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomPoint = GetRandomPointInBounds(spawnArea.bounds);
            Vector3 rayStart = randomPoint + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastHeight * 2f, floorMask))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);

                if (angle <= maxFloorAngle)
                {
                    GameObject candyPrefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
                    Instantiate(candyPrefab, hit.point, Quaternion.identity);
                    return;
                }
            }
        }
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(randomX, bounds.max.y, randomZ);
    }
}