using UnityEngine;

// On a building's restored art, next to Astra's rebuild clip (legacy Animation).
// Plays the clip when the building announces it was restored; the building never touches its visuals.
[RequireComponent(typeof(Animation))]
public class BuildingRebuildView : MonoBehaviour
{
    [SerializeField] private RestorableBuilding building;

    private Animation rebuildAnimation;

    private void Awake()
    {
        rebuildAnimation = GetComponent<Animation>();
    }

    // The restored art is switched on (OnProgressChanged) before OnRestored fires, so this listens in time.
    private void OnEnable()
    {
        building.OnRestored += Play;
    }

    private void OnDisable()
    {
        building.OnRestored -= Play;
    }

    private void Play() => rebuildAnimation.Play();
}
