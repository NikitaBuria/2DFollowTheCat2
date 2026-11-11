using UnityEngine;

// A named place where the player appears after coming through a SceneDoor (e.g. "FromTown").
// Only marks a position; SceneArrival does the moving.
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId;

    public string id => spawnId;

    // Returns null when this scene has no spawn point with that ID.
    public static SpawnPoint Find(string spawnId)
    {
        foreach (SpawnPoint point in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            if (point.spawnId == spawnId) return point;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
