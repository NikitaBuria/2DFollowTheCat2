using UnityEngine;

// Moves the enemy's Rigidbody. No Update, no decisions: states tell it where and how fast.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Moves toward a point without overshooting it in one physics step
    // (overshooting makes the enemy jitter back and forth around patrol points).
    public void MoveTowards(Vector2 destination, float speed)
    {
        Vector2 toDestination = destination - rb.position;
        float speedToArriveThisStep = toDestination.magnitude / Time.fixedDeltaTime;
        rb.linearVelocity = toDestination.normalized * Mathf.Min(speed, speedToArriveThisStep);
    }

    // Moves in a direction, e.g. knockback.
    public void Move(Vector2 direction, float speed) => rb.linearVelocity = direction * speed;

    public void Stop() => rb.linearVelocity = Vector2.zero;
}
