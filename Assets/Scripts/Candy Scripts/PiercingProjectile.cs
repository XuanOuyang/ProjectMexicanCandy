using System.Collections.Generic;
using UnityEngine;

public class PiercingProjectile : Projectile
{
    [Header("Piercing Settings")]
    [Tooltip("Maximum number of enemies this candy can pierce before destroying itself.")]
    public int maxPierceCount = 3;

    private int _currentPierceCount = 0;
    // Track hit objects so it doesn't hit the same enemy multiple times in successive frames
    private readonly HashSet<GameObject> _hitObjects = new HashSet<GameObject>();

    public string nameOfSoundFX;

    protected override void HandleImpact(GameObject hitObject)
    {
        // Ignore if we already hit this target during this throw
        if (_hitObjects.Contains(hitObject)) return;

        Pinyata pinyata = hitObject.GetComponent<Pinyata>();

        if (pinyata != null)
        {
            _hitObjects.Add(hitObject);

            //Audio
            GameObject audioObject = GameObject.Find(nameOfSoundFX);

            AudioSource collectAudio = audioObject.GetComponent<AudioSource>();

            collectAudio.Play();

            // 1. Deal standard impact damage
            pinyata.TakeDamage(damage);

            // 2. Register Piercing hit with ComboTracker
            ComboTracker tracker = hitObject.GetComponent<ComboTracker>();
            if (tracker == null)
            {
                tracker = hitObject.AddComponent<ComboTracker>();
            }
            tracker.RegisterHit(CandyHitType.Piercing);

            // 3. Track pierce count and destroy if limit reached
            _currentPierceCount++;
            if (_currentPierceCount >= maxPierceCount)
            {
                Destroy(gameObject);
            }

            return; // Return early so we DON'T destroy the projectile on enemy hit
        }

        // Environment hit (Walls/Floors) — call base behavior to destroy on impact
        base.HandleImpact(hitObject);
    }
}