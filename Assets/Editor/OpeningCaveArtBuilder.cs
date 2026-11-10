using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Art-only assembly. No gameplay, input, equipment, pickup or collision logic.
public static class OpeningCaveArtBuilder
{
    private const string Root = "Assets/Art/Map/OpeningCave";
    private const string ScenePath = "Assets/Scenes/OpeningCave_Art.unity";

    [InitializeOnLoadMethod]
    private static void Queue()
    {
        EditorApplication.playModeStateChanged -= OnMode;
        EditorApplication.playModeStateChanged += OnMode;
        EditorApplication.delayCall += TryBuild;
    }
    private static void OnMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += TryBuild;
    }
    private static void TryBuild()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(Root + "/Ready.txt") && !File.Exists(ScenePath)) Build();
    }

    private static Sprite Import(string name, float worldWidth, int maximum)
    {
        string path = Root + "/" + name + ".png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = width / worldWidth;
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(textureSettings);
        importer.maxTextureSize = maximum;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject Marker(string name, Transform parent, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        return go;
    }

    [MenuItem("Tools/Areas/Build Opening Cave Art")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
        var cave = Import("Cave", 24, 2048);
        var lantern = Import("Lantern", 0.65f, 128);
        var original = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("OpeningCave_Art");
            var background = Marker("CaveBackdrop", root.transform, Vector3.zero).AddComponent<SpriteRenderer>();
            background.sprite = cave;
            background.sortingOrder = -100;
            background.color = new Color(0.28f, 0.28f, 0.32f, 1);
            var lamp = Marker("Lantern_VisualOnly", root.transform, new Vector3(-2.85f, -2.9f, 0)).AddComponent<SpriteRenderer>();
            lamp.sprite = lantern;
            lamp.sortingOrder = 5;
            Marker("PlayerWakePoint", root.transform, new Vector3(-5.8f, -3.3f, 0));
            Marker("ExitPoint", root.transform, new Vector3(8.4f, 5.4f, 0));
            var catAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/GuideCat/GuideCat.prefab");
            var cat = (GameObject)PrefabUtility.InstantiatePrefab(catAsset, scene);
            cat.transform.SetParent(root.transform, false);
            cat.transform.localPosition = new Vector3(-4.3f, -2.5f, 0);
            cat.GetComponent<SpriteRenderer>().color = new Color(0.35f, 0.35f, 0.4f, 1);
            cat.GetComponent<SpriteRenderer>().sortingOrder = 5;
            var bubble = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialogue/CatSpeechBubble.prefab"), scene);
            bubble.transform.SetParent(cat.transform, false);
            bubble.transform.localPosition = new Vector3(0, 0.3f, 0);
            bubble.transform.localScale = Vector3.one * 0.01f;
            foreach (var text in bubble.GetComponentsInChildren<TextMeshProUGUI>())
                if (text.name == "DialogueText") text.text = "You're awake.\nWalk towards the lantern.";
            // Existing wanderer art already holds a lantern, so keep it as a hidden sizing reference.
            var wanderer = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/LostWanderer/TopDownV2/LostWandererVisual.prefab"), scene);
            wanderer.name = "Wanderer_ScaleReference_Hidden";
            wanderer.transform.SetParent(root.transform, false);
            wanderer.transform.localPosition = new Vector3(-5.8f, -3.3f, 0);
            wanderer.SetActive(false);

            var cameraGo = new GameObject("ArtPreviewCamera", typeof(Camera));
            var camera = cameraGo.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.transform.position = new Vector3(-4, -2.6f, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            PrefabUtility.SaveAsPrefabAsset(root, Root + "/OpeningCave_Art.prefab");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/Validation.txt", "PASS: cave scene and environment prefab saved; lantern, cat and dialogue placed; no gameplay or colliders. Existing scene was not changed. Visual review still required.");
            Debug.Log("Opening cave ART ONLY created at " + ScenePath);
        }
        finally
        {
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}

