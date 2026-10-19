# Lost Wanderer — Unity animation pack

Seven separate transparent PNG sheets: Idle, Walk, Run, Dash, Attack, Hurt, and Death. Each sheet contains six frames for Down, Up, and Right. Left-facing clips mirror Right. The Run sheet copies the Walk artwork and its clips play faster; it is not a separately drawn running gait.

## Use in this project

1. Find this folder in Unity: `Assets/Art/Characters/LostWanderer/TopDownV2`.
2. Drag `LostWandererVisual.prefab` under your Player as a child. The child contains the SpriteRenderer and Animator; the animation curves target components on that same object.
3. Set the child's local position to zero and adjust its sorting layer/order for your level. Avoid showing both your old placeholder renderer and the new visual.
4. Your own state code selects the action using a cached state hash. Each action contains a directional blend tree driven by the **float** parameters `MoveX` and `MoveY`. There are no transition connections.

```csharp
// Cache once as fields in your own PlayerAnimator:
private static readonly int AttackHash = Animator.StringToHash("Base Layer.Attack");
private static readonly int MoveXHash = Animator.StringToHash("MoveX");
private static readonly int MoveYHash = Animator.StringToHash("MoveY");

// Inside your own methods; animator is on the LostWandererVisual child:
animator.SetFloat(MoveXHash, 0f);
animator.SetFloat(MoveYHash, 1f); // Face up.
animator.Play(AttackHash, 0, 0f); // Call on state entry / deliberate restart.
```

Update facing from nonzero movement. Retain the last direction when stopped. For this four-direction sprite pack, choose the dominant axis for diagonal movement and pass one cardinal unit vector. Do not pass (0,0) when stopping: retain your last facing. Clips control `SpriteRenderer.flipX`, so do not also flip it in your movement script. Call `Play(hash, 0, 0f)` on state entry, not every Update, otherwise it restarts playback every frame.

| State hash string | State | FPS | Duration | Loop |
|---|---|---|---|---|
| Base Layer.Idle | Idle | 6 | 1.00 s | Yes |
| Base Layer.Walk | Walk | 8 | 0.75 s | Yes |
| Base Layer.Dash | Dash | 24 | 0.25 s | No |
| Base Layer.Attack | Attack | 12 | 0.50 s | No |
| Base Layer.Hurt | Hurt | 12 | 0.50 s | No |
| Base Layer.Dead | Dead (Death sheet) | 10 | 0.60 s | No |
| Base Layer.Run | Run | 12 | 0.50 s | Yes |

| MoveX, MoveY | Facing |
|---|---|
| 0, -1 | Down |
| 0, 1 | Up |
| 1, 0 | Right |
| -1, 0 | Left (mirrored Right) |

One-shot animations hold their final frame. Your gameplay state machine decides when to return to Idle/Walk, when damage occurs, and when movement resumes. Death stays on its final frame until another state is selected. Animation speed does not move the Rigidbody. There are no jump or fall animations.

These animation assets are independent of the existing PlayerAnimator/PlayerStateMachine code. No gameplay wiring is included. Hashes choose actions; blend trees choose their directions. The clips can also be reused in a controller you build yourself.

## Sprite Editor

The sheets are already sliced. Select a PNG and choose **Open Sprite Editor** to inspect/edit its 18 named rectangles. Avoid re-slicing after creating clips, because replacing sprite IDs can break their references.

For a fresh PNG import in another project:

- Texture Type: Sprite (2D and UI); Sprite Mode: Multiple.
- Pixels Per Unit: 128; Mesh Type: Full Rect.
- Filter Mode: Point; Compression: None; Generate Mip Maps: off.
- Alpha Source: Input Texture Alpha; Alpha Is Transparency: on.
- Max Size: at least 2048; keep the original 1536 x 768 dimensions.
- Slice: Grid by Cell Size, 256 x 256, offset (0,0), padding (0,0).
- Pivot: Custom (0.5, 0.125), which is (128,32) pixels from each cell's bottom-left.
- Rows from the top: Down, Up, Right. Read frames left to right.

The transparent margin belongs inside each cell; it is not Sprite Editor grid padding.

## Preview and verification

The pack was generated and tested in Unity 6000.3.10f1. The automated checks exercise the actual imported Sprite assets and Animator on a temporary prefab instance: 126 rectangles/pivots, 28 clips, 7 hash-driven states with no transitions, 28 state/facing combinations, 504 frame evaluations (direct samples, Animator seeks, and advancing time), and 56 loop/final-frame checks. The current result is recorded in `GeneratedAssets/LostWandererV2/unity-validation-result.json`.

To preview manually, open the prefab, select its visual GameObject, open **Window > Animation > Animation**, select a clip, and use the preview/play controls. Runtime movement and combat still require your own state logic.

The artwork was created with built-in image generation and packed into exact grids. Slight changes to cloth and face details remain between generated frames; technical playback checks are not a guarantee of hand-drawn animation polish. Left mirroring also mirrors the lantern.
