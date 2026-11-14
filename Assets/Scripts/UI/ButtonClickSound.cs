using UnityEngine;
using UnityEngine.UI;

// Plays a click when this button is pressed. The AudioSource lives on an always-active object
// (the HUD / menu canvas), so the sound keeps playing when the button's panel closes.
// Audio ignores Time.timeScale, so it works while the game is paused.
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundData clickSound;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayClick);
    }

    private void PlayClick()
    {
        if (clickSound != null) clickSound.PlayOn(audioSource);
    }
}
