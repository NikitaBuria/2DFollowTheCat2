using UnityEngine;

// Wires the player together: creates the states and forwards Update/FixedUpdate to the current one.
// No game logic here; states decide what happens, components do the work.
[RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor), typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerCombat), typeof(PlayerHealth))]
public class Player : MonoBehaviour
{
    // Components on this GameObject. Found with GetComponent in Awake, so nothing to assign in the Inspector.
    public PlayerHealth playerHealth { get; private set; }
    public PlayerInputReader playerInputReader { get; private set; }
    public PlayerMotor playerMotor { get; private set; }
    public PlayerAnimator playerAnimator { get; private set; }
    public PlayerCombat playerCombat { get; private set; }

    [Header("Data")]
    // Base stats (speeds, max health). States read them through player.characterData.
    [field: SerializeField] public CharacterData characterData { get; private set; }

    // The last hit that landed; the Hurt state reads its direction and knockback.
    public DamageData lastDamage { get; private set; }

    // Which way the player faces; used for animations and attacks.
    public Vector2 facingDirection { get; private set; } = Vector2.down;

    public PlayerStateMachine playerStateMachine { get; private set; }

    // States
    public PlayerIdleState idleState { get; private set; }
    public PlayerWalkState walkState { get; private set; }
    public PlayerRunState runState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerAttackState attackState { get; private set; }
    public PlayerHurtState hurtState { get; private set; }
    public PlayerDeadState deadState { get; private set; }

    private void Awake()
    {
        if (characterData == null)
            Debug.LogError($"{name}: Player has no CharacterData assigned.", this);

        GetReferences();
        CreateStates();
        playerHealth.Initialize(characterData.maxHealth);
    }

    private void Start()
    {
        playerStateMachine.Initialize(idleState);
    }

    private void OnEnable()
    {
        playerHealth.OnDied += HandlePlayerDeath;
        playerHealth.OnHurt += HandlePlayerHurt;
    }

    private void OnDisable()
    {
        playerHealth.OnDied -= HandlePlayerDeath;
        playerHealth.OnHurt -= HandlePlayerHurt;
    }

    private void Update()
    {
        playerStateMachine.currentState?.Update();
    }

    private void FixedUpdate()
    {
        playerStateMachine.currentState?.FixedUpdate();
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        facingDirection = direction;
        playerAnimator.SetMoveDirection(direction);
    }

    private void HandlePlayerDeath() => playerStateMachine.ChangeState(deadState);

    private void HandlePlayerHurt(DamageData damage)
    {
        lastDamage = damage;
        playerStateMachine.ChangeState(hurtState);
    }

    private void GetReferences()
    {
        playerStateMachine = new PlayerStateMachine();

        // RequireComponent guarantees these exist.
        playerHealth = GetComponent<PlayerHealth>();
        playerInputReader = GetComponent<PlayerInputReader>();
        playerMotor = GetComponent<PlayerMotor>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    private void CreateStates()
    {
        idleState = new PlayerIdleState(this);
        walkState = new PlayerWalkState(this);
        runState = new PlayerRunState(this);
        dashState = new PlayerDashState(this);
        attackState = new PlayerAttackState(this);
        hurtState = new PlayerHurtState(this);
        deadState = new PlayerDeadState(this);
    }
}
