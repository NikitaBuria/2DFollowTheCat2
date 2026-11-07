using UnityEngine;

// Read-only base stats for one kind of character (player, enemy type, NPC).
// Shared by everyone who references the asset, so never change these values at runtime:
// runtime state like current health lives on the character's own components.
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Scriptable Objects/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Info")]
    [field: SerializeField] public CharacterType characterType { get; private set; }

    [Header("Health")]
    [field: SerializeField, Min(1)] public int maxHealth { get; private set; } = 100;

    [Header("Movement")]
    [field: SerializeField, Min(0f)] public float walkSpeed { get; private set; } = 4f;
    [field: SerializeField, Min(0f)] public float runSpeed { get; private set; } = 6f;
    [field: SerializeField, Min(0f)] public float dashSpeed { get; private set; } = 10f;
}
