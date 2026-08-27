using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Targeting Settings")]
    public string[] playerTags = new string[] { "Player 1", "Player 2" };

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public int damage = 1;
    public float attackCooldown = 1f;
    public float targetUpdateRate = 0.2f;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private float nextTargetUpdateTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Don't run AI destination calls if agent isn't active or on the NavMesh yet
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Transform currentTarget = GetClosestValidPlayer();

        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateRate;

            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.position);
            }
            else
            {
                agent.ResetPath();
            }
        }

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange)
            {
                TryAttack(currentTarget);
            }
        }
    }

    Transform GetClosestValidPlayer()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (string tag in playerTags)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject playerObj in players)
            {
                Transform playerTransform = playerObj.transform;

                if (IsValidTarget(playerTransform))
                {
                    float distance = Vector3.Distance(transform.position, playerTransform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = playerTransform;
                    }
                }
            }
        }

        return closest;
    }

    bool IsValidTarget(Transform player)
    {
        if (player == null) return false;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) return false;

        return !health.isDead && !health.isDowned;
    }

    void TryAttack(Transform target)
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        PlayerHealth health = target.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}