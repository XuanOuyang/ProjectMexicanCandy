using UnityEngine;

public class TriggerZoneFade : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the empty child GameObject containing the trigger Box Collider here.")]
    public GameObject triggerZone;

    [Header("Opacity Settings")]
    [Range(0f, 1f)] public float transparentAlpha = 0.3f;
    private float originalAlpha;

    private Renderer objRenderer;
    private Color currentColor;
    private int overlappingCount = 0;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            currentColor = objRenderer.material.color;
            originalAlpha = currentColor.a;
        }

        // Automatically attach the forwarding component to the designated trigger zone
        if (triggerZone != null)
        {
            TriggerListener listener = triggerZone.GetComponent<TriggerListener>();
            if (listener == null)
            {
                listener = triggerZone.AddComponent<TriggerListener>();
            }
            listener.Initialize(this);
        }
        else
        {
            Debug.LogWarning("Please assign a Trigger Zone GameObject in the Inspector!", this);
        }
    }

    public void OnEntityEnter()
    {
        overlappingCount++;
        SetOpacity(transparentAlpha);
    }

    public void OnEntityExit()
    {
        overlappingCount--;
        if (overlappingCount <= 0)
        {
            overlappingCount = 0;
            SetOpacity(originalAlpha);
        }
    }

    private void SetOpacity(float alpha)
    {
        if (objRenderer == null) return;
        currentColor.a = alpha;
        objRenderer.material.color = currentColor;
    }
}

// Internal helper script injected onto the Trigger Zone object
public class TriggerListener : MonoBehaviour
{
    private TriggerZoneFade parentFadeScript;

    public void Initialize(TriggerZoneFade fadeScript)
    {
        parentFadeScript = fadeScript;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player 1") || other.CompareTag("Player 2") || other.CompareTag("Enemy"))
        {
            parentFadeScript?.OnEntityEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player 1") || other.CompareTag("Player 2") || other.CompareTag("Enemy"))
        {
            parentFadeScript?.OnEntityExit();
        }
    }
}