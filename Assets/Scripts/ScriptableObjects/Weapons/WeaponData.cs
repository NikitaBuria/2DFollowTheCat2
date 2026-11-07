using UnityEngine;

// Read-only data for one weapon. Shared by everyone who references the asset,
// so never change these values at runtime.
// Later (Items step): inherit from ItemData and move name/icon there.
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    [field: SerializeField] public string weaponName { get; private set; }
    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public WeaponType weaponType { get; private set; }

    [Header("Damage")]
    [field: SerializeField, Min(0)] public int damage { get; private set; } = 10;
    [field: SerializeField, Min(0f)] public float knockback { get; private set; } = 5f;
    [field: SerializeField, Min(0f)] public float knockbackDuration { get; private set; } = 0.2f;

    [Header("Hit area")]
    // Radius of the hit circle around the player's attack point (the weapon's reach).
    [field: SerializeField, Min(0.1f)] public float attackRange { get; private set; } = 1f;
    // Total cone width in degrees, centered on the facing direction.
    [field: SerializeField, Range(0f, 360f)] public float attackArc { get; private set; } = 90f;

    [Header("Timing")]
    // How long the player stays in the attack state.
    [field: SerializeField, Min(0.05f)] public float attackDuration { get; private set; } = 0.4f;
}
