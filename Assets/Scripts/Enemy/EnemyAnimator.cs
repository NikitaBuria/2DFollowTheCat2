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
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    // Blend tree parameters
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    private Animator animator;

    // A placeholder enemy (a plain square) has no controller yet; skip animation instead of spamming warnings.
    private bool hasController => animator.runtimeAnimatorController != null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        WarnAboutMissingStates();
    }

    public void SetAnimationState(EnemyAnimationState state)
    {
        if (!hasController) return;

        // A missing state was already reported once in Awake; skip it instead of warning every frame.
        int hash = ToHash(state);
        if (animator.HasState(0, hash)) animator.Play(hash);
    }

    public void SetMoveDirection(Vector2 direction)
    {
        if (!hasController) return;

        animator.SetFloat(MoveXHash, direction.x);
        animator.SetFloat(MoveYHash, direction.y);
    }

    // Every EnemyAnimationState needs an Animator state with the same name on layer 0.
    // Lists all missing ones in one warning, so a new enemy's controller is easy to fix.
    private void WarnAboutMissingStates()
    {
        if (!hasController) return;

        var missing = new System.Collections.Generic.List<string>();
        foreach (EnemyAnimationState state in System.Enum.GetValues(typeof(EnemyAnimationState)))
            if (!animator.HasState(0, ToHash(state))) missing.Add(state.ToString());

        if (missing.Count > 0)
            Debug.LogWarning($"{name}: Animator '{animator.runtimeAnimatorController.name}' is missing states: {string.Join(", ", missing)}", this);
    }

    private static int ToHash(EnemyAnimationState state) => state switch
    {
        EnemyAnimationState.Idle => IdleHash,
        EnemyAnimationState.Walk => WalkHash,
        EnemyAnimationState.Run => RunHash,
        EnemyAnimationState.Windup => WindupHash,
        EnemyAnimationState.Attack => AttackHash,
        EnemyAnimationState.Hurt => HurtHash,
        EnemyAnimationState.Dead => DeadHash,
        _ => throw new System.ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
