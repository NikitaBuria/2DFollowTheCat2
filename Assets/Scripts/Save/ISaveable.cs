// A component that puts its own state into the save file and reads it back.
// SaveManager validates every ISaveable first and only loads when all of them accept the data,
// so a broken file can never half-load.
public interface ISaveable
{
    void Save(SaveData data);

    // Returns null when the data is fine, otherwise what is wrong with it.
    string Validate(SaveData data);

    void Load(SaveData data);
}
