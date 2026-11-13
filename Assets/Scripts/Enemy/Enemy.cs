using UnityEngine;


// Wires the enemy together, like Player does: creates the states, forwards Update/FixedUpdate
// to the current one and turns health events into state changes. States decide what happens.
[RequireComponent(typeof(EnemyMotor), typeof(EnemyDetection), typeof(EnemyCombat))]
[RequireComponent(typeof(EnemyAnimator), typeof(Health))]
public class Enemy : MonoBehaviour
{
    [Header("Data")]
    [field: SerializeField] public EnemyData enemyData { get; private set; }

    [Header("Patrol")]
    // Optional. Positions are captured in Awake, so the points can be children of the enemy.
    // No points = the enemy stands guard where it was placed.
    [SerializeField] private Transform[] patrolPoints = new Transform[0];

    // Components on this GameObject (RequireComponent guarantees them).
    public EnemyMotor enemyMotor { get; private set; }
    public EnemyDetection enemyDetection { get; private set; }
    public EnemyCombat enemyCombat { get; private set; }
    public EnemyAnimator enemyAnimator { get; private set; }
    public Health health { get; private set; }

    public EnemyStateMachine enemyStateMachine { get; private set; }

    // States
    public EnemyIdleState idleState { get; private set; }
    public EnemyPatrolState patrolState { get; private set; }
    public EnemyChaseState chaseState { get; private set; }
    public EnemyWindupState windupState { get; private set; }
    public EnemyAttackState attackState { get; private set; }
    public EnemyHurtState hurtState { get; private set; }
    public EnemyDeadState deadState { get; private set; }

    public Vector2 facingDirection { get; private set; } = Vector2.down;
    public Transform target => enemyDetection.target;

    // The last hit taken; EnemyHurtState reads it for the knockback.
    public DamageData lastDamage { get; private set; }

    private Vector2[] patrolRoute;
    private int patrolIndex;

    public bool hasPatrolRoute => patrolRoute.Length > 0;
    public Vector2 patrolDestination => patrolRoute[patrolIndex];

    private void Awake()
    {
        GetReferences();

        if (enemyData == null)
        {
            Debug.LogError($"{name}: Enemy has no EnemyData assigned.", this);
            enabled = false;
            return;
        }

        health.Initialize(enemyData.maxHealth);
        enemyDetection.Initialize(enemyData);
        enemyCombat.Initialize(enemyData);

        BuildPatrolRoute();
        CreateStates();
    }

    private void Start()
    {
        enemyStateMachine.Initialize(idleState);
    }

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
    }

    private void Update()
    {
        if (health.isAlive) enemyDetection.Refresh();
        enemyStateMachine.currentState?.Update();
    }

    private void FixedUpdate()
    {
        enemyStateMachine.currentState?.FixedUpdate();
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        facingDirection = direction.normalized;
        enemyAnimator.SetMoveDirection(facingDirection);
    }

    public void FaceTarget()
    {
        if (target != null) SetFacingDirection(target.position - transform.position);
    }

    public bool ReachedPatrolPoint()
    {
        return Vector2.Distance(transform.position, patrolDestination) <= enemyData.arriveDistance;
    }

    public void AdvancePatrolPoint()
    {
        patrolIndex = (patrolIndex + 1) % patrolRoute.Length;
    }

    private void HandleDamaged(DamageData damage)
    {
        lastDamage = damage;
        // A killing hit goes to Dead through OnDied instead.
        if (health.isAlive) enemyStateMachine.ChangeState(hurtState);
    }

    private void HandleDied()
    {
        enemyStateMachine.ChangeState(deadState);
    }

    private void GetReferences()
    {
        enemyStateMachine = new EnemyStateMachine();

        enemyMotor = GetComponent<EnemyMotor>();
        enemyDetection = GetComponent<EnemyDetection>();
        enemyCombat = GetComponent<EnemyCombat>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        health = GetComponent<Health>();
    }

    private void BuildPatrolRoute()
    {
        var points = new System.Collections.Generic.List<Vector2>();
        foreach (Transform point in patrolPoints)
            if (point != null) points.Add(point.position);

        patrolRoute = points.ToArray();
    }

    private void CreateStates()
    {
        idleState = new EnemyIdleState(this);
        patrolState = new EnemyPatrolState(this);
        chaseState = new EnemyChaseState(this);
        windupState = new EnemyWindupState(this);
        attackState = new EnemyAttackState(this);
        hurtState = new EnemyHurtState(this);
        deadState = new EnemyDeadState(this);
    }

    // Yellow = notices a target, grey = gives up, red = attack reach, cyan = patrol route.
    private void OnDrawGizmosSelected()
    {
        Vector3 position = transform.position;

        if (enemyData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, enemyData.detectionRange);
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(position, enemyData.loseTargetRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, enemyData.attackRange);
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform from = patrolPoints[i];
            Transform to = patrolPoints[(i + 1) % patrolPoints.Length];
            if (from != null && to != null) Gizmos.DrawLine(from.position, to.position);
        }
    }
}
