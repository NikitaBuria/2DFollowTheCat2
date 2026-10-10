using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputReader : MonoBehaviour
{
   [Header("Input references")]
   [SerializeField] private  InputActionReference playerInput;
   [SerializeField] private InputActionReference jumpInput;


   public event Action OnJumpPerformed;
   public event Action <Vector2> MoveInputPerformed;



   private void OnEnable()
   {

      playerInput.action.performed += OnMovePerformed;
      jumpInput.action.performed += OnJumpPerformedCallback;

      EnableActions();
   }

    private void EnableActions()
    {
        playerInput.action.Enable();
        jumpInput.action.Enable();
    }

    private void OnDisable()
    {
        playerInput.action.performed -= OnMovePerformed;
        jumpInput.action.performed -= OnJumpPerformedCallback;

        DisableActions();
    }

    private void DisableActions()
    {
        playerInput.action.Disable();
        jumpInput.action.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        MoveInputPerformed?.Invoke(moveInput);
    }

    private void OnJumpPerformedCallback(InputAction.CallbackContext context)
    {
        OnJumpPerformed?.Invoke();
    }
    
}
