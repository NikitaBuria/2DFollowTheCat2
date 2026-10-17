using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 direction, float speed) => rb.linearVelocity = direction * speed;

    public void Stop() => rb.linearVelocity = Vector2.zero;
}
