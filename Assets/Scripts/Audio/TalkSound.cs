using UnityEngine;

// Plays a voice when an NPC says a line (the merchant's greeting). Listens to NpcDialogue.OnLineShown,
// so the dialogue code never knows about audio. NPCs have no data asset, so the SoundData is set here.
[RequireComponent(typeof(NpcDialogue))]
public class TalkSound : SoundPlayer
{
    [SerializeField] private SoundData talkSound;

    private NpcDialogue dialogue;

    protected override void Awake()
    {
        base.Awake();
        dialogue = GetComponent<NpcDialogue>();
    }

    private void OnEnable()
    {
        dialogue.OnLineShown += PlayTalk;
    }

    private void OnDisable()
    {
        dialogue.OnLineShown -= PlayTalk;
    }

    private void PlayTalk(string speaker, string line) => Play(talkSound);
}
