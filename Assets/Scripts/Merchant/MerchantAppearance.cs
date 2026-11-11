using UnityEngine;

// Shows the merchant's look for the current town tier (one child Animator per tier) and plays
// Talk while his NpcDialogue shows a line. Like the player and enemies, the Animators have no
// transitions; this script picks the state with Play. Follows TownProgress.townTier when one is assigned.
public class MerchantAppearance : MonoBehaviour
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int TalkHash = Animator.StringToHash("Talk");

    [SerializeField, Range(1, 3)] private int townTier = 1;
    [SerializeField] private Animator[] tiers;
    [SerializeField] private NpcDialogue dialogue;
    [Tooltip("Optional: when set, the look follows the town tier instead of the field above.")]
    [SerializeField] private TownProgress townProgress;

    private Animator currentAnimator => tiers.Length > 0 ? tiers[townTier - 1] : null;

    private void OnEnable()
    {
        dialogue.OnLineShown += HandleLineShown;
        dialogue.OnDialogueEnded += HandleDialogueEnded;
        if (townProgress != null) townProgress.OnProgressChanged += HandleProgressChanged;

        SetTownTier(townProgress != null ? townProgress.townTier : townTier);
    }

    private void OnDisable()
    {
        dialogue.OnLineShown -= HandleLineShown;
        dialogue.OnDialogueEnded -= HandleDialogueEnded;
        if (townProgress != null) townProgress.OnProgressChanged -= HandleProgressChanged;
    }

    public void SetTownTier(int tier)
    {
        townTier = Mathf.Clamp(tier, 1, Mathf.Max(1, tiers.Length));
        for (int i = 0; i < tiers.Length; i++)
            tiers[i].gameObject.SetActive(i == townTier - 1);

        PlayCurrent();
    }

    private void HandleProgressChanged() => SetTownTier(townProgress.townTier);

    private void HandleLineShown(string speaker, string line) => PlayCurrent();

    private void HandleDialogueEnded() => PlayCurrent();

    private void PlayCurrent()
    {
        if (currentAnimator == null || !currentAnimator.isActiveAndEnabled) return;

        currentAnimator.Play(dialogue.isTalking ? TalkHash : IdleHash);
    }
}
