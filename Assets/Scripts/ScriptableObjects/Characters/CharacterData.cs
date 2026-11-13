using UnityEngine;

// Read-only base stats for one kind of character (player, enemy type, NPC).
// Shared by everyone who references the asset, so never change these values at runtime:
// runtime state like current health lives on the character's own components.
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Scriptable Objects/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Health")]
    [field: SerializeField, Min(1)] public int maxHealth { get; private set; } = 100;

    [Header("Movement")]
    [field: SerializeField, Min(0f)] public float walkSpeed { get; private set; } = 4f;
    [field: SerializeField, Min(0f)] public float runSpeed { get; private set; } = 6f;
    [field: SerializeField, Min(0f)] public float dashSpeed { get; private set; } = 10f;

    [Header("Hit Reaction")]
    // Seconds after a hit during which damage is ignored. Keep it longer than the knockback.
    [field: SerializeField, Min(0f)] public float invincibleTimer { get; private set; } = 1f;
}
