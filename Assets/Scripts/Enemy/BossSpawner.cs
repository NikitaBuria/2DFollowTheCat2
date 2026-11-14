using UnityEngine;

// Puts the boss at one random spawn point when the scene starts, like a unique monster in a roguelike.
// Every visit to the scene rolls again, so the boss can always be found and farmed.
public class BossSpawner : MonoBehaviour
{
    [SerializeField] private Enemy bossPrefab;
    // Hand-placed spots on the floor; one is picked at random.
    [SerializeField] private Transform[] spawnPoints = new Transform[0];
    // Chance that the boss is in the scene on this visit (1 = always).
    [SerializeField, Range(0f, 1f)] private float spawnChance = 1f;

    private void Start()
    {
        if (bossPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: BossSpawner needs a boss prefab and at least one spawn point.", this);
            return;
        }

        // Random.value can be exactly 1, so ">" keeps spawnChance 1 always spawning.
        if (Random.value > spawnChance) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(bossPrefab, point.position, Quaternion.identity, transform);
    }

    // Shows the spawn points in the Scene view when the spawner is selected.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        foreach (Transform point in spawnPoints)
            if (point != null) Gizmos.DrawWireSphere(point.position, 1f);
    }
}
