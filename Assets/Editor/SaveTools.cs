using UnityEditor;
using UnityEngine;

// Editor-only shortcuts for the save file (Tools menu), like MetrooDvania's Tools menu.
// Uses the file name set on the Core prefab's SaveManager, so it always matches the game.
public static class SaveTools
{
    private const string MenuRoot = "Tools/Follow The Cat/";
    private const string CorePrefabPath = "Assets/Prefabs/Core.prefab";
    private const string DefaultFileName = "save.json";

    [MenuItem(MenuRoot + "Delete Save File")]
    private static void DeleteSaveFile()
    {
        string path = SaveFile.GetPath(GetSaveFileName());

        if (!System.IO.File.Exists(path) && !System.IO.File.Exists(path + ".bak"))
        {
            EditorUtility.DisplayDialog("Delete Save File", "There is no save file yet.\n\n" + path, "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Delete Save File",
                "Delete the save (and its backup)? The next Play starts a new game.\n\n" + path, "Delete", "Cancel"))
            return;

        SaveFile.Delete(path);
        Debug.Log("Deleted " + path);
    }

    // Deleting during Play would be undone by the next door or respawn autosave.
    [MenuItem(MenuRoot + "Delete Save File", true)]
    private static bool CanDeleteSaveFile() => !EditorApplication.isPlaying;

    [MenuItem(MenuRoot + "Open Save Folder")]
    private static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(SaveFile.GetPath(GetSaveFileName()));
    }

    private static string GetSaveFileName()
    {
        var core = AssetDatabase.LoadAssetAtPath<GameObject>(CorePrefabPath);
        var saveManager = core != null ? core.GetComponentInChildren<SaveManager>(true) : null;
        if (saveManager == null) return DefaultFileName;

        string fileName = new SerializedObject(saveManager).FindProperty("saveFileName").stringValue;
        return string.IsNullOrEmpty(fileName) ? DefaultFileName : fileName;
    }
}
