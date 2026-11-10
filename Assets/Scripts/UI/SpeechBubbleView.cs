using TMPro;
using UnityEngine;

// On a speech bubble prefab (CatSpeechBubble, MerchantSpeechBubble) under an NPC.
// Listens to NpcDialogue and shows the current line. Hidden with the CanvasGroup, not SetActive,
// so it stays enabled and keeps listening.
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
    }

    private void OnDisable()
    {
        dialogue.OnLineShown -= Show;
        dialogue.OnDialogueEnded -= Hide;
    }

    private void Show(string speaker, string line)
    {
        speakerText.SetText(speaker);
        dialogueText.SetText(line);
        canvasGroup.alpha = 1f;
    }

    private void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}
