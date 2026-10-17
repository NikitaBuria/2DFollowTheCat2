using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [Header("Player Refrences")]
    [SerializeField, FormerlySerializedAs("movement")] private PlayerMotor playerMotor;
    private PlayerStateMachine playerStateMachine;



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



    

    private void OnEnable()
    {
        playerStateMachine.Initialize(IdleState);   
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


    public void GetReferences()
    {
        if (playerMotor == null) playerMotor = GetComponent<PlayerMotor>();

        if (playerStateMachine == null) playerStateMachine = new PlayerStateMachine();
    }


}
