using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

// Bind Unity's existing brush editor and expose only the intended structure painting layer.
[CustomEditor(typeof(LimestoneStructureBrush))]
public class LimestoneStructureBrushEditor : GridBrushEditor
{
    public override GameObject[] validTargets => (base.validTargets ?? Array.Empty<GameObject>())
        .Where(go => go != null && go.name == LimestoneStructureBrush.WallLayer).ToArray();
}
