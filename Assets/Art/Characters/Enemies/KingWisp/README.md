# King Wisp

Large crowned relative of WispGrin: smoky purple body, green eyes and grin, aged gold crown with green gem, floating claws and a ragged spectral royal mantle.

KingWispVisual.prefab is the visual layer only: SpriteRenderer plus Animator with a four-frame looping Idle at 4 fps. Its sprite canvas spans 3.2 world units, with transparent padding; the visible boss is intended to be roughly three ordinary wisps tall. Keep its transform scale at one and tune the size after viewing it beside the player. The bottom-biased pivot supports ground placement and sorting.

One shared sprite sheet, capped at 512 x 512 at runtime, point filtering, high-quality compression, no mipmaps, no CPU-readable copy. No real-time lights, particle systems, or per-frame gameplay scripts were added.

This is not a complete playable boss: no AI, health, attacks, hurt/death clips, collision, loot or encounter triggers are wired. Add this visual under your future boss root; keep decisions in states, movement/combat work in components and tunable stats in ScriptableObject data.

Generated with the built-in image tool. Prompt brief: use WispGrin as the species reference and Lost Wanderer as the rendering-style reference; a giant purple king wisp with green eyes, sharp grin, aged gold five-point crown, green gem, floating spectral claws and torn plum mantle; crisp muted fantasy pixel art, top-down three-quarter view; transparent 2x2 sheet of four subtle down-facing idle poses, consistent scale and padding, no text/background.

Tools > Enemies > Build King Wisp Visual creates missing assets and preserves existing prefabs. Validation.txt is written after all four animation sprite keys are sampled successfully. Frame pacing and appearance still require review in the game camera.
