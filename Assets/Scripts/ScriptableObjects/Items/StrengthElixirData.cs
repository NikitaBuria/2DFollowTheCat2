using UnityEngine;

// A usable item that makes whoever drinks it hit harder for a while.
[CreateAssetMenu(fileName = "NewStrengthElixir", menuName = "Scriptable Objects/Strength Elixir Data")]
public class StrengthElixirData : ItemData, IUsable
{
    [field: SerializeField, Min(1)] public int bonusDamage { get; private set; } = 1;
    [field: SerializeField, Min(1f)] public float duration { get; private set; } = 30f;

    public bool Use(GameObject user)
    {
        if (!user.TryGetComponent(out StrengthBuff buff)) return false;
        buff.Apply(bonusDamage, duration);
        return true;
    }
}
