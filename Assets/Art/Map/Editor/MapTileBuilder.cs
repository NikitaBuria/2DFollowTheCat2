using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

// Slices Assets/Map/RuinsMap.png into a 64px grid, makes a Tile per cell,
// builds a Tile Palette, and paints the full map into the open scene.
public static class MapTileBuilder
{
    const string TexturePath = "Assets/Map/RuinsMap.png";
    const string TilesFolder = "Assets/Map/Tiles";
    const string PaletteFolder = "Assets/Map/Palettes";
    const string PaletteName = "RuinsMapPalette";
    const int TileSize = 64;

    [MenuItem("Tools/Map/Build Ruins Map Tiles")]
    public static void Build()
    {
        var tiles = SliceAndCreateTiles(out int cols, out int rows);
        BuildPalette(tiles, cols, rows);
        PaintScene(tiles, cols, rows);
        AssetDatabase.SaveAssets();
        Debug.Log($"Ruins map built: {cols}x{rows} tiles.");
    }

    static Tile[,] SliceAndCreateTiles(out int cols, out int rows)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = TileSize;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        cols = tex.width / TileSize;
        rows = tex.height / TileSize;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var rects = new List<SpriteRect>();
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                rects.Add(new SpriteRect
                {
                    name = $"RuinsMap_{x}_{y}",
                    rect = new Rect(x * TileSize, y * TileSize, TileSize, TileSize),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate()
                });
        provider.SetSpriteRects(rects.ToArray());
        var nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider?.SetNameFileIdPairs(rects.ConvertAll(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();

        var sprites = new Dictionary<string, Sprite>();
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(TexturePath))
            if (o is Sprite s) sprites[s.name] = s;

        Directory.CreateDirectory(TilesFolder);
        var tiles = new Tile[cols, rows];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                string name = $"RuinsMap_{x}_{y}";
                string path = $"{TilesFolder}/{name}.asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, path);
                }
                tile.sprite = sprites[name];
                EditorUtility.SetDirty(tile);
                tiles[x, y] = tile;
            }
        return tiles;
    }

    static void BuildPalette(Tile[,] tiles, int cols, int rows)
    {
        Directory.CreateDirectory(PaletteFolder);
        string path = $"{PaletteFolder}/{PaletteName}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            GridPaletteUtility.CreateNewPalette(PaletteFolder, PaletteName,
                GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Automatic,
                Vector3.one, GridLayout.CellSwizzle.XYZ);

        var root = PrefabUtility.LoadPrefabContents(path);
        var tilemap = root.GetComponentInChildren<Tilemap>();
        tilemap.ClearAllTiles();
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[x, y]);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void PaintScene(Tile[,] tiles, int cols, int rows)
    {
        var grid = GameObject.Find("RuinsMapGrid");
        if (grid == null)
        {
            grid = new GameObject("RuinsMapGrid", typeof(Grid));
            var child = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
            child.transform.SetParent(grid.transform, false);
            child.GetComponent<TilemapRenderer>().sortingOrder = -10;
        }
        var tilemap = grid.GetComponentInChildren<Tilemap>();
        tilemap.ClearAllTiles();
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                tilemap.SetTile(new Vector3Int(x - cols / 2, y - rows / 2, 0), tiles[x, y]);
        EditorSceneManager.MarkSceneDirty(grid.scene);
    }
}
