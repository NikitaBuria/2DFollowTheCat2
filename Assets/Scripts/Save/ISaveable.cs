// A component that puts its own state into the save file and reads it back.
// SaveManager validates every ISaveable first and only loads when all of them accept the data,
// so a broken file can never half-load.
// Repair, don't reject: Validate refuses only what can't be fixed (missing parts, setup errors).
// Load repairs single bad entries (unknown item, out-of-range value) with a warning, so one renamed
// item between game versions costs that item, not the whole save.
public interface ISaveable
{
    void Save(SaveData data);

    // Returns null when the data can be loaded, otherwise why it can't.
    string Validate(SaveData data);

    void Load(SaveData data);
}
