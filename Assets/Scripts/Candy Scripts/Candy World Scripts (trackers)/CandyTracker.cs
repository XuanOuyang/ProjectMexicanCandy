using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Enum lives here so ComboTracker.cs is fully self-contained.
// DoTProjectile and PiercingProjectile reference it from here — no extra file needed.
// ─────────────────────────────────────────────────────────────────────────────
public enum CandyHitType
{
    Normal,
    DoT,
    Piercing
}

// ─────────────────────────────────────────────────────────────────────────────
// ComboTracker — added to an enemy at runtime when a combo-eligible projectile
// hits it. Do NOT place this on any prefab yourself.
//
// Each enemy tracks its own hit history independently, so a piercing candy that
// hits three enemies simultaneously checks combos on each one separately.
//
// To add a new combo:
//   1. Add its settings to CandyComboSettings.
//   2. Add a private Trigger___() method here.
//   3. Call it from CheckForCombos().
// ─────────────────────────────────────────────────────────────────────────────
public class ComboTracker : MonoBehaviour
{
    // Maps hit type → the Time.time at which it last landed
    private readonly Dictionary<CandyHitType, float> _recentHits
        = new Dictionary<CandyHitType, float>();

    private CandyComboSettings _settings;

    private void Start()
    {
        _settings = CandyComboSettings.Instance;

        if (_settings == null)
            Debug.LogWarning("[ComboTracker] No CandyComboSettings found in scene! " +
                             "Create a GameObject and attach CandyComboSettings to it.");
    }

    // ── Public API called by projectiles ─────────────────────────────────────

    /// <summary>
    /// Call this every time a candy of the given type damages this enemy.
    /// Automatically checks all combo conditions after recording the hit.
    /// </summary>
    public void RegisterHit(CandyHitType hitType)
    {
        _recentHits[hitType] = Time.time;
        Debug.Log($"[ComboTracker] {hitType} hit registered on {gameObject.name}.");
        CheckForCombos();
    }

    // ── Combo condition checks ────────────────────────────────────────────────

    private void CheckForCombos()
    {
        if (_settings == null) return;

        // ── Candy Rupture: DoT + Piercing ─────────────────────────────────────
        if (HasRecentHit(CandyHitType.DoT) && HasRecentHit(CandyHitType.Piercing))
        {
            TriggerCandyRupture();
            return;
        }

        // ── Add future combos below ───────────────────────────────────────────
        // if (HasRecentHit(CandyHitType.Normal) && HasRecentHit(CandyHitType.DoT))
        //     TriggerSugarFreeze();
    }

    private bool HasRecentHit(CandyHitType type)
    {
        return _recentHits.TryGetValue(type, out float hitTime)
               && (Time.time - hitTime) <= _settings.comboWindow;
    }

    // ── Combo effect implementations ─────────────────────────────────────────

private void TriggerCandyRupture()
{
    Debug.Log($"[Combo] CANDY RUPTURE on {gameObject.name}!");

    // Clear so the combo can't chain-fire on the very next hit
    _recentHits.Clear();

    // Burst damage to the enemy that triggered the combo
    Pinyata self = GetComponent<Pinyata>();
    if (self != null)
        self.TakeDamage(_settings.ruptBurstDamage);


    // Audio
    if (_settings.ruptureSFX != null)
    {
        AudioSource.PlayClipAtPoint(_settings.ruptureSFX, transform.position);
    }
        

        // AOE splash to all nearby enemies
        Collider[] nearby = Physics.OverlapSphere(transform.position, _settings.ruptAoeRadius);
    foreach (Collider col in nearby)
    {
        if (col.gameObject == gameObject) continue;

        Pinyata nearbyPinyata = col.GetComponent<Pinyata>();
        if (nearbyPinyata != null)
        {
            nearbyPinyata.TakeDamage(_settings.ruptAoeDamage);
            Debug.Log($"[Combo] AOE hit {col.gameObject.name} for {_settings.ruptAoeDamage}.");
        }
    }

    // Optional VFX — Instantiated and automatically destroyed after ruptureVfxLifetime seconds
    if (_settings.ruptureVFXPrefab != null)
    {
        GameObject vfxInstance = Instantiate(_settings.ruptureVFXPrefab, transform.position, Quaternion.identity);
        Destroy(vfxInstance, _settings.ruptureVfxLifetime);
    }
}

    // Scene-view gizmo so you can see the AOE radius while playtesting
    private void OnDrawGizmosSelected()
    {
        if (_settings == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, _settings.ruptAoeRadius);
    }
}