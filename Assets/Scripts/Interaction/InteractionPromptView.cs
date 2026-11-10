using TMPro;
using UnityEngine;

// The "[E] Talk" text above the current interactable. Only reads PlayerInteractor and draws;
// it never changes gameplay. Reads in LateUpdate, after Player.Update has refreshed the target.
// Adapted from MetrooDvania: TMP instead of legacy Text, and the layout lives in the scene object,
// not in code.
public class InteractionPromptView : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private string keyName = "E";
    [Tooltip("World offset from the interactable's pivot, so the prompt sits above it.")]
    [SerializeField] private Vector3 offset = new (0f, 0.8f, 0f);
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color refusedColor = new(1f, 0.55f, 0.45f);

    private void LateUpdate()
    {
        if (interactor.currentTarget == null)
        {
            label.enabled = false;
            return;
        }

        transform.position = interactor.targetPosition + offset;
        
        label.SetText(interactor.canInteract ? $"[{keyName}] {interactor.promptText}" : interactor.promptText);
        label.color = interactor.canInteract ? availableColor : refusedColor;
        label.enabled = true;
    }
}
