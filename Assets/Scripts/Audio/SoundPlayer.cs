using System.Collections.Generic;
using UnityEngine;

// Base for the sound controllers (PlayerSoundController, EnemySoundController, TalkSound, ShopSound):
// gameplay fires events, the controller listens and plays a SoundData. The sound itself (clips, volume,
// pitch, mixer group) lives in the SoundData asset; gameplay code never knows about audio.
// Every sound gets its own voice (an AudioSource that is not playing), because changing the pitch or
// mixer group of a source also changes the sounds still playing on it.
[RequireComponent(typeof(AudioSource))]
public abstract class SoundPlayer : MonoBehaviour
{
    // Enough for a hit, a swing and footsteps at once; past this the oldest sound is cut.
    private const int MaxVoices = 6;

    private readonly List<AudioSource> voices = new List<AudioSource>();
    private int nextVoiceToSteal;

    protected virtual void Awake()
    {
        // The required AudioSource is the first voice; its mixer group is the fallback for all of them.
        AudioSource first = GetComponent<AudioSource>();
        first.playOnAwake = false;
        first.loop = false;
        first.spatialBlend = 0f;
        voices.Add(first);
    }

    // Null sound or no clips = silent, so every sound field is optional.
    protected void Play(SoundData sound)
    {
        AudioClip clip = sound != null ? sound.PickClip() : null;
        if (clip == null) return;

        AudioSource voice = GetFreeVoice();
        voice.clip = clip;
        voice.volume = sound.volume;
        voice.pitch = sound.PickPitch();
        voice.outputAudioMixerGroup = sound.mixerGroup != null ? sound.mixerGroup : voices[0].outputAudioMixerGroup;
        voice.Play();
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
        source.outputAudioMixerGroup = sound.mixerGroup != null ? sound.mixerGroup : voices[0].outputAudioMixerGroup;
        source.spatialBlend = 0f;
        source.pitch = sound.PickPitch();
        source.PlayOneShot(clip, sound.volume);

        Destroy(temp, clip.length / source.pitch + 0.1f);
    }

    private AudioSource GetFreeVoice()
    {
        foreach (AudioSource voice in voices)
        {
            if (!voice.isPlaying) return voice;
        }

        if (voices.Count < MaxVoices)
        {
            AudioSource voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0f;
            voices.Add(voice);
            return voice;
        }

        // All busy: reuse them in turn.
        AudioSource stolen = voices[nextVoiceToSteal];
        nextVoiceToSteal = (nextVoiceToSteal + 1) % voices.Count;
        return stolen;
    }
}
