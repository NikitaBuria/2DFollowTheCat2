# Opening cave — art only

Open Assets/Scenes/OpeningCave_Art.unity for the staged opening, or use OpeningCave_Art.prefab as an environment assembly. The existing SampleScene and build order are unchanged.

Muted charcoal/plum stone matches the game's fantasy pixel art. The lower-left waking chamber leads through a winding passage to the upper-right exit. The opening camera frames the waking spot, cat and separate amber lantern, leaving the exit offscreen. The cat's static bubble says: “You're awake. Walk towards the lantern.”

PlayerWakePoint and ExitPoint are placement markers only. The existing wanderer sprite already holds a lantern, so its sizing reference is hidden to avoid showing two lanterns before pickup. A lantern-free waking pose is not included.

Optimization: one environment sprite, one 128-pixel lantern sprite, the existing cat and bubble assets. No extra real-time lights, particles, physics, audio or gameplay scripts. The background retains a 2048 maximum import size for close-up stone detail, with compression, point filtering, no mipmaps and no CPU-readable copy. This is a single-room backdrop rather than a modular tile set. Walls are painted art, with no collision or foreground occlusion layer.

The dark tint and small warm patch are static art direction, not a functioning light system. The warm patch is baked into Cave.png and will not move when the lantern moves. For later gameplay, use a neutral floor or remove that patch before implementing the equipped lantern's moving light. No pickup, inventory, equipment, dialogue sequencing or exit logic is included.

Generated with the built-in image tool using Lost Wanderer as the style reference. Cave brief: landscape orthographic three-quarter pixel-art cave, broad lower-left waking room, winding middle path, upper-right exit, dark slate/plum palette, amber focal patch near 40% width and 65% height, clear route, no characters or UI. Lantern brief: isolated aged-brass amber lantern matching the character's lantern, crisp pixel edges, transparent background; second pass removed background haze.

The Editor-only builder creates a separate additive scene, saves it, then restores the active scene. It never modifies gameplay scripts. Validation.txt confirms assembly, not a playtest.
