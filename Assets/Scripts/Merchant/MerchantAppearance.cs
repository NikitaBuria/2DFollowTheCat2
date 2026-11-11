using UnityEngine;

// Shows the merchant's look for the current town tier (one child Animator per tier) and plays
// Talk while his NpcDialogue shows a line. Like the player and enemies, the Animators have no
// transitions; this script picks the state with Play. Later town restoration calls SetTownTier.
public class MerchantAppearance : MonoBehaviour
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int TalkHash = Animator.StringToHash("Talk");

    [SerializeField, Range(1, 3)] private int townTier = 1;
    [SerializeField] private Animator[] tiers;
    [SerializeField] private NpcDialogue dialogue;

    private Animator currentAnimator => tiers.Length > 0 ? tiers[townTier - 1] : null;

    private void OnEnable()
    {
        dialogue.OnLineShown += HandleLineShown;
        dialogue.OnDialogueEnded += HandleDialogueEnded;
        SetTownTier(townTier);
    }

    private void OnDisable()
    {
        dialogue.OnLineShown -= HandleLineShown;
        dialogue.OnDialogueEnded -= HandleDialogueEnded;
    }

    public void SetTownTier(int tier)
    {
        townTier = Mathf.Clamp(tier, 1, Mathf.Max(1, tiers.Length));
        for (int i = 0; i < tiers.Length; i++)
            tiers[i].gameObject.SetActive(i == townTier - 1);

        PlayCurrent();
    }

    private void HandleLineShown(string speaker, string line) => PlayCurrent();

    private void HandleDialogueEnded() => PlayCurrent();

    private void PlayCurrent()
    {
        if (currentAnimator == null || !currentAnimator.isActiveAndEnabled) return;

        currentAnimator.Play(dialogue.isTalking ? TalkHash : IdleHash);
    }
}
