using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Editor-only asset authoring. No tutorial, movement, or input behaviour.
public static class GuideCatAssetBuilder
{
    private const string Root = "Assets/Art/Characters/GuideCat";
    private const string Sheet = Root + "/GuideCat.png";
    private const float CatPixelsPerUnit = 256f;
    private static readonly string[] Actions = { "Idle", "Talk", "Walk", "Run" };
    private static readonly string[] Directions = { "Down", "Up", "Right", "Left" };

    [InitializeOnLoadMethod]
    private static void QueueFirstImport()
    {
        EditorApplication.delayCall += () =>
        {
            ApplyCatSize();
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                File.Exists(Sheet) && !File.Exists(Root + "/GuideCat.prefab"))
                Build();
        };
    }

    [MenuItem("Tools/Guide Cat/Apply Correct Proportions")]
    public static void ApplyCatSize()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var importer = AssetImporter.GetAtPath(Sheet) as TextureImporter;
        if (importer == null || Mathf.Approximately(importer.spritePixelsPerUnit, CatPixelsPerUnit)) return;
        importer.spritePixelsPerUnit = CatPixelsPerUnit;
        importer.SaveAndReimport();
        Debug.Log("Guide Cat size corrected: 256 pixels per unit, half the previous width and height, preserving proportions across all animations.");
    }

    [MenuItem("Tools/Guide Cat/Build Animation Assets")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play Mode before building cat assets.");
        if (File.Exists(Root + "/GuideCat.controller"))
        {
            Debug.Log("Guide Cat assets already exist; preserving your edits.");
            return;
        }
        AssetDatabase.ImportAsset(Sheet, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = CatPixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        var slices = new SpriteMetaData[54];
        for (int row = 0; row < 9; row++)
        for (int col = 0; col < 6; col++)
        {
            int x0 = Mathf.RoundToInt(col * width / 6f);
            int x1 = Mathf.RoundToInt((col + 1) * width / 6f);
            int y0 = height - Mathf.RoundToInt((row + 1) * height / 9f);
            int y1 = height - Mathf.RoundToInt(row * height / 9f);
            slices[row * 6 + col] = new SpriteMetaData
            {
                name = "Cat_" + row + "_" + col,
                rect = new Rect(x0, y0, x1 - x0, y1 - y0),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, 0.08f)
            };
        }
#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(s => s.name);
        if (sprites.Count != 54) throw new InvalidOperationException("Expected 54 cat sprites.");
        Directory.CreateDirectory(Root + "/Animations");
        AssetDatabase.Refresh();
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Root + "/GuideCat.controller");
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        var parameters = controller.parameters;
        parameters[1].defaultFloat = -1;
        controller.parameters = parameters;
        var machine = controller.layers[0].stateMachine;
        var preview = new GameObject("GuideCat");
        try
        {
            var renderer = preview.AddComponent<SpriteRenderer>();
            preview.AddComponent<Animator>().runtimeAnimatorController = controller;
            for (int action = 0; action < 4; action++)
            {
                var state = machine.AddState(Actions[action]);
                var tree = new BlendTree { name = Actions[action] + "Direction", blendType = BlendTreeType.SimpleDirectional2D,
                    blendParameter = "MoveX", blendParameterY = "MoveY", useAutomaticThresholds = false };
                AssetDatabase.AddObjectToAsset(tree, controller);
                state.motion = tree;
                if (action == 0) machine.defaultState = state;
                for (int direction = 0; direction < 4; direction++)
                {
                    int row = Math.Max(0, action - 1) * 3 + Math.Min(direction, 2);
                    int count = action == 0 ? 1 : 6;
                    float fps = action == 3 ? 12 : action == 2 ? 8 : 6;
                    var clip = new AnimationClip { name = Actions[action] + "_" + Directions[direction], frameRate = fps };
                    var keys = new ObjectReferenceKeyframe[count + 1];
                    for (int frame = 0; frame <= count; frame++)
                        keys[frame] = new ObjectReferenceKeyframe { time = frame / fps, value = sprites["Cat_" + row + "_" + (frame % count)] };
                    AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                    clip.SetCurve("", typeof(SpriteRenderer), "m_FlipX", AnimationCurve.Constant(0, count / fps, direction == 3 ? 1 : 0));
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    AssetDatabase.CreateAsset(clip, Root + "/Animations/" + clip.name + ".anim");
                    var positions = new[] { Vector2.down, Vector2.up, Vector2.right, Vector2.left };
                    tree.AddChild(clip, positions[direction]);
                    for (int frame = 0; frame < count; frame++)
                    {
                        clip.SampleAnimation(preview, (frame + 0.1f) / fps);
                        if (renderer.sprite != keys[frame].value)
                            throw new InvalidOperationException("Frame validation failed: " + clip.name);
                    }
                }
            }
            renderer.sprite = sprites["Cat_0_0"];
            renderer.flipX = false;
            PrefabUtility.SaveAsPrefabAsset(preview, Root + "/GuideCat.prefab");
            AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/Validation.txt", "PASS: 54 sprites, 16 clips, four directional blend trees. Every sprite key sampled successfully. Visual animation review in game still required.");
            Debug.Log("Guide Cat: created and sampled all 16 animation clips. Prefab ready.");
        }
        finally { UnityEngine.Object.DestroyImmediate(preview); }
    }
}
