public static class LostWandererUnitySetup
{
    public static string Setup()
    {
// Unity Pipeline eval statement block. No using directives or type declarations.
// Run only after all seven final PNGs are present. Does not touch scenes or existing player scripts.
var artRoot = "Assets/Art/Characters/LostWanderer/TopDownV2";
var animationRoot = artRoot + "/Animations";
var controllerPath = artRoot + "/LostWanderer.controller";
var prefabPath = artRoot + "/LostWandererVisual.prefab";
var stateNames = new string[] { "Idle", "Walk", "Dash", "Attack", "Hurt", "Death", "Run" };
var directions = new string[] { "Down", "Up", "Right", "Left" };
var rates = new float[] { 6f, 8f, 24f, 12f, 12f, 10f, 12f };
if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
    throw new System.Exception("Exit Play mode before creating animation assets.");

// Fail before modifying any importer when an input is missing or has the wrong grid.
UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
{
    var path = artRoot + "/LostWanderer_" + stateNames[stateIndex] + ".png";
    var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
    if (importer == null) throw new System.Exception("Missing texture: " + path);
    int width; int height;
    importer.GetSourceTextureWidthAndHeight(out width, out height);
    if (width != 1536 || height != 768)
        throw new System.Exception(path + " must be exactly 1536 x 768, got " + width + " x " + height);
}

var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
factory.Init();
var importers = new UnityEditor.TextureImporter[stateNames.Length];
var providers = new UnityEditor.U2D.Sprites.ISpriteEditorDataProvider[stateNames.Length];
for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
{
    var path = artRoot + "/LostWanderer_" + stateNames[stateIndex] + ".png";
    var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
    importer.textureType = UnityEditor.TextureImporterType.Sprite;
    importer.spriteImportMode = UnityEditor.SpriteImportMode.Multiple;
    importer.spritePixelsPerUnit = 128f;
    importer.alphaSource = UnityEditor.TextureImporterAlphaSource.FromInput;
    importer.alphaIsTransparency = true;
    importer.mipmapEnabled = false;
    importer.isReadable = false;
    importer.npotScale = UnityEditor.TextureImporterNPOTScale.None;
    importer.maxTextureSize = 2048;
    importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
    importer.filterMode = UnityEngine.FilterMode.Point;
    importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
    var textureSettings = new UnityEditor.TextureImporterSettings();
    importer.ReadTextureSettings(textureSettings);
    textureSettings.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
    importer.SetTextureSettings(textureSettings);
    importer.SaveAndReimport();
    var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
    if (provider == null) throw new System.Exception("No sprite data provider for " + path);
    provider.InitSpriteEditorDataProvider();
    var editCapability = provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteFrameEditCapability>();
    if (editCapability == null) throw new System.Exception("Missing edit capabilities: " + path);
    var capability = editCapability.GetEditCapability();
    var required = new UnityEditor.U2D.Sprites.EEditCapability[] {
        UnityEditor.U2D.Sprites.EEditCapability.CreateAndDeleteSprite,
        UnityEditor.U2D.Sprites.EEditCapability.EditSpriteName,
        UnityEditor.U2D.Sprites.EEditCapability.EditSpriteRect,
        UnityEditor.U2D.Sprites.EEditCapability.EditPivot,
        UnityEditor.U2D.Sprites.EEditCapability.EditBorder
    };
    foreach (var value in required)
        if (!capability.HasCapability(value))
            throw new System.Exception("Importer lacks " + value + ": " + path + ". Aborted.");
    if (provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteNameFileIdDataProvider>() == null)
        throw new System.Exception("Missing name/file ID provider: " + path);
    importers[stateIndex] = importer;
    providers[stateIndex] = provider;
}

// All providers passed capability checks; assign stable IDs by preserving matching sprite names.
for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
{
    var provider = providers[stateIndex];
    var oldIds = new System.Collections.Generic.Dictionary<string, UnityEditor.GUID>();
    foreach (var old in provider.GetSpriteRects()) oldIds[old.name] = old.spriteID;
    var rects = new UnityEditor.SpriteRect[18];
    var pairs = new System.Collections.Generic.List<UnityEditor.SpriteNameFileIdPair>();
    for (int row = 0; row < 3; row++)
    {
        for (int column = 0; column < 6; column++)
        {
            var name = "LostWanderer_" + stateNames[stateIndex] + "_" + directions[row] + "_" + column.ToString("00");
            UnityEditor.GUID id;
            if (!oldIds.TryGetValue(name, out id)) id = UnityEditor.GUID.Generate();
            var rect = new UnityEditor.SpriteRect();
            rect.name = name;
            rect.spriteID = id;
            rect.rect = new UnityEngine.Rect(column * 256, (2 - row) * 256, 256, 256);
            rect.pivot = new UnityEngine.Vector2(0.5f, 0.125f);
            rect.alignment = UnityEngine.SpriteAlignment.Custom;
            rect.border = UnityEngine.Vector4.zero;
            rects[row * 6 + column] = rect;
            pairs.Add(new UnityEditor.SpriteNameFileIdPair(name, id));
        }
    }
    provider.SetSpriteRects(rects);
    provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
    provider.Apply();
    importers[stateIndex].SaveAndReimport();
}

if (!UnityEditor.AssetDatabase.IsValidFolder(animationRoot))
    UnityEditor.AssetDatabase.CreateFolder(artRoot, "Animations");
var clips = new System.Collections.Generic.Dictionary<string, UnityEngine.AnimationClip>();
for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
{
    var sourceState = stateNames[stateIndex];
    var sheetPath = artRoot + "/LostWanderer_" + sourceState + ".png";
    var sprites = new System.Collections.Generic.Dictionary<string, UnityEngine.Sprite>();
    foreach (var asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(sheetPath))
        if (asset is UnityEngine.Sprite) sprites[asset.name] = (UnityEngine.Sprite)asset;
    if (sprites.Count != 18) throw new System.Exception("Expected 18 sprites: " + sheetPath);
    for (int direction = 0; direction < 4; direction++)
    {
        var clipName = stateNames[stateIndex] + "_" + directions[direction];
        var sourceDirection = directions[direction == 3 ? 2 : direction];
        var clipPath = animationRoot + "/" + clipName + ".anim";
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new UnityEngine.AnimationClip();
            clip.name = clipName;
            UnityEditor.AssetDatabase.CreateAsset(clip, clipPath);
        }
        clip.ClearCurves();
        clip.frameRate = rates[stateIndex];
        // Unity holds the last object-reference key for one frame interval.
        // Six keys at 0..5/fps therefore give the intended six-frame duration.
        var keys = new UnityEditor.ObjectReferenceKeyframe[6];
        var loop = stateIndex == 0 || stateIndex == 1 || stateIndex == 6;
        for (int frame = 0; frame < 6; frame++)
        {
            var spriteName = "LostWanderer_" + sourceState + "_" + sourceDirection + "_" + frame.ToString("00");
            keys[frame] = new UnityEditor.ObjectReferenceKeyframe {
                time = frame / rates[stateIndex], value = sprites[spriteName]
            };
        }
        var binding = UnityEditor.EditorCurveBinding.PPtrCurve("", typeof(UnityEngine.SpriteRenderer), "m_Sprite");
        UnityEditor.AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var flipBinding = UnityEditor.EditorCurveBinding.FloatCurve("", typeof(UnityEngine.SpriteRenderer), "m_FlipX");
        UnityEditor.AnimationUtility.SetEditorCurve(clip, flipBinding,
            UnityEngine.AnimationCurve.Constant(0f, 5f / rates[stateIndex], direction == 3 ? 1f : 0f));
        var settings = UnityEditor.AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        settings.loopBlend = false;
        UnityEditor.AnimationUtility.SetAnimationClipSettings(clip, settings);
        UnityEditor.EditorUtility.SetDirty(clip);
        clips.Add(clipName, clip);
    }
}

var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
if (controller == null) controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
controller.parameters = new UnityEngine.AnimatorControllerParameter[] {
    new UnityEngine.AnimatorControllerParameter { name = "MoveX", type = UnityEngine.AnimatorControllerParameterType.Float, defaultFloat = 0f },
    new UnityEngine.AnimatorControllerParameter { name = "MoveY", type = UnityEngine.AnimatorControllerParameterType.Float, defaultFloat = -1f }
};
var stateMachine = controller.layers[0].stateMachine;
foreach (var oldTransition in stateMachine.anyStateTransitions) stateMachine.RemoveAnyStateTransition(oldTransition);
foreach (var oldState in stateMachine.states) stateMachine.RemoveState(oldState.state);
foreach (var subAsset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(controllerPath))
    if (subAsset is UnityEditor.Animations.BlendTree) UnityEngine.Object.DestroyImmediate(subAsset, true);
var facingPositions = new UnityEngine.Vector2[] { UnityEngine.Vector2.down, UnityEngine.Vector2.up, UnityEngine.Vector2.right, UnityEngine.Vector2.left };
for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
{
    var stateName = stateIndex == 5 ? "Dead" : stateNames[stateIndex];
    var state = stateMachine.AddState(stateName, new UnityEngine.Vector3(300f, stateIndex * 80f, 0f));
    var tree = new UnityEditor.Animations.BlendTree();
    tree.name = stateName + "_Facing";
    tree.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
    tree.blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D;
    tree.blendParameter = "MoveX";
    tree.blendParameterY = "MoveY";
    tree.useAutomaticThresholds = false;
    UnityEditor.AssetDatabase.AddObjectToAsset(tree, controller);
    for (int direction = 0; direction < 4; direction++)
    {
        var name = stateNames[stateIndex] + "_" + directions[direction];
        tree.AddChild(clips[name], facingPositions[direction]);
    }
    state.motion = tree;
    state.writeDefaultValues = false;
    if (stateIndex == 0) stateMachine.defaultState = state;
}
UnityEditor.EditorUtility.SetDirty(controller);

var previewScene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var visual = new UnityEngine.GameObject("LostWandererVisual");
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(visual, previewScene);
try
{
    var renderer = visual.AddComponent<UnityEngine.SpriteRenderer>();
    var firstCurve = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clips["Idle_Down"],
        UnityEditor.EditorCurveBinding.PPtrCurve("", typeof(UnityEngine.SpriteRenderer), "m_Sprite"));
    renderer.sprite = (UnityEngine.Sprite)firstCurve[0].value;
    renderer.spriteSortPoint = UnityEngine.SpriteSortPoint.Pivot;
    var animator = visual.AddComponent<UnityEngine.Animator>();
    animator.runtimeAnimatorController = controller;
    animator.applyRootMotion = false;
    animator.cullingMode = UnityEngine.AnimatorCullingMode.AlwaysAnimate;
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(visual, prefabPath);
}
finally
{
    UnityEngine.Object.DestroyImmediate(visual);
    UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(previewScene);
}
UnityEditor.AssetDatabase.SaveAssets();
return "Created 126 sliced sprites, 28 directional clips, 7 hash-driven blend-tree states with zero transitions, and isolated visual prefab at " + artRoot;

    }
    public static string Validate()
    {
// Unity Pipeline eval statement block. Run after SetupUnityAnimations.cs.txt.
// Uses real imported Sprite assets and evaluates each AnimationClip on a temporary prefab instance.
var artRoot = "Assets/Art/Characters/LostWanderer/TopDownV2";
var stateNames = new string[] { "Idle", "Walk", "Dash", "Attack", "Hurt", "Death", "Run" };
var directionNames = new string[] { "Down", "Up", "Right", "Left" };
var rates = new float[] { 6f, 8f, 24f, 12f, 12f, 10f, 12f };
var facingPositions = new UnityEngine.Vector2[] { UnityEngine.Vector2.down, UnityEngine.Vector2.up, UnityEngine.Vector2.right, UnityEngine.Vector2.left };
var report = new System.Text.StringBuilder();
int spriteCount = 0;
for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
{
    var path = artRoot + "/LostWanderer_" + stateNames[stateIndex] + ".png";
    var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
    if (importer == null || importer.spriteImportMode != UnityEditor.SpriteImportMode.Multiple)
        throw new System.Exception("Not imported as Multiple: " + path);
    if (importer.spritePixelsPerUnit != 128f || importer.mipmapEnabled || !importer.alphaIsTransparency)
        throw new System.Exception("Incorrect importer settings: " + path);
    if (importer.filterMode != UnityEngine.FilterMode.Point ||
        importer.textureCompression != UnityEditor.TextureImporterCompression.Uncompressed ||
        importer.wrapMode != UnityEngine.TextureWrapMode.Clamp)
        throw new System.Exception("Expected Point filtering, Uncompressed texture, and Clamp wrapping: " + path);
    var textureSettings = new UnityEditor.TextureImporterSettings();
    importer.ReadTextureSettings(textureSettings);
    if (textureSettings.spriteMeshType != UnityEngine.SpriteMeshType.FullRect)
        throw new System.Exception("Expected FullRect sprite mesh: " + path);
    var sprites = new System.Collections.Generic.Dictionary<string, UnityEngine.Sprite>();
    foreach (var asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
        if (asset is UnityEngine.Sprite) sprites.Add(asset.name, (UnityEngine.Sprite)asset);
    if (sprites.Count != 18) throw new System.Exception("Expected 18 sprites in " + path);
    for (int row = 0; row < 3; row++)
        for (int frame = 0; frame < 6; frame++)
        {
            var name = "LostWanderer_" + stateNames[stateIndex] + "_" + directionNames[row] + "_" + frame.ToString("00");
            var sprite = sprites[name];
            var expectedRect = new UnityEngine.Rect(frame * 256, (2 - row) * 256, 256, 256);
            if (sprite.rect != expectedRect) throw new System.Exception("Wrong frame rectangle: " + name);
            if ((sprite.pivot - new UnityEngine.Vector2(128f, 32f)).sqrMagnitude > 0.0001f)
                throw new System.Exception("Wrong pivot: " + name);
            if (sprite.pixelsPerUnit != 128f) throw new System.Exception("Wrong PPU: " + name);
            if (sprite.texture.width != 1536 || sprite.texture.height != 768)
                throw new System.Exception("Texture was resized during import: " + path);
            spriteCount++;
        }
}
var prefabPath = artRoot + "/LostWandererVisual.prefab";
var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabPath);
if (prefab == null) throw new System.Exception("Missing visual prefab.");
var previewScene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var instance = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, previewScene);
instance.name = "LostWandererTemporaryAnimationValidation";
instance.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
int clipCount = 0;
int frameSamples = 0;
int transitionChecks = 0;
int animatorFrameSamples = 0;
int endpointChecks = 0;
int continuousFrameSamples = 0;
int continuousEndpointChecks = 0;
try
{
    var renderer = instance.GetComponent<UnityEngine.SpriteRenderer>();
    var animator = instance.GetComponent<UnityEngine.Animator>();
    if (renderer == null || animator == null || animator.runtimeAnimatorController == null)
        throw new System.Exception("Prefab missing animation components.");
    var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
    var stateMachine = controller.layers[0].stateMachine;
    if (stateMachine.states.Length != 7 || stateMachine.anyStateTransitions.Length != 0 || stateMachine.entryTransitions.Length != 0)
        throw new System.Exception("Controller must contain 7 states and zero transitions.");
    foreach (var child in stateMachine.states)
    {
        if (child.state.transitions.Length != 0) throw new System.Exception("Unexpected state transition.");
        var tree = child.state.motion as UnityEditor.Animations.BlendTree;
        if (tree == null || tree.children.Length != 4 || tree.blendParameter != "MoveX" || tree.blendParameterY != "MoveY")
            throw new System.Exception("Missing four-direction blend tree: " + child.state.name);
    }
    var binding = UnityEditor.EditorCurveBinding.PPtrCurve("", typeof(UnityEngine.SpriteRenderer), "m_Sprite");
    animator.enabled = false;
    for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
    {
        for (int direction = 0; direction < 4; direction++)
        {
            var name = stateNames[stateIndex] + "_" + directionNames[direction];
            var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(artRoot + "/Animations/" + name + ".anim");
            if (clip == null) throw new System.Exception("Missing clip: " + name);
            var keys = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding);
            if (keys == null || keys.Length != 6) throw new System.Exception("Wrong frame keys: " + name);
            if (UnityEngine.Mathf.Abs(clip.length - 6f / rates[stateIndex]) > 0.0001f)
                throw new System.Exception("Wrong clip duration: " + name);
            var shouldLoop = stateIndex == 0 || stateIndex == 1 || stateIndex == 6;
            if (UnityEditor.AnimationUtility.GetAnimationClipSettings(clip).loopTime != shouldLoop)
                throw new System.Exception("Wrong loop setting: " + name);
            for (int frame = 0; frame < 6; frame++)
            {
                if (keys[frame].value == null) throw new System.Exception("Null sprite reference: " + name);
                var sourceState = stateNames[stateIndex];
                var sourceDirection = directionNames[direction == 3 ? 2 : direction];
                var expectedSpriteName = "LostWanderer_" + sourceState + "_" + sourceDirection + "_" + frame.ToString("00");
                if (keys[frame].value.name != expectedSpriteName)
                    throw new System.Exception("Clip references wrong sprite: " + name + " frame " + frame);
                clip.SampleAnimation(instance, (frame + 0.25f) / rates[stateIndex]);
                if (renderer.sprite != keys[frame].value) throw new System.Exception("Frame sample mismatch: " + name + " frame " + frame);
                if (renderer.flipX != (direction == 3)) throw new System.Exception("Mirroring mismatch: " + name);
                frameSamples++;
            }
            clipCount++;
        }
    }
    animator.enabled = true;
    animator.Rebind();
    animator.Update(0f);
    // Exercise the user's hash-driven state selection with cardinal facing parameters.
    for (int stateIndex = 0; stateIndex < stateNames.Length; stateIndex++)
        for (int direction = 0; direction < 4; direction++)
        {
            animator.SetFloat("MoveX", facingPositions[direction].x);
            animator.SetFloat("MoveY", facingPositions[direction].y);
            var expected = stateNames[stateIndex] + "_" + directionNames[direction];
            var stateName = stateIndex == 5 ? "Dead" : stateNames[stateIndex];
            var stateHash = UnityEngine.Animator.StringToHash("Base Layer." + stateName);
            if (!animator.HasState(0, stateHash)) throw new System.Exception("Missing state hash: " + stateName);
            animator.Play(stateHash, 0, 0f);
            animator.Update(0f);
            if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != stateHash)
                throw new System.Exception("Animator hash playback failed: " + expected);
            if (renderer.sprite == null || renderer.flipX != (direction == 3))
                throw new System.Exception("Animator output invalid: " + expected);
            transitionChecks++;
            var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(artRoot + "/Animations/" + expected + ".anim");
            var keys = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding);
            // Evaluate the Animator itself at every frame, in addition to the direct clip samples above.
            for (int frame = 0; frame < 6; frame++)
            {
                animator.Play(stateHash, 0, (frame + 0.25f) / 6f);
                animator.Update(0f);
                if (renderer.sprite != keys[frame].value || renderer.flipX != (direction == 3))
                    throw new System.Exception("Animator frame evaluation failed: " + expected + " frame " + frame);
                animatorFrameSamples++;
            }
            var shouldLoop = stateIndex == 0 || stateIndex == 1 || stateIndex == 6;
            animator.Play(stateHash, 0, 1f + 0.25f / 6f);
            animator.Update(0f);
            if (renderer.sprite != keys[shouldLoop ? 0 : 5].value)
                throw new System.Exception("Animator loop/last-frame hold failed: " + expected);
            endpointChecks++;
            // Check time-driven playback as well as seeking: only the first frame is positioned explicitly.
            animator.Play(stateHash, 0, 0f);
            animator.Update(0f);
            for (int frame = 0; frame < 6; frame++)
            {
                animator.Update((frame == 0 ? 0.25f : 1f) / rates[stateIndex]);
                if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != stateHash)
                    throw new System.Exception("Continuous playback changed state unexpectedly: " + expected);
                if (renderer.sprite != keys[frame].value || renderer.flipX != (direction == 3))
                    throw new System.Exception("Continuous Animator playback failed: " + expected + " frame " + frame);
                continuousFrameSamples++;
            }
            animator.Update(1f / rates[stateIndex]);
            if (renderer.sprite != keys[shouldLoop ? 0 : 5].value || renderer.flipX != (direction == 3))
                throw new System.Exception("Continuous Animator loop/last-frame hold failed: " + expected);
            continuousEndpointChecks++;
        }
}
finally
{
    UnityEngine.Object.DestroyImmediate(instance);
    UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(previewScene);
}
report.AppendLine("PASS: " + spriteCount + " imported sprites with exact 256x256 rectangles, common pivot (128,32), PPU128, original texture dimensions, Point filtering, Uncompressed texture, Clamp wrapping, and FullRect meshes.");
report.AppendLine("PASS: " + clipCount + " clips; " + frameSamples + " actual frame samples; all left/right mirroring and loop settings checked.");
report.AppendLine("PASS: 7 hash-driven states, zero transitions, and " + transitionChecks + " state/facing combinations evaluated on the generated prefab.");
report.AppendLine("PASS: " + animatorFrameSamples + " actual Animator frame evaluations and " + endpointChecks + " loop/last-frame hold evaluations.");
report.AppendLine("PASS: " + continuousFrameSamples + " time-driven Animator frame evaluations and " + continuousEndpointChecks + " time-driven loop/last-frame hold evaluations.");
return report.ToString();

    }
}
