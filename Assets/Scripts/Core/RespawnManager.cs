using System.Collections;
using UnityEngine;

// What happens after the player dies: wait, lose a share of the coins, respawn at the spawn point
// with full health, then save so the penalty can't be undone by loading.
// Ported from MetrooDvania's RespawnManager; saving lives in SaveManager, checkpoints come later.
public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Seconds between dying and respawning (lets the Dead animation play). Paused with the game.")]
    [SerializeField, Min(0f)] private float respawnDelay = 2f;
    [Tooltip("Share of the coins lost on death (0.25 = 25%).")]
    [SerializeField, Range(0f, 1f)] private float coinLossOnDeath = 0.25f;

    private Health playerHealth;
    private Wallet playerWallet;
    private bool isRespawning;

    // Found here and not through player.playerHealth: our OnEnable can run before Player.Awake.
    private void Awake()
    {
        playerHealth = player.GetComponent<Health>();
        playerWallet = player.GetComponent<Wallet>();
    }

    private void OnEnable()
    {
        playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (!isRespawning) StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        isRespawning = true;

        // Scaled time, so opening the pause menu also pauses the countdown.
        yield return new WaitForSeconds(respawnDelay);

        int lost = playerWallet.LoseShare(coinLossOnDeath);
        player.Respawn(spawnPoint.position);
        saveManager.Save();

        Debug.Log($"Player respawned and lost {lost} coins.", this);
        isRespawning = false;
    }
}
