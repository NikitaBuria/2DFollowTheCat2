using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Slices GroundTiles.png on an exact 64px grid and sets both sheets to 64 pixels per unit,
// so one ground tile fills one Tilemap cell.
public static class GroundTilesImporter
{
    const int TileSize = 64;

    [MenuItem("Tools/Map/Reimport Ground Tiles")]
    public static void Run()
    {
        var env = (TextureImporter)AssetImporter.GetAtPath("Assets/Map/EnvironmentTileset.png");
        env.spritePixelsPerUnit = TileSize;
        env.SaveAndReimport();

        const string path = "Assets/Map/GroundTiles.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = TileSize;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int cols = tex.width / TileSize, rows = tex.height / TileSize;
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var rects = new List<SpriteRect>();
        for (int y = rows - 1; y >= 0; y--)
            for (int x = 0; x < cols; x++)
                rects.Add(new SpriteRect
                {
                    name = $"Ground_{rects.Count}",
                    rect = new Rect(x * TileSize, y * TileSize, TileSize, TileSize),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate()
                });
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?
            .SetNameFileIdPairs(rects.ConvertAll(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
    }
}
