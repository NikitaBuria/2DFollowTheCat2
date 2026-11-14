using System;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

// Editor painting tool only. Finished maps use Unity's built-in RuleTile and Tile assets.
[CustomGridBrush(false, false, false, "Limestone Walls and Stairs")]
public class LimestoneStructureBrush : GridBrush
{
    public const string KitRoot = "Assets/Art/Map/Dungeon/LimestoneStructures128";
    public const string WallLayer = "Limestone Walls";

    public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
    {
        if (brushTarget == null) return;
        if (brushTarget.name != WallLayer) { base.BoxFill(gridLayout, brushTarget, bounds); return; }
        if (!ValidateTarget(brushTarget, out var map)) return;
        var floors = Companion(map, " Floors", -1);
        var details = Companion(map, " Details", 1);
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { map, floors, details }, "Paint limestone walls and stairs");
        var wall = AssetDatabase.LoadAssetAtPath<RuleTile>(KitRoot + "/ConnectedWall.asset");
        foreach (var p in bounds.allPositionsWithin)
        {
            var local = p - bounds.min;
            var cell = cells[GetCellIndexWrapAround(local.x, local.y, local.z)];
            if (cell.tile == null) continue;
            if (cell.tile == wall)
            {
                map.SetTile(p, wall); details.SetTile(p, null);
                continue;
            }
            string name = cell.tile.name;
            bool socket = name.EndsWith("_Socket", StringComparison.Ordinal);
            bool open = name.EndsWith("_Open", StringComparison.Ordinal);
            bool floor = name.StartsWith("Stair", StringComparison.Ordinal) || name.StartsWith("Landing", StringComparison.Ordinal);
            if (floor)
            {
                floors.SetTile(p, cell.tile); floors.SetTransformMatrix(p, cell.matrix);
                details.SetTile(p, null);
                map.SetTile(p, socket ? wall : null);
            }
            else
            {
                details.SetTile(p, cell.tile); details.SetTransformMatrix(p, cell.matrix);
                if (socket) map.SetTile(p, wall);
                else if (open) map.SetTile(p, null);
            }
        }
        map.RefreshAllTiles();
        EditorUtility.SetDirty(map); EditorUtility.SetDirty(floors); EditorUtility.SetDirty(details);
    }

    public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
    {
        if (brushTarget == null) return;
        if (brushTarget.name != WallLayer) { base.BoxErase(gridLayout, brushTarget, bounds); return; }
        if (!ValidateTarget(brushTarget, out var map)) return;
        var floors = Companion(map, " Floors", -1);
        var details = Companion(map, " Details", 1);
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { map, floors, details }, "Erase limestone walls and stairs");
        foreach (var p in bounds.allPositionsWithin) { map.SetTile(p, null); floors.SetTile(p, null); details.SetTile(p, null); }
        map.RefreshAllTiles();
    }

    public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
    {
        if (brushTarget == null || brushTarget.name != WallLayer) { base.FloodFill(gridLayout, brushTarget, position); return; }
        var wall = AssetDatabase.LoadAssetAtPath<RuleTile>(KitRoot + "/ConnectedWall.asset");
        if (cellCount == 1 && cells[0].tile == wall && ValidateTarget(brushTarget, out var map))
        {
            Undo.RegisterCompleteObjectUndo(map, "Fill limestone walls");
            map.FloodFill(position, wall); map.RefreshAllTiles();
        }
        else Debug.Log("Use Paint or Box Fill to place a stair or structure stamp.");
    }

    private static bool ValidateTarget(GameObject target, out Tilemap map)
    {
        map = target.GetComponent<Tilemap>();
        var grid = target.GetComponentInParent<Grid>();
        if (map == null || grid == null || grid.cellSize != Vector3.one || grid.transform.lossyScale != Vector3.one)
        {
            Debug.LogError("Limestone structures need a 1 x 1 x 1 Grid at scale 1, matching Dungeon Limestone128.");
            return false;
        }
        if (PrefabUtility.IsPartOfPrefabInstance(target))
        {
            Debug.LogError("Unpack the scene map instance before painting, or edit it in Prefab Mode.");
            return false;
        }
        return true;
    }

    public static Tilemap Companion(Tilemap map, string suffix, int orderOffset)
    {
        string name = WallLayer + suffix;
        var sibling = map.transform.parent.Find(name);
        if (sibling != null) return sibling.GetComponent<Tilemap>();
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        Undo.RegisterCreatedObjectUndo(go, "Create limestone structure layer");
        go.transform.SetParent(map.transform.parent, false);
        go.transform.localPosition = map.transform.localPosition;
        go.transform.localRotation = map.transform.localRotation;
        go.transform.localScale = map.transform.localScale;
        go.layer = map.gameObject.layer;
        var renderer = go.GetComponent<TilemapRenderer>();
        var sourceRenderer = map.GetComponent<TilemapRenderer>();
        renderer.sharedMaterial = sourceRenderer.sharedMaterial;
        renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        renderer.sortingOrder = sourceRenderer.sortingOrder + orderOffset;
        return go.GetComponent<Tilemap>();
    }
}

