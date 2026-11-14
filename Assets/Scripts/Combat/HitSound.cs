using UnityEngine;

// Plays a sound when this character takes a hit. Only listens to Health.OnDamaged, so combat code
// never knows about audio. A small random pitch keeps repeated hits from sounding identical.
[RequireComponent(typeof(Health), typeof(AudioSource))]
public class HitSound : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField, Range(0.5f, 1.5f)] private float minPitch = 0.9f;
    [SerializeField, Range(0.5f, 1.5f)] private float maxPitch = 1.1f;

    private Health health;
    private AudioSource audioSource;

    private void Awake()
    {
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        health.OnDamaged += PlayHit;
    }

    private void OnDisable()
    {
        health.OnDamaged -= PlayHit;
    }

    private void PlayHit(DamageData damage)
    {
        if (hitClip == null) return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(hitClip, volume);
    }
}
