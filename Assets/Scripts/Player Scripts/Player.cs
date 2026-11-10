using UnityEngine;
using UnityEngine.Serialization;

// Wires the player together: creates the states and forwards Update/FixedUpdate to the current one.
// No game logic here; states decide what happens, components do the work.
[RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor), typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerCombat), typeof(Health), typeof(Inventory))]
public class Player : MonoBehaviour
{
    // Components on this GameObject. Found with GetComponent in Awake, so nothing to assign in the Inspector.
    public Health playerHealth { get; private set; }
    public PlayerInputReader playerInputReader { get; private set; }
    public PlayerMotor playerMotor { get; private set; }
    public PlayerAnimator playerAnimator { get; private set; }
    public PlayerCombat playerCombat { get; private set; }
    public Inventory playerInventory { get; private set; }
    public PlayerStateMachine playerStateMachine { get; private set; }


    [Header("Data")]
    // Base stats (speeds, max health). States read them through player.playerData.
    [field: SerializeField, FormerlySerializedAs("<characterData>k__BackingField")]
    public CharacterData playerData { get; private set; }

    // The last hit that landed; the Hurt state reads its direction and knockback.
    public DamageData lastDamage { get; private set; }

    // Which way the player faces; used for animations and attacks.
    public Vector2 facingDirection { get; private set; } = Vector2.down;

    

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
        if (playerData == null)
            Debug.LogError($"{name}: Player has no PlayerData assigned.", this);

        GetReferences();
        CreateStates();
        playerHealth.Initialize(playerData.maxHealth);
    }

    private void Start()
    {
        playerStateMachine.Initialize(idleState);
    }

    private void OnEnable()
    {
        playerHealth.OnDied += HandlePlayerDeath;
        playerHealth.OnDamaged += HandlePlayerHurt;
    }

    private void OnDisable()
    {
        playerHealth.OnDied -= HandlePlayerDeath;
        playerHealth.OnDamaged -= HandlePlayerHurt;
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

    // Called by RespawnManager: back at the spawn point with full health and spawn protection.
    public void Respawn(Vector2 position)
    {
        playerMotor.Teleport(position);
        playerHealth.Initialize(playerData.maxHealth);
        playerHealth.StartInvincibility(playerData.invincibleTimer);
        playerStateMachine.ChangeState(idleState);
    }

    private void HandlePlayerDeath()
    {
        playerStateMachine.ChangeState(deadState);
    }


    // OnDamaged also fires on the killing hit (before OnDied), so only go to Hurt while alive.
    private void HandlePlayerHurt(DamageData damage)
    {
        lastDamage = damage;
        if (playerHealth.isAlive)
            playerStateMachine.ChangeState(hurtState);
    }

    private void GetReferences()
    {
        playerStateMachine = new PlayerStateMachine();

        // RequireComponent guarantees these exist.
        playerHealth = GetComponent<Health>();
        playerInputReader = GetComponent<PlayerInputReader>();
        playerMotor = GetComponent<PlayerMotor>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerInventory = GetComponent<Inventory>();
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
