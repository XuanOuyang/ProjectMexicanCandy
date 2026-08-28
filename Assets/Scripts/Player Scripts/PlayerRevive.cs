using UnityEngine;

public class PlayerRevive: MonoBehaviour
{
    [Header("Revive Settings")]
    [Tooltip("Time in seconds required to revive a downed player.")]
    [SerializeField] private float reviveDuration = 3f;

    [Header("Revive Counter")]
    public bool hasUsedRevive = false;

    [Header("Editor Visualizer")]
    [SerializeField] private bool showRadiusGizmo = true;
    [SerializeField] private float visualRadius = 3f;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.4f);

    private float reviveTimer = 0f;
    private PlayerHealth myHealth;
    private PlayerHealth currentTargetHealth;

    private void Awake()
    {
        myHealth = GetComponent<PlayerHealth>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsTargetAPlayer(other) || other.gameObject == gameObject) return;
        if (hasUsedRevive) return;
        if (myHealth != null && (myHealth.isDowned || myHealth.isDead)) return;

        PlayerHealth targetHealth = other.GetComponent<PlayerHealth>();

        if (targetHealth != null && targetHealth.isDowned && !targetHealth.isDead)
        {
            currentTargetHealth = targetHealth;
            targetHealth.isBeingRevived = true;

            reviveTimer += Time.deltaTime;

            // Calculate the remaining time (e.g., 3.0 down to 0.0)
            float secondsRemaining = reviveDuration - reviveTimer;
            if (secondsRemaining < 0f) secondsRemaining = 0f;

            if (targetHealth.statusText != null)
            {
                // MATCHES YOUR BLEEDOUT LOOK: "Reviving: 3s" using whole numbers
                targetHealth.statusText.text = "Reviving: " + Mathf.CeilToInt(secondsRemaining) + "s";
            }

            if (reviveTimer >= reviveDuration)
            {
                targetHealth.isBeingRevived = false;
                targetHealth.Revive();
                hasUsedRevive = true;
                reviveTimer = 0f;
                currentTargetHealth = null;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTargetAPlayer(other))
        {
            if (currentTargetHealth != null && other.gameObject == currentTargetHealth.gameObject)
            {
                currentTargetHealth.isBeingRevived = false;
                currentTargetHealth = null;
            }
            reviveTimer = 0f;
        }
    }

    private bool IsTargetAPlayer(Collider other)
    {
        return other.CompareTag("Player 1") || other.CompareTag("Player 2");
    }

    private void OnDrawGizmos()
    {
        if (!showRadiusGizmo) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, visualRadius);
    }
}