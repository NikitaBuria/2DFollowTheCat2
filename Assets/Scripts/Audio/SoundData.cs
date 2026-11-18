using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

// One sound as data: which clip(s), how loud, how much pitch variation, and which Audio Mixer group
// it plays through (so the Settings volume sliders control it). Characters, weapons and items point
// to these assets; sound players only decide when to play them.
[CreateAssetMenu(fileName = "NewSound", menuName = "Scriptable Objects/Sound Data")]
public class SoundData : ScriptableObject
{
    // More than one = a random one each time, so repeated sounds don't feel identical.
    [SerializeField] private AudioClip[] clips;
    [field: SerializeField, FormerlySerializedAs("mixerGroup")]
    public AudioMixerGroup mixerGroup { get; private set; }
    [field: SerializeField, Range(0f, 1f), FormerlySerializedAs("volume")]
    public float volume { get; private set; } = 0.8f;
    [SerializeField, Range(0.5f, 1.5f)] private float minPitch = 0.95f;
    [SerializeField, Range(0.5f, 1.5f)] private float maxPitch = 1.05f;

    // Null when there are no clips.
    public AudioClip PickClip() =>
        clips == null || clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];

    public float PickPitch() => Random.Range(minPitch, maxPitch);

    // One-shot on a given source (UI sounds on the always-active UIClickAudio source).
    public void PlayOn(AudioSource source)
    {
        AudioClip clip = PickClip();
        if (source == null || clip == null) return;

        if (mixerGroup != null) source.outputAudioMixerGroup = mixerGroup;
        source.pitch = PickPitch();
        source.PlayOneShot(clip, volume);
    }

    // Loops one clip on a source that plays nothing else (portal hum, enemy hover).
    // The source keeps its own 3D settings. False when there is nothing to play.
    public bool PlayLoop(AudioSource source)
    {
        AudioClip clip = PickClip();
        if (source == null || clip == null) return false;

        source.playOnAwake = false;
        source.loop = true;
        source.clip = clip;
        source.volume = volume;
        source.pitch = PickPitch();
        if (mixerGroup != null) source.outputAudioMixerGroup = mixerGroup;
        source.Play();
        return true;
    }

    private void OnValidate()
    {
        if (maxPitch < minPitch) maxPitch = minPitch;
    }
}
