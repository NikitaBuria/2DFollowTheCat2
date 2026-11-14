using UnityEngine;

// Plays the player's CharacterData.dashSound. Listens to Player.OnDashStarted.
[RequireComponent(typeof(Player))]
public class DashSound : SoundPlayer
{
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.OnDashStarted += PlayDash;
    }

    private void OnDisable()
    {
        player.OnDashStarted -= PlayDash;
    }

    private void PlayDash() => Play(player.playerData.dashSound);
}
