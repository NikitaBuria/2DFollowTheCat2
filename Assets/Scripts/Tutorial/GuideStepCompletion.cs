// What finishes a guide step. TutorialTriggers turns game events into these; GuideCat reports
// DialogueEnded itself.
public enum GuideStepCompletion
{
    DialogueEnded, // the cat said all his lines
    ItemPickedUp,  // the step's item went into the inventory
    ItemEquipped,  // the player put the step's item on
    ShopOpened,    // the merchant's shop opened
    SceneEntered,  // the player arrived in the step's target scene
    EnemyKilled,   // any enemy died
}
