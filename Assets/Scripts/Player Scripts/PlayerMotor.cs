using System.Numerics;


[RequireComponent(typeof(Rigidbody2D))]


public class PlayerMotor : MonoBehavior
{


    [Header("References")]
    [SerializeField] private GameObject groundCheck;

    private Rigidbody2D rb;

   


    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float dashForce = 5f;



    private bool isSprinting => playerInputReader.OnSprintPerformed;

   
   void   Awake()
    {
        GetReferences();
    }




    private void Move (Vector2 direction, float speed) => rb.linearVelocity = direction * speed;
    


    private void Dash()
    {
        
       
    }


    private void GetReferences()
    {


        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
    }
    
}