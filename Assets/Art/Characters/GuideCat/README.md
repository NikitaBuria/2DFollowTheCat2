# Memorial guide cat

Reference: the supplied photographs; natural brown/charcoal tabby markings, copper flank, pale muzzle, green eyes, fluffy tail. Generated with the built-in image tool.

Generation brief: detailed pixel-art top-down fantasy companion matching Lost Wanderer; transparent 6-column, 9-row sheet; six-frame Talk, Walk and Run cycles, each facing Down, Up and Right. Preserve the photographed cat's colours and features, consistent scale, full ears and tail, no props or text. The requested 1536-square image was returned at 1254 square; the importer uses actual dimensions.

## Import

`GuideCatAssetBuilder` is Editor-only tooling. On its first script reload outside Play Mode it builds the assets automatically. Alternatively choose **Tools > Guide Cat > Build Animation Assets**. Existing controllers are preserved.

The builder creates 54 sprites, 16 animation clips (including a static Idle), four directional blend trees and a visual prefab. Left uses the right-facing frames mirrored, so asymmetric fur markings are mirrored too. `MoveX` and `MoveY` choose direction; states are named Idle, Talk, Walk and Run. There are no automatic state transitions: the future cat state machine decides when to play each state.

Settings: 256 pixels per unit for the cat (player: 128), point filtering, no mipmaps, no CPU-readable texture copy, high-quality compression, one shared texture. This halves her original width and height uniformly across all clips; keep the prefab transform at scale 1. Her seated silhouette is approximately 40% of the player's visible height. Actual platform memory/appearance must be checked in Unity. Talk is a silent mouth/ear animation, not recorded voice or lip synchronisation.

The prefab contains only SpriteRenderer and Animator. No input, movement, tutorial, protection, collision or gameplay scripts have been added. Existing scenes are untouched.

## Verification

The builder checks sprite count and samples every clip's sprite keys. A successful run writes Validation.txt. Visual review is still necessary: generated frames may need alignment or gait refinement, and the current grid must be checked for edge clipping. Do not consider this production-verified until reviewed at game scale in Unity.
