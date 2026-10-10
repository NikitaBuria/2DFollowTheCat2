TOP-DOWN RUINS KIT

1. Drag Setup/TopDownMap.prefab into the Scene. Paint Ground and Water with the Ground128 and Water128 palettes.
2. Drag complete object prefabs into WorldObjects. Each object is one sprite at 128 pixels per unit.
3. The project's URP 2D renderer sorts Default-layer actors and objects along world Y. Keep the player's sorting order at 0 and its sort pivot at its feet.
Trees, rocks, stumps, the boat, tower, and chapel have small static ground-footprint Collider2Ds. Portal and arch colliders leave the central opening clear. Bushes can be walked through.
Bridge, dais, and dock sprites draw beneath actors as walkable surfaces. Ground/Water template layers intentionally have no collision; author water boundaries or obstacle collision separately.
This kit configures map artwork, sorting, and obstacle footprints. The existing horizontal/jump player controller has not been converted to top-down movement.
