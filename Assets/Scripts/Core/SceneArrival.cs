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

    // The location is optional: a bad one only means Continue / F9 can't use it (see HasValidLocation).
    public string Validate(SaveData data) => null;

    // True when the save has a scene that is in the build and a real position. Older saves (no location),
    // or a scene removed in a newer build, return false: Continue picks the scene, F9 loads in place.
    public static bool HasValidLocation(SaveData data) =>
        data.version >= FirstVersionWithLocation
        && !string.IsNullOrEmpty(data.sceneName)
        && Application.CanStreamedLevelBeLoaded(data.sceneName)
        && float.IsFinite(data.position.x) && float.IsFinite(data.position.y);

    public void Load(SaveData data) { }
}
