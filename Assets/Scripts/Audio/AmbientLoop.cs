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
    }

    private void OnEnable()
    {
        if (loopSound != null) loopSound.PlayLoop(audioSource);
    }
}
