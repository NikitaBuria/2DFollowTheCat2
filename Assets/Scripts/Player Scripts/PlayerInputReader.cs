using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputReader : MonoBehaviour
{
   [Header("Input references")]
  
   [SerializeField] private  InputActionReference playerMoveInput;

   [SerializeField] private InputActionReference dashInput;

   [SerializeField] private InputActionReference sprintInput;

   [SerializeField] private InputActionReference  attackInput;






   public event Action OnDashPerformed;
   public event Action OnAttackPerformed;


   public Vector2 MoveInput => playerMoveInput.action.ReadValue<Vector2>(); 
   public bool IsSprintHeld => sprintInput.action.IsPressed();



   private void OnEnable()
   {

      EnableActions();
      dashInput.action.performed += OnDashPerformedCallBack;
      attackInput.action.performed += OnAttackPerformedCallBack;

   }

    private void OnAttackPerformedCallBack(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke();
    }

    private void EnableActions()
    {
        playerMoveInput.action.Enable();
        dashInput.action.Enable();
        sprintInput.action.Enable();
        attackInput.action.Enable();
    }

    private void OnDisable()
    {  
        DisableActions();
        dashInput.action.performed -= OnDashPerformedCallBack; 
        attackInput.action.performed -= OnAttackPerformedCallBack;
    
    }

    private void DisableActions()
    {
        playerMoveInput.action.Disable();
        dashInput.action.Disable();
        sprintInput.action.Disable();
        attackInput.action.Disable();
        
    }

    private void OnDashPerformedCallBack(InputAction.CallbackContext context)
    {
        OnDashPerformed?.Invoke();
    }

  
    
}
