using System.Collections.Generic;
using UnityEngine;

// Draws one equipment slot on top of the player's body (paper doll). The body's Animator picks
// the frame; every LateUpdate this layer shows the equipped item's sprite for that same frame,
// so equipment needs no animation clips of its own. Only listens to Equipment, like a UI view.
[RequireComponent(typeof(SpriteRenderer))]
public class EquipmentLayerView : MonoBehaviour
{
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Equipment equipment;
    [SerializeField] private SpriteRenderer bodyRenderer;

    // Optional: what this layer shows while the slot is empty (the Weapon layer uses Fists for
    // the bare forearm and hand). None = draw nothing when empty.
    [SerializeField] private EquipmentData emptySlotLook;

    // On the layer behind the body: draws the item's layerFramesBack instead of layerFrames.
    [SerializeField] private bool drawsBackFrames;

    // Every body sprite in a fixed order; an item's layerFrames use the same order.
    [SerializeField] private Sprite[] bodyFrames;

    private readonly Dictionary<Sprite, int> frameIndex = new();
    private SpriteRenderer layerRenderer;
    private Sprite[] currentFrames;

    private void Awake()
    {
        layerRenderer = GetComponent<SpriteRenderer>();

        for (int i = 0; i < bodyFrames.Length; i++)
        {
            if (bodyFrames[i] != null)
                frameIndex[bodyFrames[i]] = i;
        }
    }

    private void OnEnable()
    {
        equipment.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void OnDisable()
    {
        equipment.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    // Start, not OnEnable: Equipment's Awake (starting weapon) has run by now.
    private void Start()
    {
        Show(equipment.GetEquipped(slotType));
    }

    private void HandleEquipmentChanged(EquipmentSlotType changedSlot, EquipmentData item)
    {
        if (changedSlot == slotType)
            Show(item);
    }

    private void Show(EquipmentData item)
    {
        EquipmentData look = item != null ? item : emptySlotLook;
        if (look == null)
            currentFrames = null;
        else
            currentFrames = drawsBackFrames ? look.layerFramesBack : look.layerFrames;
    }

    // LateUpdate: after the Animator has set this frame's body sprite.
    private void LateUpdate()
    {
        Sprite sprite = null;

        if (currentFrames != null && bodyRenderer.sprite != null &&
            frameIndex.TryGetValue(bodyRenderer.sprite, out int index) && index < currentFrames.Length)
        {
            sprite = currentFrames[index];
        }

        layerRenderer.sprite = sprite;
        layerRenderer.flipX = bodyRenderer.flipX; // Left clips mirror Right
    }
}
