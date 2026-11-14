using UnityEngine;

// Turns game events into tutorial progress (on SaveSystem, next to TutorialProgress). Listens only;
// TutorialProgress decides whether the event finishes the current step.
// SceneEntered is checked by TutorialProgress itself, DialogueEnded by the GuideCat.
[RequireComponent(typeof(TutorialProgress))]
public class TutorialTriggers : MonoBehaviour
{
    // The player's components directly: Player fills its own properties only in its Awake, which may run after this OnEnable.
    [SerializeField] private Inventory inventory;
    [SerializeField] private Equipment equipment;
    [SerializeField] private GameStateManager gameStateManager;

    private TutorialProgress progress;

    private void Awake()
    {
        progress = GetComponent<TutorialProgress>();
    }

    private void OnEnable()
    {
        inventory.OnItemAdded += HandleItemAdded;
        equipment.OnItemEquipped += HandleItemEquipped;
        gameStateManager.OnGameStateChanged += HandleGameStateChanged;
        Enemy.OnAnyEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        inventory.OnItemAdded -= HandleItemAdded;
        equipment.OnItemEquipped -= HandleItemEquipped;
        gameStateManager.OnGameStateChanged -= HandleGameStateChanged;
        Enemy.OnAnyEnemyDied -= HandleEnemyDied;
    }

    private void HandleItemAdded(ItemData item, int amount) => progress.TryComplete(GuideStepCompletion.ItemPickedUp, item);

    private void HandleItemEquipped(EquipmentData item) => progress.TryComplete(GuideStepCompletion.ItemEquipped, item);

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Shop) progress.TryComplete(GuideStepCompletion.ShopOpened);
    }

    private void HandleEnemyDied(Enemy enemy) => progress.TryComplete(GuideStepCompletion.EnemyKilled);
}
