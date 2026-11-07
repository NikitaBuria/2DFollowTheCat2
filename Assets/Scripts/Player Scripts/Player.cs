using UnityEngine;

[RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor), typeof(PlayerAnimator))]
public class Player : MonoBehaviour
{
    [Header("Player References")]
    [field: SerializeField] public PlayerInputReader playerInputReader { get; private set; }
    [field: SerializeField] public PlayerMotor playerMotor { get; private set; }
    [field: SerializeField] public PlayerAnimator playerAnimator { get; private set; }
    [field: SerializeField] public PlayerHealth playerHealth {get; private set;}

    [Header("Movement")]
    [field: SerializeField] public float WalkSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RunSpeed { get; private set; } = 9f;
    [field: SerializeField] public float DashSpeed {get; private set;} = 10f;

    
    public  Vector2 FacingDirection {get; private set;} = Vector2.down;

    public PlayerStateMachine playerStateMachine { get; private set; }

    // PlayerStates
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    private void Awake()
    {
        GetReferences();
        CreateStates();
    }


    private void Start()
    {
        playerStateMachine.Initialize(IdleState);
    }

    public void OnEnable()
    {
        // playerHealth.playerDied += handelPlayerDeath;
        
    }


    public void OnDisable()
    {
        // playerHealth.playerDied -= handelPlayerDeath;
    }
    
        private void Update()
    {
        playerStateMachine.CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        playerStateMachine.CurrentState?.FixedUpdate();
    }

    private void CreateStates()
    {
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        RunState = new PlayerRunState(this);
        DashState = new PlayerDashState(this);
        AttackState = new PlayerAttackState(this);
        HurtState = new PlayerHurtState(this);
        DeadState = new PlayerDeadState(this);
    }


    public void handelPlayerDeath()
    {
        //Show lose screen or a menu to rest to checkpoint or main menu;
    }

    private void GetReferences()
    {
        playerStateMachine ??= new PlayerStateMachine();

        if (playerInputReader == null) playerInputReader = GetComponent<PlayerInputReader>();
        if (playerMotor == null) playerMotor = GetComponent<PlayerMotor>();
        if (playerHealth==null) playerHealth = GetComponent<PlayerHealth>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();    



    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        FacingDirection = direction;
        playerAnimator.SetMoveDirection(direction);
    }
}
