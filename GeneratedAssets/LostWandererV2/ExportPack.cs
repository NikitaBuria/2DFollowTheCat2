public static class LostWandererExportPack
{
    public static string Export()
    {
        const string root = "Assets/Art/Characters/LostWanderer/TopDownV2";
        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
        UnityEditor.AssetDatabase.SaveAssets();
        string destination = System.IO.Path.GetFullPath("GeneratedAssets/LostWandererV2/LostWanderer-Unity.unitypackage");
        var assets = new System.Collections.Generic.List<string>();
        foreach (var state in new string[] { "Idle", "Walk", "Run", "Dash", "Attack", "Hurt", "Death" })
            assets.Add(root + "/LostWanderer_" + state + ".png");
        assets.Add(root + "/Animations");
        assets.Add(root + "/LostWanderer.controller");
        assets.Add(root + "/LostWandererVisual.prefab");
        assets.Add(root + "/README.md");
        UnityEditor.AssetDatabase.ExportPackage(assets.ToArray(), destination, UnityEditor.ExportPackageOptions.Recurse);
        if (!System.IO.File.Exists(destination) || new System.IO.FileInfo(destination).Length == 0)
            throw new System.Exception("Unity package export did not produce a file.");
        return destination;
    }
}
