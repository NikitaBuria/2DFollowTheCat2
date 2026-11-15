# Leather Armor handoff for Claude

The existing LeatherArmor item already references its inventory icon and seven wearable sprite sheets in this folder. Each sheet has 18 poses. The item uses 126 front-layer slots in Idle, Walk, Run, Dash, Attack, Hurt, Death order. Left mirrors Right.

## Where to look

- Item: `Assets/Scripts/ScriptableObjects/Items/LeatherArmor.asset`
- Player: `Assets/Prefabs/Core.prefab` → Player → EquipmentLayers → ArmorLayer
- Renderer binding: `Assets/Scripts/Player Scripts/EquipmentLayerView.cs` reads the equipped Armor item, looks up the current body sprite in bodyFrames and applies the matching overlay in LateUpdate.
- Offline preview: `GeneratedAssets/LeatherArmorAnimations.gif`

## Remaining Play Mode check

Equip Leather Armor from the inventory and confirm the chestnut jerkin appears on the player. Turn in all four directions, walk, run, dash and attack. Check Hurt and Death, especially the lowered torso in the last Death poses. Equip the club or lantern alongside the armor and verify the gripping hand remains visible. Unequip the armor and confirm the overlay clears. If a pose slips, record the state, direction and body sprite name so the matching artwork can be corrected.

The item references and offline composites have been checked. In-game appearance is pending.
