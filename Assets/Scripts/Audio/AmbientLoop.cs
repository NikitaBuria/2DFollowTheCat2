using UnityEngine;

// Loops a SoundData while this object is active (the dungeon portals' hum). The AudioSource keeps its
// own 3D settings, so it is only heard nearby; clip, volume and mixer group come from the SoundData.
[RequireComponent(typeof(AudioSource))]
public class AmbientLoop : MonoBehaviour
{
    [SerializeField] private SoundData loopSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
    }

    private void OnEnable()
    {
        AudioClip clip = loopSound != null ? loopSound.PickClip() : null;
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.volume = loopSound.volumeScale;
        audioSource.pitch = loopSound.PickPitch();
        if (loopSound.output != null) audioSource.outputAudioMixerGroup = loopSound.output;
        audioSource.Play();
    }
}
