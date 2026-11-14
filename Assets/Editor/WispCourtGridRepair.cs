using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Editor-only diagnostics and asset conversion. No gameplay behaviour.
[InitializeOnLoad]
public static class WispCourtGridRepair
{
    private const string Root = "Assets/Art/Map/Dungeon/WispCourt128";
    private const string Original = "Assets/Art/Map/Dungeon/WispCourt";
    private const string Palette = Original + "/Wisp Court.prefab";
    private const int CellPixels = 128;
    private const int Subdivision = 4;

    static WispCourtGridRepair() { EditorApplication.delayCall += Run; }

    private static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Inspect();
        try
        {
            if (!File.Exists(Root + "/Verified.txt")) Repair();
            FinalizeScene();
        }
        catch (Exception error)
        {
            File.WriteAllText("Temp/WispCourt128Failure.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    public static void Repair()
    {
        var reference = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Map/ReferenceRuins128/Palettes/Ground128.prefab");
        if (reference == null || reference.GetComponent<Grid>().cellSize != Vector3.one)
            throw new InvalidOperationException("ReferenceRuins128 unit grid must be present before conversion.");
        var referenceImporter = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Map/ReferenceRuins128/Textures/Ground_128.png");
        if (referenceImporter.spritePixelsPerUnit != CellPixels)
            throw new InvalidOperationException("ReferenceRuins128 is no longer using 128 PPU.");

        // Snapshot the actual prefab before touching it. Existing source Tile assets are preserved.
        var originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Palette);
        var originalMap = originalPrefab.GetComponentInChildren<Tilemap>();
        var originalGrid = originalPrefab.GetComponent<Grid>();
        if (originalGrid.cellSize.x != 4 || originalGrid.cellSize.y != 4)
            throw new InvalidOperationException("Expected the original four-unit Wisp Court grid.");
        var placements = new Dictionary<Vector3Int, string>();
        foreach (var p in originalMap.cellBounds.allPositionsWithin)
        {
            var tile = originalMap.GetTile<Tile>(p);
            if (tile == null) continue;
            if (originalMap.GetTransformMatrix(p) != Matrix4x4.identity || originalMap.GetColor(p) != Color.white)
                throw new InvalidOperationException("Non-default stamp transform or tint needs explicit conversion: " + p);
            placements.Add(p, tile.name);
        }
        if (placements.Count != 96) throw new InvalidOperationException("Expected 96 original Wisp Court stamps.");
        Directory.CreateDirectory(Root + "/Textures");
        Directory.CreateDirectory(Root + "/Tiles");
        Directory.CreateDirectory("Temp/WispCourtBeforeRepair");
        File.Copy(Palette, "Temp/WispCourtBeforeRepair/Wisp Court.prefab", true);
        File.Copy(Palette + ".meta", "Temp/WispCourtBeforeRepair/Wisp Court.prefab.meta", true);

        foreach (var window in Resources.FindObjectsOfTypeAll<GridPaintPaletteWindow>()) window.Close();
        var sourceFiles = Directory.GetFiles(Original, "Court_*.png").OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (sourceFiles.Length != 96) throw new InvalidOperationException("Expected exactly 96 source PNGs.");
        var allTiles = new Dictionary<string, Tile>();
        for (int page = 0; page < 6; page++)
        {
            string sheetPath = Root + "/Textures/WispCourt128_" + page.ToString("00") + ".png";
            var pixels = new Color32[2048 * 2048];
            var rectangles = new List<SpriteRect>();
            for (int stamp = 0; stamp < 16; stamp++)
            {
                string sourcePath = sourceFiles[page * 16 + stamp];
                string stem = Path.GetFileNameWithoutExtension(sourcePath);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!texture.LoadImage(File.ReadAllBytes(sourcePath)) || texture.width != 128 || texture.height != 128)
                        throw new InvalidDataException("Unexpected source dimensions: " + sourcePath);
                    var sourcePixels = texture.GetPixels32();
                    int baseX = stamp % 4 * 512;
                    int baseY = stamp / 4 * 512;
                    // Exact nearest-neighbour enlargement: original art keeps its world size and pixel appearance.
                    for (int y = 0; y < 512; y++)
                    for (int x = 0; x < 512; x++)
                        pixels[(baseY + y) * 2048 + baseX + x] = sourcePixels[y / Subdivision * 128 + x / Subdivision];
                    for (int y = 0; y < Subdivision; y++)
                    for (int x = 0; x < Subdivision; x++)
                        rectangles.Add(new SpriteRect {
                            name = UnitName(stem, x, y),
                            rect = new Rect(baseX + x * CellPixels, baseY + y * CellPixels, CellPixels, CellPixels),
                            pivot = new Vector2(0.5f, 0.5f), alignment = SpriteAlignment.Center, spriteID = GUID.Generate()
                        });
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
            }
            var atlas = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
            try { atlas.SetPixels32(pixels); atlas.Apply(); File.WriteAllBytes(sheetPath, atlas.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(atlas); }
            AssetDatabase.ImportAsset(sheetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(sheetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = referenceImporter.spritePixelsPerUnit;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite Editor data provider unavailable.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite Editor edit capability unavailable; conversion aborted.");
            var capabilities = edit.GetEditCapability();
            foreach (var required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect, EEditCapability.EditSpriteName, EEditCapability.EditPivot })
                if (!capabilities.HasCapability(required)) throw new InvalidOperationException("Sprite Editor does not support " + required + "; conversion aborted.");
            var oldRects = provider.GetSpriteRects().ToDictionary(rect => rect.name);
            foreach (var rect in rectangles)
                if (oldRects.TryGetValue(rect.name, out var old)) rect.spriteID = old.spriteID;
            provider.SetSpriteRects(rectangles.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names == null) throw new InvalidOperationException("Sprite name and file-ID provider unavailable.");
            names.SetNameFileIdPairs(rectangles.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
            if (sprites.Length != 256) throw new InvalidDataException("Expected 256 sprites in " + sheetPath);
            foreach (var sprite in sprites)
            {
                if (sprite.rect.width != CellPixels || sprite.rect.height != CellPixels || sprite.pixelsPerUnit != CellPixels || sprite.bounds.size.x != 1 || sprite.bounds.size.y != 1)
                    throw new InvalidDataException("Sprite does not match ReferenceRuins128: " + sprite.name);
                string path = Root + "/Tiles/" + sprite.name + ".asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, path); }
                tile.sprite = sprite; tile.color = Color.white; tile.transform = Matrix4x4.identity;
                tile.colliderType = Tile.ColliderType.None; EditorUtility.SetDirty(tile);
                allTiles.Add(sprite.name, tile);
            }
        }
        AssetDatabase.SaveAssets();
        if (allTiles.Count != 1536) throw new InvalidDataException("Expected 1536 unit tiles before replacing palette.");

        var prefab = PrefabUtility.LoadPrefabContents(Palette);
        try
        {
            var grid = prefab.GetComponent<Grid>(); grid.cellSize = reference.GetComponent<Grid>().cellSize;
            grid.cellGap = Vector3.zero; grid.cellLayout = GridLayout.CellLayout.Rectangle;
            var map = prefab.GetComponentInChildren<Tilemap>();
            map.ClearAllTiles();
            foreach (var entry in placements)
            for (int y = 0; y < Subdivision; y++)
            for (int x = 0; x < Subdivision; x++)
                map.SetTile(new Vector3Int(entry.Key.x * Subdivision + x, entry.Key.y * Subdivision + y, entry.Key.z), allTiles[UnitName(entry.Value, x, y)]);
            map.RefreshAllTiles(); map.CompressBounds();
            PrefabUtility.SaveAsPrefabAsset(prefab, Palette);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        var paletteSettings = AssetDatabase.LoadAllAssetsAtPath(Palette).OfType<GridPalette>().Single();
        paletteSettings.cellSizing = GridPalette.CellSizing.Manual; EditorUtility.SetDirty(paletteSettings);
        AssetDatabase.SaveAssets();

        // Validate actual painting in an isolated preview scene, without adding test tiles to the user's scene.
        var preview = EditorSceneManager.NewPreviewScene();
        try
        {
            var testGrid = new GameObject("WispCourtPaintVerification", typeof(Grid));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(testGrid, preview);
            testGrid.GetComponent<Grid>().cellSize = Vector3.one;
            var testMapObject = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
            testMapObject.transform.SetParent(testGrid.transform, false);
            var testMap = testMapObject.GetComponent<Tilemap>();
            for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++)
                testMap.SetTile(new Vector3Int(x, y, 0), allTiles[UnitName("Court_00_00", x, y)]);
            testMap.RefreshAllTiles();
            if (testMap.GetUsedTilesCount() != 16 || testMap.GetCellCenterWorld(Vector3Int.right) - testMap.GetCellCenterWorld(Vector3Int.zero) != Vector3.right)
                throw new InvalidDataException("Unit-grid paint verification failed.");
            testMap.SetTile(Vector3Int.zero, null);
            if (testMap.HasTile(Vector3Int.zero)) throw new InvalidDataException("Erase verification failed.");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }

        var repaired = AssetDatabase.LoadAssetAtPath<GameObject>(Palette);
        var repairedMap = repaired.GetComponentInChildren<Tilemap>();
        int occupied = 0;
        foreach (var position in repairedMap.cellBounds.allPositionsWithin) if (repairedMap.HasTile(position)) occupied++;
        if (occupied != 1536 || repaired.GetComponent<Grid>().cellSize != Vector3.one || repairedMap.cellBounds.size != new Vector3Int(48, 32, 1))
            throw new InvalidDataException("Converted palette dimensions or tile count do not match.");
        File.WriteAllText(Root + "/Verified.txt", "ReferenceRuins128 matched: 128px sprites, 128 PPU, centered pivots, full rectangles, Point filtering, no mipmaps, no compression, identity tile transforms, 1x1x1 palette Grid.\n96 four-unit source stamps converted into 1536 one-unit paint cells; assembled art remains 48 x 32 world units. Original source assets unchanged.\nSprite Editor edit capabilities checked. Paint, adjacent placement, erase, asset references and palette dimensions verified in the Unity Editor.\nExisting half-unit collision grid and gameplay scripts unchanged. Collision/gameplay alignment remains unverified.\n2D Animation 13.0.6 is installed; static map tiles do not need skeletal animation.\n");
        AssetDatabase.Refresh();
        Inspect();
        Selection.activeObject = repaired;
        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        GridPaintingState.palette = repaired;
        var liveMap = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(map => PrefabUtility.GetCorrespondingObjectFromSource(map.gameObject) == repairedMap.gameObject);
        if (liveMap != null) GridPaintingState.scenePaintTarget = liveMap.gameObject;
        Debug.Log("Wisp Court repaired: 1536 paintable one-unit tiles, matching ReferenceRuins128. Original map size preserved; paint and erase checks passed.");
    }

    private static string UnitName(string stem, int x, int y) { return stem + "_u" + x + "_" + y; }

    private static void FinalizeScene()
    {
        if (File.Exists(Root + "/SceneSetup.txt")) return;
        var map = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.gameObject.scene.name == "Dungeon" &&
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(candidate.gameObject) == Palette);
        if (map == null) return;
        var root = PrefabUtility.GetOutermostPrefabInstanceRoot(map.gameObject);
        if (root == null || root.GetComponent<Grid>().cellSize != Vector3.one)
            throw new InvalidOperationException("Scene Wisp Court must have the verified unit Grid.");
        // Unity refuses tile painting on prefab instances: make this map instance editable.
        Directory.CreateDirectory("Temp/WispCourtBeforeRepair");
        if (!EditorSceneManager.SaveScene(root.scene, "Temp/WispCourtBeforeRepair/DungeonBeforeUnpack.unity", true))
            throw new IOException("Could not checkpoint the current unsaved scene before unpacking.");
        Undo.RegisterFullObjectHierarchyUndo(root, "Make Wisp Court tilemap editable");
        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        map.name = "Court Artwork";
        EditorSceneManager.MarkSceneDirty(root.scene);
        if (PrefabUtility.IsPartOfPrefabInstance(map.gameObject))
            throw new InvalidOperationException("Scene tilemap is still a prefab instance and cannot be painted.");
        if (!EditorSceneManager.SaveScene(root.scene)) throw new IOException("Could not save the repaired Dungeon scene.");
        Selection.activeGameObject = map.gameObject;
        GridPaintingState.scenePaintTarget = map.gameObject;
        File.WriteAllText(Root + "/SceneSetup.txt", "Dungeon/Wisp Court/Court Artwork: editable scene Tilemap on a 1-unit Grid. Prefab instance unpacked with Undo support; original palette remains an asset. Current scene saved.\n");
        Inspect();
        Debug.Log("Wisp Court scene canvas is now editable: prefab-instance paint restriction removed; Dungeon saved.");
    }

    private static void Inspect()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var report = new StringBuilder();
        foreach (var grid in UnityEngine.Object.FindObjectsByType<Grid>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            report.AppendLine("GRID " + PathOf(grid.transform) + " cell=" + grid.cellSize + " position=" + grid.transform.position + " scale=" + grid.transform.lossyScale);
            foreach (var map in grid.GetComponentsInChildren<Tilemap>(true))
            {
                report.AppendLine(" MAP " + PathOf(map.transform) + " bounds=" + map.cellBounds + " local=" + map.transform.localPosition + " scale=" + map.transform.lossyScale + " tiles=" + map.GetUsedTilesCount());
                int count = 0;
                foreach (var p in map.cellBounds.allPositionsWithin)
                {
                    var tile = map.GetTile(p); if (tile == null) continue;
                    if (count++ < 3) report.AppendLine("  " + p + " " + AssetDatabase.GetAssetPath(tile) + " center=" + map.GetCellCenterWorld(p));
                }
                report.AppendLine("  occupied=" + count);
            }
        }
        File.WriteAllText("Temp/WispCourtGridInspection.txt", report.ToString());
        Debug.Log("Wisp Court grid inspection ready.");
    }

    private static string PathOf(Transform t) { return t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name; }
}
