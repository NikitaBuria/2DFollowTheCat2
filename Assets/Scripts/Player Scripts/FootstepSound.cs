using UnityEngine;

// Plays a footstep every stepDistance units the character actually moves, so running sounds faster
// than walking without any state knowing about audio. Reads the Rigidbody, never input.
// No stepClip = silent (the Dungeon's Core leaves it empty until it has its own surface sound).
[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class FootstepSound : MonoBehaviour
{
    [SerializeField] private AudioClip stepClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    // World units walked between two steps.
    [SerializeField, Min(0.1f)] private float stepDistance = 1.2f;
    // Slower than this counts as standing (collision shoves fading out).
    [SerializeField, Min(0f)] private float minSpeed = 0.5f;
    [SerializeField, Range(0.5f, 1.5f)] private float minPitch = 0.9f;
    [SerializeField, Range(0.5f, 1.5f)] private float maxPitch = 1.1f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float distanceSinceStep;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        if (stepClip == null || speed < minSpeed)
        {
            // The first step sounds as soon as the character starts moving.
            distanceSinceStep = stepDistance;
            return;
        }

        distanceSinceStep += speed * Time.deltaTime;
        if (distanceSinceStep < stepDistance) return;

        distanceSinceStep = 0f;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(stepClip, volume);
    }
}
