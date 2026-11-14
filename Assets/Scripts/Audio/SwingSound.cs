using UnityEngine;

// Plays the swung weapon's WeaponData.swingSound. Listens to PlayerCombat.OnSwing.
[RequireComponent(typeof(PlayerCombat))]
public class SwingSound : SoundPlayer
{
    private PlayerCombat playerCombat;

    protected override void Awake()
    {
        base.Awake();
        playerCombat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        playerCombat.OnSwing += PlaySwing;
    }

    private void OnDisable()
    {
        playerCombat.OnSwing -= PlaySwing;
    }

    private void PlaySwing(WeaponData weapon) => Play(weapon != null ? weapon.swingSound : null);
}
