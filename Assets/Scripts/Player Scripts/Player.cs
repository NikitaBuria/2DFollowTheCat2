using UnityEngine;

[RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor))]
public class Player : MonoBehaviour
{
    [Header("Player References")]
    [field: SerializeField] public PlayerInputReader playerInputReader { get; private set; }
    [field: SerializeField] public PlayerMotor playerMotor { get; private set; }
    [field: SerializeField] public Animator animator { get; private set; }

    [Header("Movement")]
    [field: SerializeField] public float WalkSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RunSpeed { get; private set; } = 9f;

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

    // Start (not OnEnable) so every component's Awake has run before the first Enter,
    // and so re-enabling the player doesn't call Initialize twice.
    private void Start()
    {
        playerStateMachine.Initialize(IdleState);
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

    private void GetReferences()
    {
        playerStateMachine ??= new PlayerStateMachine();

        if (playerInputReader == null) playerInputReader = GetComponent<PlayerInputReader>();
        if (playerMotor == null) playerMotor = GetComponent<PlayerMotor>();
    }
}
