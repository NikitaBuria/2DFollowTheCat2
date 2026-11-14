using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class LimestoneConnectionsBuilder
{
    private const string Root = LimestoneStructureBrush.KitRoot;
    private const string Source = "Assets/Art/Map/Dungeon/Limestone128";
    private const int Size = 128;
    private static readonly Vector3Int[] Directions = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
    private sealed class Piece
    {
        public string name;
        public Color32[] pixels;
        public Vector3Int palette;
        public Sprite sprite;
        public Tile tile;
    }
    private static readonly List<Piece> Pieces = new List<Piece>();
    private static readonly Dictionary<string, Color32[]> Sources = new Dictionary<string, Color32[]>();
    private static readonly StringBuilder Analysis = new StringBuilder();

    static LimestoneConnectionsBuilder() { EditorApplication.delayCall += Run; }
    private static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            if (!File.Exists(Root + "/Verified.txt")) Build();
            if (!File.Exists(Root + "/EditorVerified.txt")) VerifyEditorTarget();
        }
        catch (Exception error) { File.WriteAllText("Temp/LimestoneConnectionsFailure.txt", error.ToString()); Debug.LogException(error); }
    }

    private static void VerifyEditorTarget()
    {
        var map = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.name == LimestoneStructureBrush.WallLayer && candidate.gameObject.scene.name == "Dungeon");
        if (map == null) return;
        var brush = AssetDatabase.LoadAssetAtPath<LimestoneStructureBrush>(Root + "/WallsAndStairsBrush.asset");
        var editor = Editor.CreateEditor(brush) as LimestoneStructureBrushEditor;
        try
        {
            if (editor == null || !editor.validTargets.Contains(map.gameObject))
                throw new InvalidOperationException("Custom brush editor must expose the live Limestone Walls target.");
        }
        finally { if (editor != null) UnityEngine.Object.DestroyImmediate(editor); }
        GridPaintingState.gridBrush = brush;
        GridPaintingState.scenePaintTarget = map.gameObject;
        Selection.activeGameObject = map.gameObject;
        File.WriteAllText(Root + "/EditorVerified.txt", "Custom brush editor binding verified. Live Limestone Walls Tilemap is a valid painting target; selected in Tile Palette.\n");
        AssetDatabase.Refresh();
    }

    public static void Build()
    {
        var paletteReference = AssetDatabase.LoadAssetAtPath<GameObject>(Source + "/Dungeon Limestone128.prefab");
        var importerReference = (TextureImporter)AssetImporter.GetAtPath(Source + "/Limestone128.png");
        if (paletteReference == null || paletteReference.GetComponent<Grid>().cellSize != Vector3.one || importerReference.spritePixelsPerUnit != Size)
            throw new InvalidOperationException("Dungeon Limestone128 must use 128 PPU and a unit Grid.");
        Pieces.Clear(); Sources.Clear(); Analysis.Clear();
        Directory.CreateDirectory(Root + "/Tiles");
        foreach (string path in Directory.GetFiles(Source + "/Tiles", "*.asset"))
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path.Replace('\\', '/'));
            if (tile == null || tile.sprite == null) continue;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(tile.sprite)));
                var r = tile.sprite.rect;
                Sources[tile.name] = texture.GetPixels((int)r.x, (int)r.y, Size, Size).Select(c => (Color32)c).ToArray();
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        for (int mask = 0; mask < 16; mask++)
        {
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++)
            {
                if (y >= 40 && y < 88 && ((x >= 64 && (mask & 2) != 0) || (x < 64 && (mask & 8) != 0)))
                {
                    var colour = Sample("WallHorizontal", 32 + x % 64, 38 + (y - 40) * 44 / 48);
                    var edge = Sample("WallHorizontal", 64, 38 + (y - 40) * 44 / 48);
                    float weight = Mathf.Clamp01(Mathf.Min(x, Size - 1 - x) / 8f);
                    pixels[y * Size + x] = Color32.Lerp(edge, colour, weight);
                }
                if (x >= 48 && x < 80 && ((y >= 64 && (mask & 1) != 0) || (y < 64 && (mask & 4) != 0)))
                {
                    var colour = Sample("WallVertical", 48 + (x - 48) * 24 / 32, 16 + y % 96);
                    var edge = Sample("WallVertical", 48 + (x - 48) * 24 / 32, 64);
                    float weight = Mathf.Clamp01(Mathf.Min(y, Size - 1 - y) / 8f);
                    pixels[y * Size + x] = Color32.Lerp(edge, colour, weight);
                }
                if (x >= 40 && x < 88 && y >= 28 && y < 100)
                {
                    var post = Sample("PillarA", 44 + (x - 40) * 40 / 48, 8 + (y - 28) * 112 / 72);
                    Over(ref pixels[y * Size + x], post);
                }
            }
            Add("Wall_" + mask.ToString("00"), pixels, new Vector3Int(-100, -100, 0));
        }
        CheckConnectionPixels();
        CreateStairsAndLandings();
        CreateGate();
        string[] structures = { "PillarA", "PillarB", "DiamondPillar", "WallShort", "WallArches", "WallLong", "WallPanel", "WallBroken", "BrokenArchLeft", "BrokenArchRight", "ColumnBroken", "RubbleA", "RubbleB", "RubbleC", "GhostFlame", "PurpleCrystals" };
        for (int i = 0; i < structures.Length; i++)
        {
            string source = structures[i];
            bool socket = i < 11;
            var pixels = (Color32[])Sources[source].Clone();
            // Reserve all cell boundaries for the common RuleTile connectors underneath the decoration.
            for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++)
                if (Mathf.Min(x, y, Size - 1 - x, Size - 1 - y) < 8) pixels[y * Size + x] = default;
            Add("Structure_" + source + (socket ? "_Socket" : "_Decor"), pixels,
                new Vector3Int(i % 8 * 3, -19 - i / 8 * 3, 0));
        }

        ImportSprites();
        var wall = AssetDatabase.LoadAssetAtPath<RuleTile>(Root + "/ConnectedWall.asset");
        if (wall == null) { wall = ScriptableObject.CreateInstance<RuleTile>(); AssetDatabase.CreateAsset(wall, Root + "/ConnectedWall.asset"); }
        wall.m_DefaultSprite = Pieces[0].sprite;
        wall.m_DefaultColliderType = Tile.ColliderType.None;
        wall.m_TilingRules.Clear();
        // Colour-grid analysis is retained for inspection; authored socket topology decides masonry connectivity.
        // Exact absent-neighbour conditions keep ends, corners and junctions mutually exclusive.
        foreach (int mask in Enumerable.Range(0, 16).OrderByDescending(BitCount))
        {
            var rule = new RuleTile.TilingRule {
                m_Sprites = new[] { Pieces[mask].sprite }, m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_RuleTransform = RuleTile.TilingRuleOutput.Transform.Fixed, m_ColliderType = Tile.ColliderType.None
            };
            rule.m_Neighbors.Clear(); rule.m_NeighborPositions.Clear();
            for (int d = 0; d < 4; d++)
            {
                rule.m_NeighborPositions.Add(Directions[d]);
                rule.m_Neighbors.Add((mask & (1 << d)) != 0 ? RuleTile.TilingRuleOutput.Neighbor.This : RuleTile.TilingRuleOutput.Neighbor.NotThis);
            }
            wall.m_TilingRules.Add(rule);
        }
        wall.UpdateNeighborPositions(); EditorUtility.SetDirty(wall);
        for (int i = 16; i < Pieces.Count; i++)
        {
            var piece = Pieces[i]; string path = Root + "/Tiles/" + piece.name + ".asset";
            piece.tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (piece.tile == null) { piece.tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(piece.tile, path); }
            piece.tile.sprite = piece.sprite; piece.tile.colliderType = Tile.ColliderType.None;
            piece.tile.color = Color.white; piece.tile.transform = Matrix4x4.identity; EditorUtility.SetDirty(piece.tile);
        }
        var brush = AssetDatabase.LoadAssetAtPath<LimestoneStructureBrush>(Root + "/WallsAndStairsBrush.asset");
        if (brush == null) { brush = ScriptableObject.CreateInstance<LimestoneStructureBrush>(); AssetDatabase.CreateAsset(brush, Root + "/WallsAndStairsBrush.asset"); }
        brush.size = Vector3Int.one; brush.pivot = Vector3Int.zero; brush.cells[0].tile = wall; EditorUtility.SetDirty(brush);
        CreatePalette(wall);
        AssetDatabase.SaveAssets();
        VerifyPainting(wall, brush);
        File.WriteAllText(Root + "/SpriteAnalysis.txt", Analysis.ToString());
        var target = PrepareSceneTarget();
        File.WriteAllText(Root + "/Verified.txt", "128px / 128 PPU / unit grid, matching Dungeon Limestone128.\n16 exact NESW wall rules checked in Unity; 128 connected edge pairings checked pixel-for-pixel.\nActual multi-layer brush painting, wall-to-stair sockets, north/east stairs, landings, erase and neighbour refresh checked in a preview scene.\nStructures use shared wall sockets; stairs and gates are multi-cell stamps. Collision/gameplay untested. No gameplay scripts changed.\n");
        AssetDatabase.Refresh();
        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        GridPaintingState.palette = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Dungeon Structures128.prefab");
        GridPaintingState.gridBrush = brush;
        if (target != null) { Selection.activeGameObject = target.gameObject; GridPaintingState.scenePaintTarget = target.gameObject; }
        Debug.Log("Dungeon Structures128 ready: connected walls, compatible stair rails and landings. Use the Limestone Walls and Stairs brush on Limestone Walls.");
    }

    private static void CreateStairsAndLandings()
    {
        var north = new Color32[384 * 256];
        for (int y = 0; y < 256; y++) for (int x = 0; x < 384; x++)
        {
            var floor = Floor(x, y);
            if (x >= 64 && x < 320)
            {
                var step = Sample("StairsLong", 8 + (x - 64) * 112 / 256, 8 + y * 112 / 256);
                float blend = Mathf.Clamp01(Mathf.Min(y, 255 - y) / 10f);
                if (step.a > 180) floor = Color32.Lerp(floor, step, blend);
            }
            north[y * 384 + x] = floor;
        }
        AddStamp("StairNorth", north, 3, 2, new Vector3Int(0, -13, 0), (x, y) => x == 0 || x == 2);
        var east = new Color32[256 * 384];
        for (int y = 0; y < 384; y++) for (int x = 0; x < 256; x++) east[y * 256 + x] = north[(255 - x) * 384 + y];
        AddStamp("StairEast", east, 2, 3, new Vector3Int(9, -13, 0), (x, y) => y == 0 || y == 2);
        var landing = new Color32[384 * 384];
        for (int y = 0; y < 384; y++) for (int x = 0; x < 384; x++) landing[y * 384 + x] = Floor(x, y);
        AddStamp("LandingStraight", landing, 3, 3, new Vector3Int(4, -13, 0), (x, y) => x == 0 || x == 2);
        AddStamp("LandingTurnEast", landing, 3, 3, new Vector3Int(13, -13, 0), (x, y) => x == 0 || y == 2 || (x == 2 && y == 0));
    }

    private static void CreateGate()
    {
        var gate = new Color32[384 * 256];
        for (int y = 0; y < 256; y++) for (int x = 0; x < 384; x++)
        {
            if (x < 8 || x >= 376 || y < 8 || y >= 248) continue;
            gate[y * 384 + x] = Sample("DoorArch", x * Size / 384, y * Size / 256);
        }
        AddStamp("GateArch", gate, 3, 2, new Vector3Int(18, -13, 0), (x, y) => y == 0 && (x == 0 || x == 2));
    }

    private static void AddStamp(string name, Color32[] pixels, int width, int height, Vector3Int origin, Func<int, int, bool> socket)
    {
        for (int cy = 0; cy < height; cy++) for (int cx = 0; cx < width; cx++)
        {
            var tile = new Color32[Size * Size];
            for (int y = 0; y < Size; y++) Array.Copy(pixels, (cy * Size + y) * width * Size + cx * Size, tile, y * Size, Size);
            Add(name + "_" + cx + "_" + cy + (socket(cx, cy) ? "_Socket" : "_Open"), tile, origin + new Vector3Int(cx, cy, 0));
        }
    }

    private static Color32 Floor(int x, int y)
    {
        int px = x % Size, py = y % Size;
        return Color32.Lerp(new Color32(173, 164, 141, 255), Sample("Floor_Clean_PavingA", px, py),
            Mathf.Clamp01(Mathf.Min(px, py, Size - 1 - px, Size - 1 - py) / 10f));
    }
    private static Color32 Sample(string name, int x, int y) { return Sources[name][Mathf.Clamp(y, 0, 127) * Size + Mathf.Clamp(x, 0, 127)]; }
    private static void Over(ref Color32 bottom, Color32 top) { if (top.a == 0) return; float a = top.a / 255f; var c = Color.Lerp((Color)bottom, (Color)top, a); c.a = a + bottom.a / 255f * (1 - a); bottom = c; }
    private static int BitCount(int mask) { int result = 0; for (; mask != 0; mask >>= 1) result += mask & 1; return result; }
    private static void Add(string name, Color32[] pixels, Vector3Int palette) { Pieces.Add(new Piece { name = name, pixels = pixels, palette = palette }); }

    private static void CheckConnectionPixels()
    {
        for (int a = 0; a < 16; a++) for (int b = 0; b < 16; b++)
        {
            if ((a & 2) != 0 && (b & 8) != 0)
                for (int y = 0; y < Size; y++) if (!Pieces[a].pixels[y * Size + 127].Equals(Pieces[b].pixels[y * Size])) throw new InvalidDataException("Horizontal masonry edge mismatch.");
            if ((a & 1) != 0 && (b & 4) != 0)
                for (int x = 0; x < Size; x++) if (!Pieces[a].pixels[127 * Size + x].Equals(Pieces[b].pixels[x])) throw new InvalidDataException("Vertical masonry edge mismatch.");
        }
    }

    private static void ImportSprites()
    {
        int height = Mathf.CeilToInt(Pieces.Count / 8f) * Size;
        var atlas = new Texture2D(1024, height, TextureFormat.RGBA32, false);
        var pixels = new Color32[1024 * height];
        for (int i = 0; i < Pieces.Count; i++) for (int y = 0; y < Size; y++)
            Array.Copy(Pieces[i].pixels, y * Size, pixels, (i / 8 * Size + y) * 1024 + i % 8 * Size, Size);
        string path = Root + "/ConnectedStructures128.png";
        try { atlas.SetPixels32(pixels); atlas.Apply(); File.WriteAllBytes(path, atlas.EncodeToPNG()); }
        finally { UnityEngine.Object.DestroyImmediate(atlas); }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = Size; importer.maxTextureSize = 2048;
        importer.npotScale = TextureImporterNPOTScale.None; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true; importer.isReadable = true; importer.wrapMode = TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        var factories = new SpriteDataProviderFactories(); factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null) throw new InvalidOperationException("Sprite Editor provider missing.");
        provider.InitSpriteEditorDataProvider();
        var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (capability == null) throw new InvalidOperationException("Sprite Editor capability provider missing; slicing aborted.");
        foreach (var required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect, EEditCapability.EditSpriteName, EEditCapability.EditPivot })
            if (!capability.GetEditCapability().HasCapability(required)) throw new InvalidOperationException("Sprite Editor does not support " + required);
        var old = provider.GetSpriteRects().ToDictionary(rect => rect.name);
        var rects = Pieces.Select((piece, i) => new SpriteRect { name = piece.name, rect = new Rect(i % 8 * Size, i / 8 * Size, Size, Size),
            alignment = SpriteAlignment.Center, pivot = new Vector2(.5f, .5f), spriteID = old.TryGetValue(piece.name, out var existing) ? existing.spriteID : GUID.Generate() }).ToArray();
        provider.SetSpriteRects(rects);
        var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (names == null) throw new InvalidOperationException("Sprite Editor file-ID provider missing.");
        names.SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
        provider.Apply(); importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        foreach (var piece in Pieces)
        {
            piece.sprite = sprites[piece.name];
            if (piece.sprite.rect.size != new Vector2(Size, Size) || piece.sprite.bounds.size.x != 1 || piece.sprite.bounds.size.y != 1)
                throw new InvalidDataException("Invalid structure sprite dimensions: " + piece.name);
            Analysis.AppendLine(piece.name + ": " + Analyze3x3(piece.sprite) + (piece.name.StartsWith("Wall_") ? " ; authored NESW mask=" + piece.name.Substring(5) : ""));
        }
    }

    private static string Analyze3x3(Sprite sprite)
    {
        var r = sprite.rect; var pixels = sprite.texture.GetPixels32(); int width = sprite.texture.width;
        var bins = new Dictionary<int, int>();
        for (int y = 43; y < 85; y++) for (int x = 43; x < 85; x++)
        {
            var c = pixels[((int)r.y + y) * width + (int)r.x + x];
            int key = (c.r / 16) | (c.g / 16 << 4) | (c.b / 16 << 8) | (c.a / 16 << 12);
            bins[key] = bins.TryGetValue(key, out int count) ? count + 1 : 1;
        }
        int majority = bins.OrderByDescending(pair => pair.Value).First().Key;
        var rows = new List<string>();
        for (int gy = 2; gy >= 0; gy--)
        {
            var cells = new List<string>();
            for (int gx = 0; gx < 3; gx++)
            {
                if (gx == 1 && gy == 1) { cells.Add("*"); continue; }
                int match = 0, total = 0;
                for (int y = gy * Size / 3; y < (gy + 1) * Size / 3; y++) for (int x = gx * Size / 3; x < (gx + 1) * Size / 3; x++)
                {
                    var c = pixels[((int)r.y + y) * width + (int)r.x + x]; int key = c.r / 16 | c.g / 16 << 4 | c.b / 16 << 8 | c.a / 16 << 12;
                    if (key == majority) match++; total++;
                }
                cells.Add(match / (float)total >= .75f ? "." : "X");
            }
            rows.Add(string.Join(" ", cells));
        }
        return string.Join(" / ", rows);
    }

    private static void CreatePalette(RuleTile wall)
    {
        string path = Root + "/Dungeon Structures128.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            GridPaletteUtility.CreateNewPalette(Root, "Dungeon Structures128", GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Manual, Vector3.one, GridLayout.CellSwizzle.XYZ);
        var go = PrefabUtility.LoadPrefabContents(path);
        try
        {
            go.GetComponent<Grid>().cellSize = Vector3.one;
            var map = go.GetComponentInChildren<Tilemap>(); map.ClearAllTiles();
            map.SetTile(Vector3Int.zero, wall);
            for (int x = 0; x < 4; x++) map.SetTile(new Vector3Int(x, -4, 0), wall);
            for (int x = 6; x <= 11; x++) { map.SetTile(new Vector3Int(x, -3, 0), wall); map.SetTile(new Vector3Int(x, -7, 0), wall); }
            for (int y = -6; y <= -4; y++) { map.SetTile(new Vector3Int(6, y, 0), wall); map.SetTile(new Vector3Int(11, y, 0), wall); }
            foreach (var piece in Pieces.Skip(16)) map.SetTile(piece.palette, piece.tile);
            map.RefreshAllTiles(); map.CompressBounds(); PrefabUtility.SaveAsPrefabAsset(go, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(go); }
    }

    private static void SetStamp(LimestoneStructureBrush brush, string prefix, int width, int height)
    {
        brush.size = new Vector3Int(width, height, 1); brush.pivot = Vector3Int.zero;
        foreach (var piece in Pieces.Where(piece => piece.name.StartsWith(prefix + "_", StringComparison.Ordinal)))
        {
            string[] parts = piece.name.Split('_'); int x = int.Parse(parts[1]), y = int.Parse(parts[2]);
            brush.cells[brush.GetCellIndex(x, y, 0)].tile = piece.tile;
        }
    }

    private static void VerifyPainting(RuleTile wall, LimestoneStructureBrush brush)
    {
        var scene = EditorSceneManager.NewPreviewScene();
        try
        {
            var gridObject = new GameObject("LimestoneConnectionTests", typeof(Grid));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gridObject, scene);
            gridObject.GetComponent<Grid>().cellSize = Vector3.one;
            var go = new GameObject(LimestoneStructureBrush.WallLayer, typeof(Tilemap), typeof(TilemapRenderer)); go.transform.SetParent(gridObject.transform, false);
            var map = go.GetComponent<Tilemap>();
            for (int mask = 0; mask < 16; mask++)
            {
                map.ClearAllTiles(); map.SetTile(Vector3Int.zero, wall);
                for (int d = 0; d < 4; d++) if ((mask & 1 << d) != 0) map.SetTile(Directions[d], wall);
                map.RefreshAllTiles();
                if (map.GetSprite(Vector3Int.zero) != Pieces[mask].sprite) throw new InvalidDataException("RuleTile neighbour test failed for " + mask);
            }
            map.ClearAllTiles(); SetStamp(brush, "StairNorth", 3, 2);
            brush.Paint(gridObject.GetComponent<Grid>(), go, Vector3Int.zero);
            var floors = LimestoneStructureBrush.Companion(map, " Floors", -1);
            if (floors.GetUsedTilesCount() != 6 || map.HasTile(Vector3Int.right) || map.GetTile(Vector3Int.zero) != wall)
                throw new InvalidDataException("North stair socket or opening placement failed.");
            map.SetTile(Vector3Int.left, wall); map.RefreshAllTiles();
            if (map.GetSprite(Vector3Int.zero) != Pieces[9].sprite) throw new InvalidDataException("Wall-to-stair corner did not connect.");
            map.SetTile(Vector3Int.left, null); map.RefreshAllTiles();
            if (map.GetSprite(Vector3Int.zero) != Pieces[1].sprite) throw new InvalidDataException("Erase did not refresh the stair rail end.");
            brush.Erase(gridObject.GetComponent<Grid>(), go, Vector3Int.zero);
            if (floors.HasTile(Vector3Int.zero) || map.HasTile(Vector3Int.zero)) throw new InvalidDataException("Multi-layer stair erase failed.");
            SetStamp(brush, "StairEast", 2, 3); brush.Paint(gridObject.GetComponent<Grid>(), go, new Vector3Int(5, 0, 0));
            if (map.GetTile(new Vector3Int(5, 0, 0)) != wall || map.HasTile(new Vector3Int(5, 1, 0))) throw new InvalidDataException("East stair rail layout failed.");
            map.ClearAllTiles(); floors.ClearAllTiles();
            for (int x = 0; x <= 11; x++) { map.SetTile(new Vector3Int(x, 0, 0), wall); map.SetTile(new Vector3Int(x, 8, 0), wall); }
            for (int y = 1; y < 8; y++) { map.SetTile(new Vector3Int(0, y, 0), wall); map.SetTile(new Vector3Int(11, y, 0), wall); }
            SetStamp(brush, "StairNorth", 3, 2); brush.Paint(gridObject.GetComponent<Grid>(), go, new Vector3Int(4, 0, 0)); brush.Paint(gridObject.GetComponent<Grid>(), go, new Vector3Int(4, 2, 0));
            SetStamp(brush, "LandingTurnEast", 3, 3); brush.Paint(gridObject.GetComponent<Grid>(), go, new Vector3Int(4, 4, 0));
            SetStamp(brush, "StairEast", 2, 3); brush.Paint(gridObject.GetComponent<Grid>(), go, new Vector3Int(7, 4, 0));
            map.SetTile(new Vector3Int(9, 4, 0), wall); map.SetTile(new Vector3Int(9, 6, 0), wall);
            map.RefreshAllTiles();
            if (map.GetSprite(new Vector3Int(4, 3, 0)) != Pieces[5].sprite)
                throw new InvalidDataException("Consecutive stair stamps did not produce a continuous rail.");
            if (map.GetSprite(new Vector3Int(4, 6, 0)) != Pieces[6].sprite || map.GetSprite(new Vector3Int(7, 4, 0)) != Pieces[10].sprite)
                throw new InvalidDataException("Stair landing turn did not join the horizontal stair rails.");
            WritePreview(map, floors, LimestoneStructureBrush.Companion(map, " Details", 1));
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); brush.size = Vector3Int.one; brush.pivot = Vector3Int.zero; brush.cells[0].tile = wall; EditorUtility.SetDirty(brush); AssetDatabase.SaveAssets(); }
    }

    private static void WritePreview(Tilemap walls, Tilemap floors, Tilemap details)
    {
        const int width = 12 * Size, height = 9 * Size;
        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) pixels[y * width + x] = Floor(x, y);
        foreach (var map in new[] { floors, walls, details })
        for (int cy = 0; cy < 9; cy++) for (int cx = 0; cx < 12; cx++)
        {
            var sprite = map.GetSprite(new Vector3Int(cx, cy, 0)); if (sprite == null) continue;
            var source = Pieces.First(piece => piece.sprite == sprite).pixels;
            for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++) Over(ref pixels[(cy * Size + y) * width + cx * Size + x], source[y * Size + x]);
        }
        var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
        try { output.SetPixels32(pixels); output.Apply(); File.WriteAllBytes("Temp/LimestoneConnectionPreview.png", output.EncodeToPNG()); }
        finally { UnityEngine.Object.DestroyImmediate(output); }
    }

    private static Tilemap PrepareSceneTarget()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "Dungeon") return null;
        var grid = UnityEngine.Object.FindObjectsByType<Grid>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.gameObject.scene == scene && candidate.name == "Wisp Court" && candidate.cellSize == Vector3.one && !PrefabUtility.IsPartOfPrefabInstance(candidate));
        if (grid == null)
        {
            var origin = UnityEngine.Object.FindObjectsByType<Grid>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.gameObject.scene == scene && candidate.cellSize == Vector3.one && candidate.transform.lossyScale == Vector3.one);
            var go = new GameObject("Limestone Structures Grid", typeof(Grid));
            Undo.RegisterCreatedObjectUndo(go, "Add unit-sized limestone structure grid");
            if (origin != null) go.transform.position = origin.transform.position;
            grid = go.GetComponent<Grid>(); grid.cellSize = Vector3.one;
        }
        var child = grid.transform.Find(LimestoneStructureBrush.WallLayer);
        Tilemap map;
        if (child != null) map = child.GetComponent<Tilemap>();
        else
        {
            var go = new GameObject(LimestoneStructureBrush.WallLayer, typeof(Tilemap), typeof(TilemapRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Add connected limestone painting layer"); go.transform.SetParent(grid.transform, false);
            map = go.GetComponent<Tilemap>();
            var renderer = go.GetComponent<TilemapRenderer>(); var art = grid.GetComponentInChildren<TilemapRenderer>();
            if (art != null) { renderer.sharedMaterial = art.sharedMaterial; renderer.sortingLayerID = art.sortingLayerID; renderer.sortingOrder = art.sortingOrder + 10; }
        }
        LimestoneStructureBrush.Companion(map, " Floors", -1); LimestoneStructureBrush.Companion(map, " Details", 1);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the new limestone painting layers.");
        return map;
    }
}
