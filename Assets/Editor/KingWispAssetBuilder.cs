using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Creates boss visuals only; the developer owns the combat/state implementation.
public static class KingWispAssetBuilder
{
    private const string Root = "Assets/Art/Characters/Enemies/KingWisp";

    [InitializeOnLoadMethod]
    private static void QueueBuild()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(Root + "/KingWisp_Idle.png") && !File.Exists(Root + "/KingWispVisual.prefab")) Build();
        };
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(Root + "/KingWisp_Idle.png")) Build();
            };
    }

    [MenuItem("Tools/Enemies/Build King Wisp Visual")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(Root + "/KingWispVisual.prefab")) return;
        string texturePath = Root + "/KingWisp_Idle.png";
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = width / 2f / 3.2f;
        importer.maxTextureSize = 512;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        var slices = new SpriteMetaData[4];
        for (int i = 0; i < 4; i++)
            slices[i] = new SpriteMetaData
            {
                name = "KingWisp_Idle_" + i,
                rect = new Rect(i % 2 * (width / 2), (1 - i / 2) * (height / 2), width / 2, height / 2),
                alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(0.5f, 0.15f)
            };
#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length != 4) throw new Exception("King Wisp needs four idle frames.");
        var clip = new AnimationClip { name = "KingWisp_Idle", frameRate = 4 };
        var frames = new ObjectReferenceKeyframe[5];
        for (int i = 0; i < frames.Length; i++) frames[i] = new ObjectReferenceKeyframe { time = i / 4f, value = sprites[i % 4] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, Root + "/KingWisp_Idle.anim");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Root + "/KingWisp.controller");
        var state = controller.layers[0].stateMachine.AddState("Idle");
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        var go = new GameObject("KingWispVisual");
        try
        {
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 1;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            go.AddComponent<Animator>().runtimeAnimatorController = controller;
            for (int i = 0; i < 4; i++)
            {
                clip.SampleAnimation(go, (i + 0.1f) / 4f);
                if (renderer.sprite != sprites[i]) throw new Exception("King Wisp frame verification failed.");
            }
            renderer.sprite = sprites[0];
            PrefabUtility.SaveAsPrefabAsset(go, Root + "/KingWispVisual.prefab");
            AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/Validation.txt", "PASS: four sprites imported and sampled in the looping idle clip; visual prefab and controller saved. Combat and in-game visual review are not included.");
            Debug.Log("King Wisp visual built: four idle frames verified, 512 texture cap, 3.2-unit sprite canvas.");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
}

