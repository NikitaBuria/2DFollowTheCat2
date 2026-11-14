# Equipment layers: art brief for Astra

The player now shows equipment as layers drawn over the body (paper doll).
The body's Animator picks the frame. Each equipment layer shows the matching frame from the
equipped item's sheet, so equipment needs **no animation clips**, only sprite sheets.

## What to draw (first batch)

| Sheet set | What it is |
|---|---|
| **Body (bare)** | The Lost Wanderer with **empty hands, no lantern**: edit the existing 7 PNGs **in place** (same files, same size, don't re-slice in Unity) and repair the fingers, sleeve and cloak the lantern covered. |
| **Lantern** | Only the lantern (and the hand/arm parts that cover it, if needed), recovered from the original frames where possible. Check: body + Lantern layer should look like the original art. |
| **Wooden Club** | Only the club. `Art/Items/Weapons/WoodenClub_HandFit.png` and `_GripOcclusion.png` are a good start. |
| **Leather Armor** | Only the leather piece worn over the cloak. |

Later: helmets and boots, same rules.

## Rules for every equipment sheet

1. **Same 7 sheets as the body**: Idle, Walk, Run, Dash, Attack, Hurt, Death.
2. **Same canvas and positions**: each equipment PNG is the same size as the body PNG
   (1536 x 768), with every frame drawn exactly over the body frame it belongs to.
   Easiest: draw on a layer above the body in your art tool and export that layer alone.
3. **Same frame order**: rows from the top Down, Up, Right; 6 frames left to right.
   Left is the Right row mirrored by the game, so don't draw Left.
4. **Only the item** on transparent background, never the body.
5. **Draw only what is visible.** The layers are always drawn on top of the body,
   in this order: Boots, Armor, Helmet, Weapon (top). When something should be *behind*
   the body (e.g. the lantern on the far side when facing Up), leave the hidden part out.
6. A frame where the item can't be seen may stay empty, and a held pose may reuse the same drawing.
7. **Stay inside the frame's 256 x 256 cell** (the sheet is a 6 x 3 grid of cells). The item does
   *not* have to fit the body's trimmed sprite rectangle; a club tip may stick out past the body.

## What Claude does when the sheets arrive

- Slices each equipment PNG per 256 x 256 cell, with each pivot set to the same canvas point as
  that frame's body pivot, so the item lines up with the body even though the body sprites are trimmed.
- Fills the item's `layerFrames` (126 sprites: 7 sheets x 18) in the same order.
- Checks the bare body sheets keep their existing slicing (sprite IDs), so the clips still work.

## Pilot (before the full batch)

Right Attack (6 frames, Wooden Club) and one Up Idle pose (bare body + Lantern). Claude hooks them
up so they can be reviewed in game at gameplay size, including the mirrored Left.

Until then the layers stay empty and the current art (with the lantern drawn in) shows.

## Walk/Run arm split (agreed after the bare-body review)

Empty hands looked like holding a "ghost lantern" in Walk/Run (fist parked at the hip). Fix, **Walk and Run only**
(they share art; Idle, Dash, Hurt, Death, Attack keep the arm on the body):

- **Body:** keeps the shoulder and as much upper sleeve as the swing allows; the forearm and hand are removed,
  with a sleeve fold as the seam.
- **Fists sheet** (`Fists_Walk`, `Fists_Run`): the bare forearm and hand, relaxed curl, swinging opposite the
  stepping leg. Shown on the Weapon layer whenever no weapon is equipped (`EquipmentLayerView.emptySlotLook`).
  Up row: only the part visible past the cloak.
- **Lantern Walk/Run:** add the current carrying forearm back in, so the approved equipped look stays identical.
- **Club Walk/Run** (when drawn): includes its own holding forearm; reuse the carrying sleeve where the grip fits.
- Clean the leftover orange lantern-glow pixels off the bare hand (all sheets); keep the warm colour on the Lantern layer.
- First review: the six Right Walk frames, at Walk and Run speed, empty-handed and with the Lantern.

## Weapon grip direction (all weapon types)
Use the dwarf reference for every held weapon. Seat the handle through the palm with the thumb opposing the curled fingers. Back carries parts obscured by the body and cloak. Front carries exposed weapon parts and the gripping forearm and fingers. Export Weapon_State_Back.png and Weapon_State_Front.png on matching canvases. Carry angles depend on the weapon. Preserve the approved Lantern appearance.
