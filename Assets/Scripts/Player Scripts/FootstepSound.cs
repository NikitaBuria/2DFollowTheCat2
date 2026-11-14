using UnityEngine;

// Plays a footstep every stepDistance units the character actually moves, so running sounds faster
// than walking without any state knowing about audio. Reads the Rigidbody, never input.
// The step sound depends on the ground, so it is set per scene (grass in Core, stone in the Dungeon).
// No stepSound = silent.
[RequireComponent(typeof(Rigidbody2D))]
public class FootstepSound : SoundPlayer
{
    [SerializeField] private SoundData stepSound;
    // World units walked between two steps.
    [SerializeField, Min(0.1f)] private float stepDistance = 1.2f;
    // Slower than this counts as standing (collision shoves fading out).
    [SerializeField, Min(0f)] private float minSpeed = 0.5f;

    private Rigidbody2D rb;
    private float distanceSinceStep;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        if (stepSound == null || speed < minSpeed)
        {
            // The first step sounds as soon as the character starts moving.
            distanceSinceStep = stepDistance;
            return;
        }

        distanceSinceStep += speed * Time.deltaTime;
        if (distanceSinceStep < stepDistance) return;

        distanceSinceStep = 0f;
        Play(stepSound);
    }
}
