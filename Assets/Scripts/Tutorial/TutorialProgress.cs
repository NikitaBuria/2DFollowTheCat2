using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Which GuideCat step the player is on (on SaveSystem in Core, saved like TownProgress).
// Only the current step can be completed; completing it moves to the next and fires OnStepChanged.
// TutorialTriggers and GuideCat report events here; this script never listens to gameplay itself.
public class TutorialProgress : MonoBehaviour, ISaveable
{
    // Saves before version 4 come from players who started before the tutorial existed.
    public const int FirstVersionWithTutorial = 4;

    [SerializeField] private TutorialSequence sequence;

    private int stepIndex;

    public GuideStep currentStep => sequence.GetStep(stepIndex);
    public bool isFinished => stepIndex >= sequence.count;

    public event Action<GuideStep> OnStepChanged;

    // No save yet (new game): the arrival scene may already finish a SceneEntered step.
    private void Start()
    {
        CompleteSceneEntered();
    }

    // Completes the current step if it waits for this kind of event (and this item, for item steps).
    public void TryComplete(GuideStepCompletion completion, ItemData item = null)
    {
        GuideStep step = currentStep;
        if (step == null || step.completion != completion) return;
        if (step.item != null && step.item != item) return;

        stepIndex++;
        OnStepChanged?.Invoke(currentStep);

        // Two scene steps in a row: arriving finishes the next one too.
        CompleteSceneEntered();
    }

    private void CompleteSceneEntered()
    {
        GuideStep step = currentStep;
        if (step != null && step.completion == GuideStepCompletion.SceneEntered && step.targetScene == SceneManager.GetActiveScene().name)
            TryComplete(GuideStepCompletion.SceneEntered);
    }

    public void Save(SaveData data) => data.tutorialStep = stepIndex;

    public string Validate(SaveData data)
    {
        if (data.version < FirstVersionWithTutorial) return null;
        return data.tutorialStep < 0 || data.tutorialStep > sequence.count ? $"tutorial step {data.tutorialStep} is outside 0-{sequence.count}" : null;
    }

    public void Load(SaveData data)
    {
        stepIndex = data.version < FirstVersionWithTutorial ? sequence.count : data.tutorialStep;
        OnStepChanged?.Invoke(currentStep);
        CompleteSceneEntered();
    }
}
