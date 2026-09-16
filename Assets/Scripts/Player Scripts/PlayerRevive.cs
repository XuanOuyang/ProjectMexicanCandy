using UnityEngine;

public class PlayerRevive : MonoBehaviour
{
    [Header("Revive Settings")]
    [Tooltip("Time in seconds required to revive a downed player.")]
    [SerializeField] private float reviveDuration = 3f;

    [Header("Revive Counter")]
    public bool hasUsedRevive = false;

    [Header("VFX Settings")]
    [Tooltip("Continuous looping VFX attached to the player while actively reviving.")]
    public GameObject reviveChannelVFXPrefab;
    [Tooltip("Burst VFX spawned when the revive successfully completes.")]
    public GameObject reviveCompleteVFXPrefab;
    [Tooltip("Time in seconds before the completion VFX object is destroyed.")]
    public float completeVFXLifetime = 3f;

    [Header("Audio Settings")]
    public AudioClip reviveSound;
    [Range(0f, 1f)]
    public float reviveSoundVolume = 0.5f;

    [Header("Editor Visualizer")]
    [SerializeField] private bool showRadiusGizmo = true;
    [SerializeField] private float visualRadius = 3f;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.4f);

    private float reviveTimer = 0f;
    private PlayerHealth myHealth;
    private PlayerHealth currentTargetHealth;
    private GameObject activeChannelVFX;

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

            // Spawn loop channel VFX attached to the target player while reviving
            if (reviveChannelVFXPrefab != null && activeChannelVFX == null)
            {
                activeChannelVFX = Instantiate(reviveChannelVFXPrefab, targetHealth.transform.position, Quaternion.identity, targetHealth.transform);
            }

            reviveTimer += Time.deltaTime;

            // Calculate the remaining time (e.g., 3.0 down to 0.0)
            float secondsRemaining = reviveDuration - reviveTimer;
            if (secondsRemaining < 0f) secondsRemaining = 0f;

            if (targetHealth.statusText != null)
            {
                targetHealth.statusText.text = "Reviving: " + Mathf.CeilToInt(secondsRemaining) + "s";
            }

            // Revive Completed
            if (reviveTimer >= reviveDuration)
            {
                targetHealth.isBeingRevived = false;
                targetHealth.transform.position += Vector3.up * 0.5f;

                // Spawn completion burst VFX
                if (reviveCompleteVFXPrefab != null)
                {
                    GameObject burstFX = Instantiate(reviveCompleteVFXPrefab, targetHealth.transform.position, Quaternion.identity);
                    Destroy(burstFX, completeVFXLifetime);
                }

                // Clean up channeling VFX
                StopChannelVFX();

                targetHealth.Revive();

                if (reviveSound != null)
                {
                    AudioSource.PlayClipAtPoint(reviveSound, transform.position, reviveSoundVolume);
                }

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

            StopChannelVFX();
            reviveTimer = 0f;
        }
    }

    private void StopChannelVFX()
    {
        if (activeChannelVFX != null)
        {
            Destroy(activeChannelVFX);
            activeChannelVFX = null;
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