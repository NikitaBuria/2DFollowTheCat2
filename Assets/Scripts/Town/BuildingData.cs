using UnityEngine;

// One building that can be restored in town (Smithy, Apothecary, Town Hall).
// Saved by ID (the asset name) in TownProgress, like items in the ItemDatabase.
[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Scriptable Objects/Building Data")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string buildingName { get; private set; } = "Building";
    [field: SerializeField, Min(0)] public int cost { get; private set; } = 100;

    [Tooltip("Optional item that is used up when restoring (e.g. the boss drop for the Town Hall).")]
    [field: SerializeField] public ItemData requiredItem { get; private set; }

    public string id => name;
}
