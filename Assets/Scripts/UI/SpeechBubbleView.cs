using TMPro;
using UnityEngine;

// On a speech bubble under an NPC (CatSpeechBubble in the GuideCat, the Merchant's own bubble).
// Listens to NpcDialogue and shows the current line. Hidden with the CanvasGroup, not SetActive,
// so it stays enabled and keeps listening. While a menu is open UIManager says the world UI is
// hidden; the line comes back when the menu closes if the NPC is still talking.
[RequireComponent(typeof(CanvasGroup))]
public class SpeechBubbleView : MonoBehaviour
{
    [SerializeField] private NpcDialogue dialogue;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    private void OnEnable()
    {
        dialogue.OnLineShown += Show;
        dialogue.OnDialogueEnded += Hide;
        UIManager.OnWorldUIVisibilityChanged += HandleWorldUIVisibilityChanged;
    }

    private void OnDisable()
    {
        dialogue.OnLineShown -= Show;
        dialogue.OnDialogueEnded -= Hide;
        UIManager.OnWorldUIVisibilityChanged -= HandleWorldUIVisibilityChanged;
    }

    private void Show(string speaker, string line)
    {
        speakerText.SetText(speaker);
        dialogueText.SetText(line);
        RefreshVisibility();
    }

    private void HandleWorldUIVisibilityChanged(bool isVisible)
    {
        RefreshVisibility();
    }

    // A line is drawn only while the NPC is talking and no menu covers the world.
    private void RefreshVisibility()
    {
        canvasGroup.alpha = dialogue.isTalking && UIManager.isWorldUIVisible ? 1f : 0f;
    }

    private void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}
