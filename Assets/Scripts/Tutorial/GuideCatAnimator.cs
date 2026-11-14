using UnityEngine;

// Plays the GuideCat's animations, like PlayerAnimator: an enum maps to a cached hash, and MoveX/MoveY
// pick the direction in each blend tree. Only plays when the state changes, so a looping clip
// isn't restarted every frame.
[RequireComponent(typeof(Animator))]
public class GuideCatAnimator : MonoBehaviour
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int TalkHash = Animator.StringToHash("Talk");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    private Animator animator;
    private GuideCatAnimationState? currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // The Animator resets when the cat is re-enabled, so the next request plays again.
    private void OnDisable()
    {
        currentState = null;
    }

    public void SetAnimationState(GuideCatAnimationState state)
    {
        if (currentState == state) return;

        currentState = state;
        animator.Play(ToHash(state));
    }

    // Four directions, like the player: the bigger axis wins.
    public void SetMoveDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        Vector2 facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
            ? new Vector2(Mathf.Sign(direction.x), 0f)
            : new Vector2(0f, Mathf.Sign(direction.y));

        animator.SetFloat(MoveXHash, facing.x);
        animator.SetFloat(MoveYHash, facing.y);
    }

    private static int ToHash(GuideCatAnimationState state) => state switch
    {
        GuideCatAnimationState.Idle => IdleHash,
        GuideCatAnimationState.Talk => TalkHash,
        GuideCatAnimationState.Walk => WalkHash,
        GuideCatAnimationState.Run => RunHash,
        _ => throw new System.ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
