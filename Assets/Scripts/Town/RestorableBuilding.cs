using System;
using UnityEngine;

// A ruined building in town. Press E to pay and restore it; then it saves so the purchase sticks.
// Only shows the Ruined or Restored child (Astra's art goes there); TownProgress owns the data.
// Announces the restore with OnRestored; BuildingRebuildView plays the effect.
// PlayerInteractor finds interactables through their collider.
[RequireComponent(typeof(Collider2D))]
public class RestorableBuilding : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingData building;
    [SerializeField] private TownProgress townProgress;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameObject ruinedVisual;
    [SerializeField] private GameObject restoredVisual;
    [SerializeField] private string restoredPrompt = "Restored";

    // Fired once, when the player restores it (not when a save is loaded).
    public event Action OnRestored;

    private void OnEnable()
    {
        townProgress.OnProgressChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        townProgress.OnProgressChanged -= Refresh;
    }

    public bool CanInteract(Player player) => townProgress.GetRestoreBlock(building, player) == RestoreBlock.None;

    // Refused prompts are drawn red by InteractionPromptView; NotEnoughCoins still shows the cost.
    public string GetPrompt(Player player) => townProgress.GetRestoreBlock(building, player) switch
    {
        RestoreBlock.AlreadyRestored => restoredPrompt,
        RestoreBlock.MissingItem => $"Needs {building.requiredItem.displayName}",
        _ => $"Restore {building.buildingName} ({building.cost} coins)"
    };

    public void Interact(Player player)
    {
        if (!townProgress.TryRestore(building, player)) return;

        saveManager.Save();
        OnRestored?.Invoke();
    }

    private void Refresh()
    {
        bool restored = townProgress.IsRestored(building);
        ruinedVisual.SetActive(!restored);
        restoredVisual.SetActive(restored);
    }
}
