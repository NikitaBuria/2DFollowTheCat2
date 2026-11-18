using UnityEngine;
using UnityEngine.SceneManagement;

// Changes scenes and remembers where the player should appear in the new one: at a SpawnPoint
// (doors) or at an exact position (Continue and F9, from the save file).
// Static, so this survives the scene load (scene objects are destroyed, statics are not).
// Player data does not travel here: the new scene's SaveManager loads it in Start.
public static class SceneLoader
{
    private static string pendingSpawnId;
    private static Vector2? pendingPosition;

    public static void Load(string sceneName, string spawnId)
    {
        pendingSpawnId = spawnId;
        pendingPosition = null;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, Vector2 position)
    {
        pendingSpawnId = null;
        pendingPosition = position;
        SceneManager.LoadScene(sceneName);
    }

    // Called once by SceneArrival in the new scene; null when the scene was not entered through a door.
    public static string TakePendingSpawnId()
    {
        string spawnId = pendingSpawnId;
        pendingSpawnId = null;
        return spawnId;
    }

    // Called once by SceneArrival in the new scene; null when no exact position was asked for.
    public static Vector2? TakePendingPosition()
    {
        Vector2? position = pendingPosition;
        pendingPosition = null;
        return position;
    }
}
