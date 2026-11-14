# King Wisp visuals

KingWispVisual uses a 3.2-unit sprite canvas, a bottom-biased pivot and a transition-free Animator. The MoveX/MoveY trees share frontal artwork in all four slots. Idle loops. Attack, Spawn, Hurt and Dead are one-shots selected by the boss state code.

| State | Duration | Visual cue |
| --- | --- | --- |
| Attack | 0.222 s | The widest claw slash lands at 0.111 s. |
| Spawn | 0.400 s | Three green wisps appear around the crown at 0.200 s. |
| Hurt | 0.75 s | Impact flash, held recoil, then recovery. |
| Dead | 1.90 s | Explosion at 0.30 s, shards at 0.90 s, sparks clear at 1.80 s. |

WispBossData references KingWispLoot, configured to drop one MarbleStonePickup. The Apothecary requires Marble Stone in addition to its gold cost. The boss encounter owns damage, minion creation and calling the loot table. The visual prefab contains the renderer and Animator.

King Wisp Run uses four directional four-frame spectral flight loops at 14 fps in the MoveX/MoveY tree. Asset bindings checked offline; in-game playback awaits verification.

King Wisp Walk and Windup have distinct four-direction sprite clips. Walk loops at 8 fps (0.5 s); Windup plays once at 20 fps (0.2 s, matching WispBossData). Clip references checked offline; Play Mode playback unverified.
