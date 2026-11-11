using UnityEngine;

// A door, stairs or cave exit that leads to another scene. Saves first (health, coins, inventory),
// then loads the target scene, whose SaveManager loads the save again and whose SceneArrival
// puts the player at targetSpawnId. Every door use is therefore also an autosave.
// PlayerInteractor finds interactables through their collider.
[RequireComponent(typeof(Collider2D))]
public class SceneDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private SaveManager saveManager;
    [Tooltip("Scene name as listed in Build Profiles > Scene List.")]
    [SerializeField] private string targetScene;
    [Tooltip("ID of the SpawnPoint in the target scene.")]
    [SerializeField] private string targetSpawnId;
    [SerializeField] private string enterPrompt = "Enter";
    [SerializeField] private string blockedPrompt = "Blocked";

    // False when the scene is missing from the build list: LoadScene would fail.
    public bool CanInteract(Player player) => Application.CanStreamedLevelBeLoaded(targetScene);

    public string GetPrompt(Player player) => CanInteract(player) ? enterPrompt : blockedPrompt;

    public void Interact(Player player)
    {
        // Without a save the new scene would load older data and the progress would be lost.
        if (!saveManager.Save())
        {
            Debug.LogWarning($"{name}: save failed, staying in this scene.", this);
            return;
        }

        SceneLoader.Load(targetScene, targetSpawnId);
    }
}
