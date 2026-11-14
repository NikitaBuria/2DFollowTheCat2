using UnityEngine;

// One step of the GuideCat's tutorial, as data: where it happens, where the cat goes, what he says
// and what finishes it. TutorialSequence orders the steps; TutorialProgress remembers the current one.
[CreateAssetMenu(fileName = "NewGuideStep", menuName = "Scriptable Objects/Tutorial/Guide Step")]
public class GuideStep : ScriptableObject
{
    [Header("Where")]
    // The scene the cat shows up in for this step (scene name, as in the build list).
    [field: SerializeField] public string sceneName { get; private set; }
    // GuideMarker IDs the cat walks through, in order; he appears at the first one when the scene loads.
    [field: SerializeField] public string[] catPath { get; private set; }
    // On: the cat only walks while the player keeps up (he leads). Off: he just walks to his spot.
    [field: SerializeField] public bool leadsPlayer { get; private set; }
    // On: after appearing he follows the player around (Dungeon) instead of walking a path.
    [field: SerializeField] public bool followsPlayer { get; private set; }

    [Header("What he says")]
    [field: SerializeField, TextArea(2, 4)] public string[] lines { get; private set; }

    [Header("What finishes it")]
    [field: SerializeField] public GuideStepCompletion completion { get; private set; }
    // For ItemPickedUp / ItemEquipped.
    [field: SerializeField] public ItemData item { get; private set; }
    // For SceneEntered.
    [field: SerializeField] public string targetScene { get; private set; }

    public string id => name;
}
