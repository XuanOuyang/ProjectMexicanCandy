using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Place this on a trigger-collider GameObject in the scene.
// When a player walks over it, it restores ammo and destroys itself.
//
// Setup checklist:
//  1. Add a Collider to this object and tick "Is Trigger".
//  2. Set candyIndex:
//       0  → restores Candy slot 1
//       1  → restores Candy slot 2
//       2  → restores Candy slot 3
//      -1  → restores ALL candy slots
//  3. Set ammoAmount to however many shots you want to give back.
// ─────────────────────────────────────────────────────────────────────────────
public class CandyBoxPickup : MonoBehaviour
{
    [Tooltip("Which candy slot to restore. -1 = restore all slots.")]
    [Range(-1, 2)]
    public int candyIndex = -1;

    [Tooltip("How many ammo charges this box grants.")]
    [Min(1)]
    public int ammoAmount = 5;

    [Tooltip("Optional VFX/SFX prefab spawned when the box is collected.")]
    public GameObject collectEffectPrefab;
    [Tooltip("Optional VFX/SFX prefab spawn lifetime")]
    [Min(0.1f)]
    public float EffectlifeTime = 2f;

    [Header("Sound Effect")]
    public AudioSource collectAudio;

    // ── Trigger ───────────────────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {

        GameObject audioObject = GameObject.Find("sparkle");
        if (audioObject != null)
        {
            collectAudio = audioObject.GetComponent<AudioSource>();
        }

        // Only care about objects that have a CandyShootingInput
        CandyShootingInput shooter = other.GetComponent<CandyShootingInput>();
        if (shooter == null) return;

        // Restore ammo
        shooter.RestoreAmmo(candyIndex, ammoAmount);

        // Play optional collect effect
        // Play optional collect effect and destroy the spawned instance after 'lifeTime' seconds
        if (collectEffectPrefab != null)
        {
            GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, EffectlifeTime); // Passes the instance ('effect') instead of the prefab blueprint
        }
        collectAudio.Play();

        // Remove the pickup from the scene
        Destroy(gameObject);
    }
}