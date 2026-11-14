using UnityEngine;

// A named spot the GuideCat walks to (e.g. "TownMerchant"). Only marks a position, like SpawnPoint.
public class GuideMarker : MonoBehaviour
{
    [SerializeField] private string markerId;

    public string id => markerId;

    // Returns null when this scene has no marker with that ID.
    public static GuideMarker Find(string markerId)
    {
        foreach (GuideMarker marker in FindObjectsByType<GuideMarker>(FindObjectsSortMode.None))
        {
            if (marker.markerId == markerId) return marker;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
