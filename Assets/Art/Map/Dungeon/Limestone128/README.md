# Dungeon Limestone128

Open **Window > 2D > Tile Palette** and select **Dungeon Limestone128**.

- The palette has 64 separate Tile assets, each 128 x 128 pixels at 128 pixels per unit.
- Use a rectangular scene Grid with cell size **(1, 1, 1)** and unscaled Tilemaps.
- Top four palette rows: clean, cracked, shadowed and corrupted floor variants.
- Bottom four rows: walls, corners, junctions, arches, stairs, rubble and props.
- Paint floors onto a ground Tilemap and structures/props onto separate Tilemaps above it.
- These are manually selected tiles. They do not automatically choose neighbours or join patterns.
- Wisp Court / Court Artwork now uses the same one-unit cell size after its Sprite Editor conversion. CaveWalls remains a separate half-unit collision grid. Paint visuals on a one-unit Tilemap, not CaveWalls.

`Limestone128.png` is the normalized, grid-sliced sheet. The original 1254-pixel PNG remains unchanged. Each source cell was independently fitted to a 128-pixel cell. This preserves source artwork, including its existing edge marks, small debris and imperfect joins; slicing does not repair those art defects. No claim of seamless connections between arbitrary floor variants is made.

Unity Editor verification checked all 64 sprite references, dimensions, pixels per unit, Tile assets and palette cells, plus the GridPalette sub-asset. The populated palette was observed in the Editor. Painting into the existing dungeon and gameplay collision have not been tested.

Rebuild using **Tools > Tiles > Build Dungeon Limestone Palette**. The editor utility updates the generated kit in place and preserves existing generated sprite and Tile identities. It does not edit gameplay scripts or scenes.
