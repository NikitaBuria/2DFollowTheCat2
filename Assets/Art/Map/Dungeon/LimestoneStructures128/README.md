# Dungeon Structures128

Walls and stair rails share one connecting RuleTile. All sprite cells are 128 x 128 pixels at 128 PPU on a 1 x 1 x 1 Grid, matching Dungeon Limestone128.

Use **Window > 2D > Tile Palette** with:

| Control | Selection |
| --- | --- |
| Palette | Dungeon Structures128 |
| Brush | WallsAndStairsBrush |
| Painting target | Limestone Walls |

The brush and target are selected after setup. The scene's new layers are under **Wisp Court** (or a separate unit-sized structure Grid if Wisp Court is absent).

## Painting

- Pick the single post at the top-left of the palette to draw walls. It is the wall RuleTile: dragging makes straight runs, ends, corners, T junctions and crossings automatically. The nearby line and rectangle are examples made with that same brush.
- Below the wall examples are the structure stamps, arranged left to right: **north stairs (3 x 2 cells)**, **straight landing (3 x 3)**, **east stairs (2 x 3)**, **turning landing (3 x 3)**, and **gate arch (3 x 2)**. Box-select the entire stamp to paint it.
- North stairs can be continued with another north stair stamp or a landing. East stairs connect to the turning landing. Their sidewalls use the same RuleTile as ordinary walls, so adjoining walls and rail corners update when you paint or erase.
- The lower rows contain pillars, wall panels, broken masonry, rubble, flame and crystals. Structural pieces sit over a shared wall connection; decorative rubble and effects leave existing walls unchanged.
- Use Paint or Box Fill for multi-cell stamps. Flood Fill supports the single wall brush.

The editor brush writes stair surfaces to **Limestone Walls Floors**, the connecting wall network to **Limestone Walls**, and pillars/arches/props to **Limestone Walls Details**. Painting and erasing support Undo. Keep the custom brush selected for stairs and structures; the standard brush does not perform this layer routing.

## Validation and limits

The wall profiles were prepared from the existing Dungeon Limestone128 art. All 16 cardinal neighbour configurations were checked through Unity's RuleTile evaluation. The 64 horizontal and 64 vertical connected edge pairings match pixel-for-pixel. Tests also exercised the actual editor brush: stair-to-wall corners, consecutive stair sections, a turning landing into east stairs, multi-layer erasing and neighbour refresh. The custom brush editor was checked against the live scene target, and the selected palette, brush and target were observed in Unity.

Sprite Editor capability checks run before slicing. `SpriteAnalysis.txt` records 3 x 3 colour-grid analysis; the known connection directions define the exact wall rules, since detailed stone textures cannot reliably infer topology from colour alone.

Only editor tooling was added; finished maps use Unity's built-in RuleTile and Tile assets. The installed 2D Animation package does not determine static tile dimensions. Collision and gameplay were not changed or tested; the existing separate collision grid remains independent of these visual painting layers.

Source assets and the existing Dungeon Limestone128 palette are preserved. Editor tools are `LimestoneConnectionsBuilder`, `LimestoneStructureBrush` and its editor binding in `Assets/Editor/`. The builder runs once when its verification marker is absent.
