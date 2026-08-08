using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

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
        player1 = GameObject.Find("Player 1").transform;
        player2 = GameObject.Find("Player 2").transform;
    }

    void Update()
    {
        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateRate;

            Transform target = GetClosestValidPlayer();
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.ResetPath();
            }
        }

        Transform attackTarget = GetClosestValidPlayer();
        if (attackTarget != null)
        {
            float distance = Vector3.Distance(transform.position, attackTarget.position);
            if (distance <= attackRange)
            {
                TryAttack(attackTarget);
            }
        }
    }

    Transform GetClosestValidPlayer()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        if (IsValidTarget(player1))
        {
            float d1 = Vector3.Distance(transform.position, player1.position);
            if (d1 < closestDistance)
            {
                closestDistance = d1;
                closest = player1;
            }
        }

        if (IsValidTarget(player2))
        {
            float d2 = Vector3.Distance(transform.position, player2.position);
            if (d2 < closestDistance)
            {
                closestDistance = d2;
                closest = player2;
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