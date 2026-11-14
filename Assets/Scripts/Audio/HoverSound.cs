using UnityEngine;

// Loops the enemy's EnemyData.hoverSound while it is active (on the enemy's HoverAudio child).
// The child's AudioSource keeps its own 3D settings, so the hum is only heard nearby; the clip,
// volume and mixer group come from the SoundData. Pooled minions start again when re-enabled.
[RequireComponent(typeof(AudioSource))]
public class HoverSound : MonoBehaviour
{
    private AudioSource audioSource;
    private Enemy enemy;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnEnable()
    {
        SoundData sound = enemy != null ? enemy.enemyData.hoverSound : null;
        AudioClip clip = sound != null ? sound.PickClip() : null;
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.volume = sound.volumeScale;
        audioSource.pitch = sound.PickPitch();
        if (sound.output != null) audioSource.outputAudioMixerGroup = sound.output;
        audioSource.Play();
    }
}
