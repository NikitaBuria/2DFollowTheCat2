using System.Collections;
using UnityEngine;

// When the last town building is restored, waits for the rebuild clip and switches to GameState.Won
// (UIManager then shows the win panel). On SaveSystem next to TownProgress.
[RequireComponent(typeof(TownProgress))]
public class WinCondition : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    // Seconds to watch the last building rebuild before the panel opens.
    [SerializeField, Min(0f)] private float delay = 2f;

    private TownProgress townProgress;

    private void Awake()
    {
        townProgress = GetComponent<TownProgress>();
    }

    private void OnEnable()
    {
        townProgress.OnTownRestored += HandleTownRestored;
    }

    private void OnDisable()
    {
        townProgress.OnTownRestored -= HandleTownRestored;
    }

    private void HandleTownRestored()
    {
        StartCoroutine(WinAfterDelay());
    }

    private IEnumerator WinAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        gameStateManager.ChangeState(GameState.Won);
    }
}
