using UnityEngine;

// Moves the player's Rigidbody. No Update, no input, no decisions: states pass in direction and speed.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 direction, float speed) => rb.linearVelocity = direction * speed;

    // Moves the body straight to a position (respawn, scene arrival), with no velocity left over.
    public void Teleport(Vector2 position)
    {
        rb.position = position;
        rb.linearVelocity = Vector2.zero;
    }

    public void Stop() => rb.linearVelocity = Vector2.zero;
}
