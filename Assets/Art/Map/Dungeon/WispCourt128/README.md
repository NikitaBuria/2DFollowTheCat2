# Wisp Court — ReferenceRuins128 scale

Select **Wisp Court** in **Window > 2D > Tile Palette**. The editable scene target is **Wisp Court / Court Artwork**.

| Setting | Wisp Court | ReferenceRuins128 |
| --- | --- | --- |
| Sprite rectangle | 128 x 128 pixels | 128 x 128 pixels |
| Pixels per unit | 128 | 128 |
| World size per cell | 1 x 1 | 1 x 1 |
| Grid cell size | 1, 1, 1 | 1, 1, 1 |
| Pivot | Center | Center |
| Tile transform | Identity | Identity |

The original 96 four-unit stamps were subdivided into 1,536 one-unit tiles. The assembled court remains 48 x 32 world units. The source PNGs and original stamp Tile assets are preserved. The six new 2048-pixel sheets are in `Textures`; Sprite Editor shows their individual 128-pixel rectangles. Source pixels were enlarged with nearest-neighbour sampling, retaining the existing art and its detail level.

Choose one palette cell to paint a one-unit piece, or box-select several cells to stamp a larger section. An original four-unit stamp now occupies a 4 x 4 selection. These pieces reproduce the court artwork; they are not terrain RuleTiles and do not invent connecting walls automatically. **Dungeon Limestone128** supplies the separate manually selected floor, wall and prop kit at the same cell size.

The scene Wisp Court instance was unpacked so Unity permits painting on it; the palette asset remains reusable. The Dungeon scene was saved, retaining the existing instance position. The separate ReferenceRuins scene Grid and the half-unit CollisionGrid were not modified by this conversion. The two visual Grid objects retain their existing world origins; use Court Artwork as the target when editing the court.

Verified through the running Editor: Sprite Editor edit capabilities, 1,536 sprite and Tile bindings, full-rectangle one-unit bounds, palette dimensions, adjacent tile placement, painting and erasing in an isolated preview scene, plus removal of the live scene prefab painting restriction. Gameplay and collision alignment were not tested.

2D Animation 13.0.6 is installed. Static tile grid sizing uses Sprite Editor import data and Tilemap Editor; skeletal animation is not needed for these tiles.

The one-time editor conversion is in `Assets/Editor/WispCourtGridRepair.cs`. It stops when the verification markers exist and does not run a gameplay component.
