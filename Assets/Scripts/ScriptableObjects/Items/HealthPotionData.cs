using UnityEngine;

// A usable item that heals whoever uses it.
[CreateAssetMenu(fileName = "NewHealthPotion", menuName = "Scriptable Objects/Health Potion Data")]
public class HealthPotionData : ItemData, IUsable
{
    [field: SerializeField, Min(1)] public int healAmount { get; private set; } = 3;

    public bool Use(GameObject user)
    {
        if (!user.TryGetComponent(out Health health)) return false;
        return health.Heal(healAmount);
    }
}
