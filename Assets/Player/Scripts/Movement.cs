using UnityEngine;

public class Movement : MonoBehaviour
{


    [Header("References")]

    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private Rigidbody2D rb;
    [Header("Movement Settings")]
   


    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

   
   void   Awake() {
    
    
        Subscribe();
    }


    void Update()
    {
        
    }

    private void OnEnable()
    {
        playerInputReader.MoveInputPerformed += Move;
        playerInputReader.OnJumpPerformed += Jump;  
    }

    private void OnDisable()
    {
        playerInputReader.MoveInputPerformed -= Move;
        playerInputReader.OnJumpPerformed -= Jump;  
    }



    private void Move (Vector2 moveInput)
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }


    private void Jump()
    {
        
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
    }


    private void Subscribe()
    {

        if (playerInputReader == null)
        {
            playerInputReader = GetComponent<PlayerInputReader>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
    }
}
