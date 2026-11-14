using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

// Asset preparation only. Does not change scenes or gameplay scripts.
public static class DungeonLimestonePaletteBuilder
{
    private const string Source = "Assets/Art/Map/Dungeon/Seamless limestone dungeon tile atlas.png";
    private const string Root = "Assets/Art/Map/Dungeon/Limestone128";
    private const string Sheet = Root + "/Limestone128.png";
    private const string PalettePath = Root + "/Dungeon Limestone128.prefab";
    private const int Cell = 128;
    private const int Columns = 8;

    [InitializeOnLoadMethod]
    private static void QueueBuild()
    {
        if (File.Exists(Source) && (!File.Exists(Root + "/Verified.txt") || !File.ReadAllText(Root + "/Verified.txt").Contains("Manual unit grid")))
            EditorApplication.delayCall += BuildWhenReady;
    }

    private static void BuildWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;
        try { Build(); }
        catch (Exception error) { Debug.LogException(error); }
    }

    private static string TileName(int row, int column)
    {
        string[] floors = { "Clean", "Cracked", "Shadow", "Corrupted" };
        string[] patterns = { "PavingA", "PavingB", "Studded", "PavingC", "Diamond", "Arc", "PavingD", "Inlay" };
        string[] structures = {
            "WallHorizontal", "WallVertical", "CornerNW", "CornerNE", "CornerSW", "CornerSE", "JunctionSouth", "JunctionNorth",
            "Cross", "EndLeft", "EndRight", "PillarA", "PillarB", "WallShort", "DiamondPillar", "WallArches",
            "WallLong", "DoorArch", "BrokenArchLeft", "BrokenArchRight", "WallPanel", "WallBroken", "RubbleA", "RubbleB",
            "StairsA", "StairsB", "StairsC", "StairsLong", "ColumnBroken", "RubbleC", "GhostFlame", "PurpleCrystals"
        };
        return row < 4 ? "Floor_" + floors[row] + "_" + patterns[column] : structures[(row - 4) * Columns + column];
    }

    [MenuItem("Tools/Tiles/Build Dungeon Limestone Palette")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play mode before preparing the palette.");
        foreach (var window in Resources.FindObjectsOfTypeAll<GridPaintPaletteWindow>()) window.Close();
        Directory.CreateDirectory(Root + "/Tiles");
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        var normalized = new Texture2D(Cell * Columns, Cell * Columns, TextureFormat.RGBA32, false);
        try
        {
            if (!source.LoadImage(File.ReadAllBytes(Source))) throw new InvalidDataException("Cannot read dungeon PNG.");
            if (source.width != 1254 || source.height != 1254)
                throw new InvalidDataException("Expected the inspected 1254 x 1254 dungeon atlas.");
            // Normalize each rounded source cell separately so every imported sprite is exactly one unit.
            var output = new Color32[normalized.width * normalized.height];
            for (int row = 0; row < Columns; row++)
            for (int col = 0; col < Columns; col++)
            {
                int left = Mathf.RoundToInt(col * source.width / (float)Columns);
                int right = Mathf.RoundToInt((col + 1) * source.width / (float)Columns);
                int top = Mathf.RoundToInt(row * source.height / (float)Columns);
                int bottom = Mathf.RoundToInt((row + 1) * source.height / (float)Columns);
                for (int y = 0; y < Cell; y++)
                for (int x = 0; x < Cell; x++)
                {
                    int sx = left + Mathf.Min(right - left - 1, Mathf.FloorToInt((x + 0.5f) * (right - left) / Cell));
                    int sy = source.height - 1 - top - Mathf.Min(bottom - top - 1, Mathf.FloorToInt((y + 0.5f) * (bottom - top) / Cell));
                    output[(Columns * Cell - 1 - row * Cell - y) * Columns * Cell + col * Cell + x] = source.GetPixel(sx, sy);
                }
            }
            normalized.SetPixels32(output);
            normalized.Apply();
            File.WriteAllBytes(Sheet, normalized.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(normalized);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = Cell;
        importer.maxTextureSize = 2048;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null) throw new InvalidOperationException("Sprite Editor data provider unavailable.");
        provider.InitSpriteEditorDataProvider();
        var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (edit == null) throw new InvalidOperationException("Sprite Editor edit capability unavailable; slicing aborted.");
        var capabilities = edit.GetEditCapability();
        foreach (var required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect, EEditCapability.EditSpriteName, EEditCapability.EditPivot })
            if (!capabilities.HasCapability(required)) throw new InvalidOperationException("Sprite Editor does not support " + required + "; slicing aborted.");
        var existing = provider.GetSpriteRects().ToDictionary(rect => rect.name);
        var rectangles = new SpriteRect[Columns * Columns];
        for (int row = 0; row < Columns; row++)
        for (int col = 0; col < Columns; col++)
        {
            string name = TileName(row, col);
            rectangles[row * Columns + col] = new SpriteRect {
                name = name, rect = new Rect(col * Cell, (Columns - 1 - row) * Cell, Cell, Cell),
                alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f),
                spriteID = existing.TryGetValue(name, out var old) ? old.spriteID : GUID.Generate()
            };
        }
        provider.SetSpriteRects(rectangles);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
            rectangles.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        if (sprites.Count != 64) throw new InvalidDataException("Expected 64 sliced sprites, found " + sprites.Count);

        var palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        if (palette == null)
            palette = GridPaletteUtility.CreateNewPalette(Root, "Dungeon Limestone128", GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Manual, Vector3.one, GridLayout.CellSwizzle.XYZ);
        var paletteSettings = AssetDatabase.LoadAllAssetsAtPath(PalettePath).OfType<GridPalette>().Single();
        paletteSettings.cellSizing = GridPalette.CellSizing.Manual;
        EditorUtility.SetDirty(paletteSettings);
        var content = PrefabUtility.LoadPrefabContents(PalettePath);
        try
        {
            content.GetComponent<Grid>().cellSize = Vector3.one;
            var tilemap = content.GetComponentInChildren<Tilemap>();
            tilemap.ClearAllTiles();
            for (int row = 0; row < Columns; row++)
            for (int col = 0; col < Columns; col++)
            {
                string name = TileName(row, col);
                string path = Root + "/Tiles/" + name + ".asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, path); }
                tile.sprite = sprites[name];
                // Stairs and decorative props remain walkable; wall pieces carry sprite collision when painted on a collider Tilemap.
                tile.colliderType = row >= 4 && row < 7 && name != "DoorArch" ? Tile.ColliderType.Sprite : Tile.ColliderType.None;
                tile.color = Color.white;
                tile.transform = Matrix4x4.identity;
                EditorUtility.SetDirty(tile);
                tilemap.SetTile(new Vector3Int(col, -row, 0), tile);
            }
            tilemap.RefreshAllTiles();
            tilemap.CompressBounds();
            PrefabUtility.SaveAsPrefabAsset(content, PalettePath);
        }
        finally { PrefabUtility.UnloadPrefabContents(content); }
        AssetDatabase.SaveAssets();

        palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        if (!AssetDatabase.LoadAllAssetsAtPath(PalettePath).OfType<GridPalette>().Any())
            throw new InvalidDataException("Palette is missing its GridPalette sub-asset.");
        var savedMap = palette.GetComponentInChildren<Tilemap>();
        int count = 0;
        foreach (var position in savedMap.cellBounds.allPositionsWithin)
        {
            var tile = savedMap.GetTile<Tile>(position);
            if (tile == null) continue;
            if (tile.sprite == null || tile.sprite.rect.size != new Vector2(Cell, Cell) || tile.sprite.pixelsPerUnit != Cell)
                throw new InvalidDataException("Invalid sprite binding: " + tile.name);
            count++;
        }
        if (count != 64) throw new InvalidDataException("Palette must contain 64 paintable cells.");
        File.WriteAllText(Root + "/Verified.txt", "Verified in Unity Editor: 64 sprites, 128 x 128 px, 128 PPU; 64 Tile assets; rectangular palette with GridPalette sub-asset. Manual unit grid.\nRows 1-4: floors. Rows 5-8: structures, stairs and props.\nArt is manually placed tiles, not RuleTiles. Source artwork edge continuity and gameplay have not been verified.\n");
        AssetDatabase.Refresh();
        Selection.activeObject = palette;
        EditorGUIUtility.PingObject(palette);
        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        GridPaintingState.palette = palette;
        Debug.Log("Dungeon Limestone128 ready: 64 verified paintable tiles. Top four rows are floors; bottom four rows are structures and props.");
    }
}
