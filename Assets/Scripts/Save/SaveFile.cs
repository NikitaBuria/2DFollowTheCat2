using System;
using System.IO;
using UnityEngine;

// The only code that touches the save file. Knows nothing about the player or the scene.
public static class SaveFile
{
    // A real save is a few KB; anything bigger is broken or tampered with.
    private const int MaxFileSize = 65536;

    public static string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName);

    // Writes to a .tmp file first, then swaps it in and keeps the old save as .bak,
    // so a crash in the middle of saving never destroys the last good save.
    public static bool Write(SaveData data, string path, out string error)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path + ".tmp", JsonUtility.ToJson(data, true));

            if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
            else File.Move(path + ".tmp", path);

            error = null;
            return true;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            error = e.Message;
            return false;
        }
    }

    // Returns null when there is no file. Throws on unreadable files; the caller decides what to do.
    public static SaveData Read(string path)
    {
        if (!File.Exists(path)) return null;
        if (new FileInfo(path).Length > MaxFileSize) throw new InvalidDataException("Save file is too large.");

        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public static void Delete(string path)
    {
        foreach (string suffix in new[] { "", ".bak", ".tmp" })
        {
            if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
    }
}
