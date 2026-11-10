# Speech bubbles

Two world-space visual prefabs: CatSpeechBubble (sage green) and MerchantSpeechBubble (warm gold). Both have a dark panel, speech tail, editable speaker label and editable dialogue text. The labels are GUIDE and MERCHANT until character names are chosen.

Parent the matching prefab under the character. Keep its local scale at 0.005 on all axes. Start with local position (0, 0.3, 0) for the cat and (0, 1, 0) for a player-sized merchant, then adjust to clear the character's head. The panel is 1.8 world units wide; its tail extends below it. The current root uses bottom pivot with centre-anchored artwork, so its tail begins approximately 0.4 units above the root. These prefabs are intended for the current fixed-facing 2D camera, not a rotating 3D camera.

Edit Border/SpeakerName and Border/DialogueText in the Inspector. Default lines are previews, not implemented story events. The text area fits short two-line messages. Longer text uses ellipsis; divide longer conversations into short pages in your future dialogue system. Hide the prefab when nobody is speaking.

No runtime scripts, input polling, dialogue sequencing, typewriter effect, voice, shop actions or character behaviour are included. The future dialogue system should publish changes and the UI should listen, following the project's event rules. Speech does not automatically trigger the cat's Talk animation yet.

Both variants share the existing LiberationSans SDF font and use texture-free UI shapes. No GraphicRaycaster or EventSystem is needed because the bubbles are non-interactive. All graphics disable raycast targeting. Each world-space bubble has its own Canvas; only enable visible speakers.

Validation: Unity built both prefabs and verified that their default text fits. In-game placement and readability at the final camera zoom still need visual review. Tools > Dialogue > Build Speech Bubbles creates missing prefabs and preserves existing edits.
