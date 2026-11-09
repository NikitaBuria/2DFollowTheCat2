using UnityEngine;
using UnityEngine.InputSystem;

// The only script that reads input. States and GameStateManager poll these properties in Update.
public class PlayerInputReader : MonoBehaviour
{
    [Header("Input references")]
    [SerializeField] private InputActionReference playerMoveInput;
    [SerializeField] private InputActionReference dashInput;
    [SerializeField] private InputActionReference sprintInput;
    [SerializeField] private InputActionReference attackInput;
    [SerializeField] private InputActionReference toggleMenuInput;
    [SerializeField] private InputActionReference toggleInventoryInput;

    // All input is polled in Update (never in FixedUpdate, or one-frame presses can be missed).
    public Vector2 moveInput => playerMoveInput.action.ReadValue<Vector2>();
    public bool isSprintHeld => sprintInput.action.IsPressed();
    public bool dashPressedThisFrame => dashInput.action.WasPressedThisFrame();
    public bool attackPressedThisFrame => attackInput.action.WasPressedThisFrame();
    public bool menuPressedThisFrame => toggleMenuInput.action.WasPressedThisFrame();
    public bool inventoryPressedThisFrame => toggleInventoryInput.action.WasPressedThisFrame();

    private void OnEnable()
    {
        EnableUIActions();
        EnablePlayerActions();
    }

    private void OnDisable()
    {
        DisablePlayerActions();
        DisableUIActions();
    }

    // The Player map is switched by GameStateManager; the UI map stays on so menus can always be toggled.
    public void EnablePlayerActions() => playerMoveInput.action.actionMap.Enable();
    public void DisablePlayerActions() => playerMoveInput.action.actionMap.Disable();

    private void EnableUIActions() => toggleInventoryInput.action.actionMap.Enable();
    private void DisableUIActions() => toggleInventoryInput.action.actionMap.Disable();
}
