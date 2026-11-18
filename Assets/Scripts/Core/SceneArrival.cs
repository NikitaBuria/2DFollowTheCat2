using UnityEngine;
using UnityEngine.SceneManagement;

// On SaveSystem. Owns where the player is:
// - after a scene change, moves the player to the SpawnPoint the door asked for, or to the saved
//   position (Continue, F9);
// - as an ISaveable, writes the current scene and position into the save.
// Load does nothing: the position is applied by Start after SceneLoader has loaded the saved scene.
public class SceneArrival : MonoBehaviour, ISaveable
{
    public const int FirstVersionWithLocation = 5;

    [SerializeField] private Player player;

    // Start: Player.Awake has found its PlayerMotor by now.
    private void Start()
    {
        string spawnId = SceneLoader.TakePendingSpawnId();
        Vector2? position = SceneLoader.TakePendingPosition();

        if (position.HasValue)
        {
            player.playerMotor.Teleport(position.Value);
            return;
        }

        if (spawnId == null) return; // pressed Play in this scene, no door used

        SpawnPoint point = SpawnPoint.Find(spawnId);
        if (point == null)
        {
            Debug.LogWarning($"No SpawnPoint '{spawnId}' in this scene; the player stays where they are.", this);
            return;
        }

        player.playerMotor.Teleport(point.transform.position);
    }

    public void Save(SaveData data)
    {
        data.sceneName = SceneManager.GetActiveScene().name;
        data.position = player.transform.position;
    }

    public string Validate(SaveData data)
    {
        if (data.version < FirstVersionWithLocation) return null; // older saves: no location, Continue picks the scene
        if (string.IsNullOrEmpty(data.sceneName)) return "no scene saved";
        if (!Application.CanStreamedLevelBeLoaded(data.sceneName)) return $"scene '{data.sceneName}' is not in the build";
        if (!float.IsFinite(data.position.x) || !float.IsFinite(data.position.y)) return "invalid position";
        return null;
    }

    public void Load(SaveData data) { }
}
