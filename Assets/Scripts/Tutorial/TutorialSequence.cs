using UnityEngine;

// The whole tutorial: the GuideCat's steps in order. The saved progress is an index into this list,
// so only add steps at the end once players have saves.
[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Scriptable Objects/Tutorial/Tutorial Sequence")]
public class TutorialSequence : ScriptableObject
{
    [SerializeField] private GuideStep[] steps;

    public int count => steps.Length;

    // Null past the last step (tutorial finished).
    public GuideStep GetStep(int index) => index >= 0 && index < steps.Length ? steps[index] : null;
}
