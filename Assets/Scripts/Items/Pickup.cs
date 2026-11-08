using UnityEngine;

// Base for world pickups: a trigger that hands itself to whoever touches it.
// Subclasses decide what "picking up" means (add coins, add an item...).
[RequireComponent(typeof(CircleCollider2D))]
public abstract class Pickup : MonoBehaviour, IPickable
{
    private void Awake()
    {
        // Top-down: no gravity, nothing to rest on, so the pickup only needs a trigger.
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public abstract bool TryPickUp(GameObject collector);

    private void OnTriggerEnter2D(Collider2D other)
    {
        // The collider can sit on a child; the components we need live on the Rigidbody's object.
        GameObject collector = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (TryPickUp(collector))
            Destroy(gameObject);
    }
}
