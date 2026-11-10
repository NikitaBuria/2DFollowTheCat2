using System;
using UnityEngine;

// The simplest interactable: an NPC (the guide cat, later the merchant) that says a few lines.
// Each interact shows the next line; after the last one the conversation ends.
// It only announces lines through events; SpeechBubbleView draws them.
public class NpcDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private string speakerName = "Guide";
    [SerializeField, TextArea(2, 3)] private string[] lines = { "Hello, wanderer." };
    [Tooltip("Seconds a line stays up if the player doesn't press interact again.")]
    [SerializeField, Min(0.5f)] private float lineDuration = 4f;
    [SerializeField] private string talkPrompt = "Talk";
    [SerializeField] private string nextPrompt = "Next";

    private int currentLine = -1;
    private float hideTimer;

    public bool isTalking => currentLine >= 0;

    public event Action<string, string> OnLineShown;  // speaker name, line
    public event Action OnDialogueEnded;

    private void Update()
    {
        if (!isTalking) return;

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f) EndDialogue();
    }

    public bool CanInteract(Player player) => lines.Length > 0;

    public string GetPrompt(Player player) => isTalking ? nextPrompt : talkPrompt;

    public void Interact(Player player)
    {
        currentLine++;
        if (currentLine >= lines.Length)
        {
            EndDialogue();
            return;
        }

        hideTimer = lineDuration;
        OnLineShown?.Invoke(speakerName, lines[currentLine]);
    }

    private void EndDialogue()
    {
        currentLine = -1;
        OnDialogueEnded?.Invoke();
    }
}
