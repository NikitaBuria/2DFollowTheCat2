using System;
using System.IO;
using System.Threading;
using UnityEngine;

// The only code that touches the save file. Knows nothing about the player or the scene.
public static class SaveFile
{
    // The game's one save slot (SaveManager, the main menu and the dev tools all use it).
    public const string DefaultFileName = "save.json";

    // A real save is a few KB; anything bigger is broken or tampered with.
    private const int MaxFileSize = 65536;

    public static string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName);

    // A locked file (antivirus, cloud sync, search indexer) usually frees up within milliseconds.
    private const int ReplaceAttempts = 3;
    private const int RetryDelayMs = 50;

    // Writes to a .tmp file first, reads it back to verify it, then swaps it in and keeps the old save as .bak,
    // so a crash or a bad write never destroys the last good save.
    public static bool Write(SaveData data, string path, out string error)
    {
        string tmpPath = path + ".tmp";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(tmpPath, json);

            // Verify: the file on disk must be exactly what was written and must parse back.
            string written = File.ReadAllText(tmpPath);
            SaveData check = JsonUtility.FromJson<SaveData>(written);
            if (written != json || check == null || check.version != data.version)
            {
                error = "the written save did not verify";
                return false;
            }

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    if (File.Exists(path)) File.Replace(tmpPath, path, path + ".bak");
                    else File.Move(tmpPath, path);
                    break;
                }
                catch (IOException) when (attempt < ReplaceAttempts)
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            error = null;
            return true;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
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
