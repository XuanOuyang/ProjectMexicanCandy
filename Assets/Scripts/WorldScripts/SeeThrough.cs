using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SeeThrough : MonoBehaviour
{
    [Header("Tags to Detect")]
    [SerializeField] private List<string> targetTags = new List<string> { "Player 1", "Player 2", "Enemy" };

    [Header("Material Settings")]
    [SerializeField] private Material targetMaterial; // Drag your 'Stone Green' material here
    [SerializeField] private float transparentAlpha = 0.3f;
    [SerializeField] private float opaqueAlpha = 1.0f;

    // Track how many valid objects are currently inside the trigger
    private int objectsInsideCount = 0;

    private void Start()
    {
        // Ensure initial state is opaque if material is assigned
        if (targetMaterial != null)
        {
            SetMaterialAlpha(opaqueAlpha);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetTags.Contains(other.tag))
        {
            objectsInsideCount++;

            // Fade to transparent on the first valid entity entering
            if (objectsInsideCount == 1)
            {
                SetMaterialAlpha(transparentAlpha);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetTags.Contains(other.tag))
        {
            objectsInsideCount--;

            // Return to opaque only when all valid entities have left
            if (objectsInsideCount <= 0)
            {
                objectsInsideCount = 0;
                SetMaterialAlpha(opaqueAlpha);
            }
        }
    }

    private void SetMaterialAlpha(float alpha)
    {
        if (targetMaterial == null) return;

        Color currentColor = targetMaterial.color;
        currentColor.a = alpha;
        targetMaterial.color = currentColor;
    }
}