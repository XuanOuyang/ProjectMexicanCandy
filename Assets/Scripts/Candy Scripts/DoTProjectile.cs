using System.Collections;
using UnityEngine;

public class DoTProjectile : Projectile
{
    [Header("DoT Settings")]
    [Tooltip("Total damage applied per tick over time.")]
    public int tickDamage = 1;

    [Tooltip("Time in seconds between each damage tick.")]
    public float tickInterval = 0.5f;

    [Tooltip("How many total damage ticks occur.")]
    public int totalTicks = 4;

    protected override void HandleImpact(GameObject hitObject)
    {
        Pinyata pinyata = hitObject.GetComponent<Pinyata>();

        if (pinyata != null)
        {
            // 1. Deal initial impact damage (if any)
            pinyata.TakeDamage(damage);

            // 2. Register DoT hit with ComboTracker
            ComboTracker tracker = hitObject.GetComponent<ComboTracker>();
            if (tracker == null)
            {
                tracker = hitObject.AddComponent<ComboTracker>();
            }
            tracker.RegisterHit(CandyHitType.DoT);

            // 3. Start DoT Coroutine on the enemy target so it persists after the projectile is destroyed
            pinyata.StartCoroutine(ApplyDamageOverTime(pinyata));

            // 4. Destroy the projectile visual on impact
            Destroy(gameObject);
            return;
        }

        // Fallback for environment collision
        base.HandleImpact(hitObject);
    }

    private IEnumerator ApplyDamageOverTime(Pinyata target)
    {
        for (int i = 0; i < totalTicks; i++)
        {
            yield return new WaitForSeconds(tickInterval);

            // Safety check: ensure target wasn't destroyed mid-duration
            if (target != null)
            {
                target.TakeDamage(tickDamage);
            }
            else
            {
                yield break;
            }
        }
    }
}