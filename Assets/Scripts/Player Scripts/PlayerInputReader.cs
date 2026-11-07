using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
   [Header("Input references")]

   [SerializeField] private  InputActionReference playerMoveInput;

   [SerializeField] private InputActionReference dashInput;

   [SerializeField] private InputActionReference sprintInput;

   [SerializeField] private InputActionReference  attackInput;


   // All input is polled by the current state in Update (never in FixedUpdate,
   // or one-frame presses can be missed).
   public Vector2 MoveInput => playerMoveInput.action.ReadValue<Vector2>();
   public bool IsSprintHeld => sprintInput.action.IsPressed();
   public bool DashPressedThisFrame => dashInput.action.WasPressedThisFrame();
   public bool AttackPressedThisFrame => attackInput.action.WasPressedThisFrame();


   private void OnEnable()
   {
      EnableActions();
   }

    private void OnDisable()
    {
        DisableActions();
    }

    private void EnableActions()
    {
        playerMoveInput.action.Enable();
        dashInput.action.Enable();
        sprintInput.action.Enable();
        attackInput.action.Enable();
    }

    private void DisableActions()
    {
        playerMoveInput.action.Disable();
        dashInput.action.Disable();
        sprintInput.action.Disable();
        attackInput.action.Disable();
    }
}
