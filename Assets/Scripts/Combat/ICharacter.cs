// Anything with CharacterData (player, enemies). Lets shared components like HitSound and DeathSound
// read the owner's data without knowing whether it is a Player or an Enemy.
public interface ICharacter
{
    CharacterData characterData { get; }
}
