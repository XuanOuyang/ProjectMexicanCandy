using UnityEngine;
using UnityEngine.VFX;

//SOURCE:https://qriva.github.io/posts/how-to-vfx-graph/#how-to-send-event-with-attributes

public class ImpactFX : MonoBehaviour
{
    public VisualEffect vfx;
    private VFXEventAttribute eventAttribute;
    public Rigidbody player;

    void Start()
    {
        // Create vfx event attribute object
        eventAttribute = vfx.CreateVFXEventAttribute();

        if (player != null)
        {
            eventAttribute.SetVector3("Position", player.transform.position);
        }
    }
    
    void Update()
    {
            eventAttribute.SetVector3("Position", player.transform.position);
    }

    // Call this method to the send event
    public void PlayVFX()
    {
        // Set event data

        //eventAttribute.SetFloat("Size", Random.Range(0f, 1f));
        eventAttribute.SetVector3("Position", player.transform.position);

        // Data is copied from eventAttribute, so this object can be used again
        vfx.SendEvent("OnImpact", eventAttribute);
    }
}
