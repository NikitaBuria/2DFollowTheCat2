// Changes incoming damage before Health applies it (e.g. armor). Optional: Health uses the one on
// its own GameObject if there is one, so enemies without it take full damage.
public interface IDamageModifier
{
    // Returns the damage that is actually taken.
    int ModifyIncomingDamage(int amount);
}
