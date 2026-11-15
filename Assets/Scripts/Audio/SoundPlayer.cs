using UnityEngine;

// Base for the sound controllers (PlayerSoundController, EnemySoundController): gameplay fires events,
// the controller on the same object listens and plays a SoundData. The sound itself (clips, volume, pitch, mixer group)
// lives in the SoundData asset; gameplay code never knows about audio.
// Shares the object's AudioSource (2D).
[RequireComponent(typeof(AudioSource))]
public abstract class SoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // Null sound or no clips = silent, so every sound field is optional.
    protected void Play(SoundData sound)
    {
        AudioClip clip = sound != null ? sound.PickClip() : null;
        if (clip == null) return;

        if (sound.output != null) audioSource.outputAudioMixerGroup = sound.output;
        audioSource.pitch = sound.PickPitch();
        audioSource.PlayOneShot(clip, sound.volumeScale);
    }

    // For sounds that must outlive this object (an enemy is despawned before its death sound ends):
    // plays on a temporary object, destroyed when the clip is done.
    protected void PlayDetached(SoundData sound)
    {
        AudioClip clip = sound != null ? sound.PickClip() : null;
        if (clip == null) return;

        var temp = new GameObject($"{name} {clip.name}");
        temp.transform.position = transform.position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sound.output != null ? sound.output : audioSource.outputAudioMixerGroup;
        source.spatialBlend = 0f;
        source.pitch = sound.PickPitch();
        source.PlayOneShot(clip, sound.volumeScale);

        Destroy(temp, clip.length / source.pitch + 0.1f);
    }
}
