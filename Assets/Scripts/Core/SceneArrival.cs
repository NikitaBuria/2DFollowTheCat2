using UnityEngine;

// On SaveSystem. After a scene change, moves the player to the SpawnPoint the door asked for.
// Position is not part of the save, so this and SaveManager's load don't overlap.
public class SceneArrival : MonoBehaviour
{
    [SerializeField] private Player player;

    // Start: Player.Awake has found its PlayerMotor by now.
    private void Start()
    {
        string spawnId = SceneLoader.TakePendingSpawnId();
        if (spawnId == null) return; // pressed Play in this scene, no door used

        SpawnPoint point = SpawnPoint.Find(spawnId);
        if (point == null)
        {
            Debug.LogWarning($"No SpawnPoint '{spawnId}' in this scene; the player stays where they are.", this);
            return;
        }

        player.playerMotor.Teleport(point.transform.position);
    }
}
