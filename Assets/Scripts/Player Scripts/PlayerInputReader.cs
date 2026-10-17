using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputReader : MonoBehaviour
{
   [Header("Input references")]
  
   [SerializeField] private  InputActionReference playerMoveInput;

   [SerializeField] private InputActionReference dashInput;

   [SerializeField] private InputActionReference sprintInput;






   public event Action OnDashPerformed;


   public Vector2 MoveInput => playerMoveInput.action.ReadValue<Vector2>(); 
   public bool IsSprintHeld => sprintInput.action.IsPressed();



   private void OnEnable()
   {

      EnableActions();
      dashInput.action.performed += OnDashPerformedCallBack;

   }

    private void EnableActions()
    {
        playerMoveInput.action.Enable();
        dashInput.action.Enable();
        sprintInput.action.Enable();
    }

    private void OnDisable()
    {  
        DisableActions();
        dashInput.action.performed -= OnDashPerformedCallBack; 
    }

    private void DisableActions()
    {
        playerMoveInput.action.Disable();
        dashInput.action.Disable();
        sprintInput.action.Disable();
        
    }

    private void OnDashPerformedCallBack(InputAction.CallbackContext context)
    {
        OnDashPerformed?.Invoke();
    }

  
    
}
