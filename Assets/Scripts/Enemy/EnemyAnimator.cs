using UnityEngine;

// Plays enemy animations, the same way PlayerAnimator does: enum -> cached hash -> Play.
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int WindupHash = Animator.StringToHash("Windup");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int SpawnHash = Animator.StringToHash("Spawn");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    // Blend tree parameters
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    private Animator animator;
    private readonly System.Collections.Generic.HashSet<EnemyAnimationState> reportedMissing = new();

    // A placeholder enemy (a plain square) has no controller yet; skip animation instead of spamming warnings.
    private bool hasController => animator.runtimeAnimatorController != null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimationState(EnemyAnimationState state)
    {
        if (!hasController) return;

        int hash = ToHash(state);
        if (animator.HasState(0, hash)) animator.Play(hash);
        else WarnMissingStateOnce(state);
    }

    public void SetMoveDirection(Vector2 direction)
    {
        if (!hasController) return;

        animator.SetFloat(MoveXHash, direction.x);
        animator.SetFloat(MoveYHash, direction.y);
    }

    // States the code played but the controller lacks; each is warned about once.
    // Only states this enemy actually uses are checked (Spawn is boss-only).
    private void WarnMissingStateOnce(EnemyAnimationState state)
    {
        if (!reportedMissing.Add(state)) return;
        Debug.LogWarning($"{name}: Animator '{animator.runtimeAnimatorController.name}' has no '{state}' state.", this);
    }

    private static int ToHash(EnemyAnimationState state) => state switch
    {
        EnemyAnimationState.Idle => IdleHash,
        EnemyAnimationState.Walk => WalkHash,
        EnemyAnimationState.Run => RunHash,
        EnemyAnimationState.Windup => WindupHash,
        EnemyAnimationState.Attack => AttackHash,
        EnemyAnimationState.Hurt => HurtHash,
        EnemyAnimationState.Spawn => SpawnHash,
        EnemyAnimationState.Dead => DeadHash,
        _ => throw new System.ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
