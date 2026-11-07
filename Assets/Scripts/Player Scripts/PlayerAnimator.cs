using UnityEngine;

// Plays player animations. States ask for an animation by enum; the enum maps to a cached hash.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    // Blend tree parameters
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimationState(PlayerAnimationState state) => animator.Play(ToHash(state));

    public void SetMoveDirection(Vector2 direction)
    {
        animator.SetFloat(MoveXHash, direction.x);
        animator.SetFloat(MoveYHash, direction.y);
    }

    private static int ToHash(PlayerAnimationState state) => state switch
    {
        PlayerAnimationState.Idle => IdleHash,
        PlayerAnimationState.Walk => WalkHash,
        PlayerAnimationState.Run => RunHash,
        PlayerAnimationState.Dash => DashHash,
        PlayerAnimationState.Attack => AttackHash,
        PlayerAnimationState.Hurt => HurtHash,
        PlayerAnimationState.Dead => DeadHash,
        _ => throw new System.ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
