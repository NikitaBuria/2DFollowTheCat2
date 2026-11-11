using UnityEngine.SceneManagement;

// Changes scenes and remembers where the player should appear in the new one.
// Static, so the spawn ID survives the scene load (scene objects are destroyed, statics are not).
// Player data does not travel here: SceneDoor saves first, and the new scene's SaveManager loads in Start.
public static class SceneLoader
{
    private static string pendingSpawnId;

    public static void Load(string sceneName, string spawnId)
    {
        pendingSpawnId = spawnId;
        SceneManager.LoadScene(sceneName);
    }

    // Called once by SceneArrival in the new scene; null when the scene was not entered through a door.
    public static string TakePendingSpawnId()
    {
        string spawnId = pendingSpawnId;
        pendingSpawnId = null;
        return spawnId;
    }
}
