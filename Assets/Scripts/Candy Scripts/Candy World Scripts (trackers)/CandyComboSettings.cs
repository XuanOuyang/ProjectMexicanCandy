using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// CandyComboSettings — Singleton that holds every combo's tuning values.
//
// Setup:
//   1. Create an empty GameObject in your scene, name it "CandyComboSettings".
//   2. Attach this script to it.
//   3. Tune the values in the Inspector.
//
// ComboTracker reads from CandyComboSettings.Instance at runtime,
// so you never have to touch those values in code.
// ─────────────────────────────────────────────────────────────────────────────
public class CandyComboSettings : MonoBehaviour
{
    public static CandyComboSettings Instance { get; private set; }

    // ── Shared ────────────────────────────────────────────────────────────────
    [Header("Combo Window (all combos)")]
    [Tooltip("How many seconds between two different hits still counts as a combo")]
    public float comboWindow = 3f;

    // ── Candy Rupture  (DoT + Piercing) ───────────────────────────────────────
    [Header("Candy Rupture  —  DoT + Piercing")]
    [Tooltip("Extra burst damage dealt to the enemy the combo triggers on")]
    public int ruptBurstDamage = 5;

    [Tooltip("Radius of the AOE splash that hits nearby enemies")]
    public float ruptAoeRadius = 3f;

    [Tooltip("Damage dealt to every enemy caught in the AOE splash")]
    public int ruptAoeDamage = 3;

    [Tooltip("Optional VFX prefab spawned at the combo's origin point")]
    public GameObject ruptureVFXPrefab;

    [Tooltip("How many seconds before the spawned Rupture VFX GameObject is destroyed.")]
    [Min(0f)]
    public float ruptureVfxLifetime = 2f;

    // ── Future combos go here ─────────────────────────────────────────────────
    // [Header("Sugar Freeze  —  Normal + DoT")]
    // public float freezeDuration = 2f;
    // etc.

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[CandyComboSettings] Duplicate instance destroyed.");
            Destroy(gameObject);
        }
    }
}