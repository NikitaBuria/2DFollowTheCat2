using UnityEngine;

// An item that is worn in one Equipment slot instead of being used up.
// Keep maxStack at 1: Equipment only equips single items.
public abstract class EquipmentData : ItemData
{
    public abstract EquipmentSlotType slotType { get; }

    // How the item looks on the player: one sprite per body frame, in the same order as
    // EquipmentLayerView.bodyFrames (empty = invisible on the player, e.g. until the art exists).
    [field: SerializeField] public Sprite[] layerFrames { get; private set; }

    // The parts the body hides (e.g. a club head behind the leg), drawn behind the body.
    // Same order as layerFrames; empty = nothing behind.
    [field: SerializeField] public Sprite[] layerFramesBack { get; private set; }

    // Played when the player puts it on. Optional.
    [field: SerializeField] public SoundData equipSound { get; private set; }

    [Header("Light (optional)")]
    // Radius of the light around the player while it is held/worn (0 = gives no light). The Lantern.
    [field: SerializeField, Min(0f)] public float lightRadius { get; private set; }
    [field: SerializeField] public Color lightColor { get; private set; } = new Color(1f, 0.78f, 0.45f);
    [field: SerializeField, Min(0f)] public float lightIntensity { get; private set; } = 1f;

    public bool givesLight => lightRadius > 0f;
}
