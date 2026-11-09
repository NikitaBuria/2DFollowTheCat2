# Equipment slots

Drag EquipmentLayout.prefab under InventoryPanel > EquipmentArea. Its root stretches to the existing area. Helmet is above, Boots below, Weapon left and Armor right, leaving the center free for a character portrait.

Alternatively place ArmorSlot, WeaponSlot, HelmetSlot and BootsSlot individually. These are true variants of ../EquipmentSlot.prefab. Change common appearance on that base; change category label/sprite on the variant. The older standalone slots in the parent UI folder are preserved for existing references; use the variants in this folder for new work.

Each slot has the frame Image and Button on the root, plus Label, Placeholder and Icon children. Placeholder starts visible; Icon starts inactive. When occupied, hide Placeholder and assign/show Icon. During dragging, move a temporary drag image, leaving the slot in place. Commit data only after a valid drop. Interaction and equipment scripts are intentionally not supplied.

The layout fits the current 326.4 x 304.4 EquipmentArea at the panel's 960-unit width. Weapon/Armor roots are 112 x 128; Helmet/Boots are 112 x 96. The frame preserves its aspect ratio. Keep the panel's current reference dimensions and scale it through its parent Canvas.

All slots share the frame sprite and static Cinzel label font/material. Placeholder textures import at a maximum of 128 pixels with mipmaps and CPU readability disabled. Only the root background receives raycasts; no extra Canvases, layout rebuild components or runtime scripts are attached. This is a lightweight setup, not a measured performance benchmark.

Validated: prefab variant types, default visual states, required sprites, one raycast target per slot, area bounds, and a rendered Unity preview.
