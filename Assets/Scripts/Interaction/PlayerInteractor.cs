using System.Collections.Generic;
using UnityEngine;

// Finds the interactable in front of the player. Doesn't read input: the player states that allow
// interacting call TryInteract (Idle, Walk, Run). Doesn't know the UI: InteractionPromptView reads
// the prompt properties. Player.Update calls Refresh every frame, like Enemy does with EnemyDetection.
// Adapted from MetrooDvania's PlayerInteraction2D: OverlapCircle in the facing direction
// instead of a trigger around the player, so you use what you are looking at.
[RequireComponent(typeof(Player))]
public class PlayerInteractor : MonoBehaviour
{
    [Tooltip("How far in front of the player the search circle sits.")]
    [SerializeField, Min(0f)] private float searchOffset = 0.5f;
    [SerializeField, Min(0.1f)] private float searchRadius = 0.6f;
    [SerializeField] private LayerMask interactableLayerMask;

    private Player player;
    private readonly List<Collider2D> hits = new();
    private ContactFilter2D filter;

    public IInteractable currentTarget { get; private set; }

    // What the prompt shows for the current target (read by InteractionPromptView).
    public string promptText { get; private set; }
    public bool canInteract { get; private set; }
    public Vector3 targetPosition { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();

        filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(interactableLayerMask);

        if (interactableLayerMask == 0)
            Debug.LogError($"{name}: PlayerInteractor's interactableLayerMask is Nothing. Pick the Interactable layer.", this);
    }

    public void Refresh()
    {
        currentTarget = player.playerHealth.isAlive ? FindNearestTarget() : null;

        if (currentTarget is Component target)
        {
            promptText = currentTarget.GetPrompt(player);
            canInteract = currentTarget.CanInteract(player);
            targetPosition = target.transform.position;
        }
        else
        {
            promptText = null;
            canInteract = false;
        }
    }

    // Returns whether something was used.
    public bool TryInteract()
    {
        if (currentTarget == null || !currentTarget.CanInteract(player)) return false;

        currentTarget.Interact(player);
        return true;
    }

    private Vector2 searchCenter => (Vector2)transform.position + player.facingDirection * searchOffset;

    private IInteractable FindNearestTarget()
    {
        Physics2D.OverlapCircle(searchCenter, searchRadius, filter, hits);

        IInteractable nearest = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float distance = ((Vector2)hit.transform.position - searchCenter).sqrMagnitude;
            
            if (distance < nearestDistance)
            {
                nearest = interactable;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) player = GetComponent<Player>();
        Vector2 facing = Application.isPlaying ? player.facingDirection : Vector2.down;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((Vector2)transform.position + facing * searchOffset, searchRadius);
    }
}
