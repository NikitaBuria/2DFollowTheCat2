using UnityEngine;

// The guide: shows up for the current tutorial step when it belongs to this scene, walks the step's
// path of GuideMarkers (waiting for the player when he leads) and says the step's lines through
// NpcDialogue. Reports DialogueEnded to TutorialProgress; every other step ending comes from
// TutorialTriggers. Hidden when the current step is in another scene or the tutorial is done.
[RequireComponent(typeof(NpcDialogue), typeof(Collider2D))]
public class GuideCat : MonoBehaviour
{
    [SerializeField] private TutorialProgress progress;
    [SerializeField] private Player player;
    [Tooltip("The child with the SpriteRenderer, Animator and GuideCatAnimator.")]
    [SerializeField] private GuideCatAnimator catAnimator;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float walkSpeed = 2.5f;
    [Tooltip("While leading, he only walks on while the player is this close.")]
    [SerializeField, Min(0.5f)] private float leadDistance = 4f;
    [SerializeField, Min(0.01f)] private float arriveDistance = 0.05f;

    [Header("Following (steps with followsPlayer)")]
    [Tooltip("He stays about this far from the player.")]
    [SerializeField, Min(0.5f)] private float followDistance = 3.5f;
    [Tooltip("Farther than this, he runs to catch up.")]
    [SerializeField, Min(1f)] private float runDistance = 4f;
    [SerializeField, Min(0.1f)] private float runSpeed = 6f;

    [Header("Talking")]
    [Tooltip("He starts his lines when the player comes this close.")]
    [SerializeField, Min(0.5f)] private float talkDistance = 3f;

    private NpcDialogue dialogue;
    private Collider2D interactCollider;

    private GuideStep step;
    private int pathIndex;
    private bool hasSpoken;
    private bool placed;           // already standing in this scene (later steps walk instead of jumping)
    private bool changingLines;    // SetLines ends the old conversation; that end is not "dialogue finished"

    private bool isHere => step != null && step.sceneName == gameObject.scene.name;
    // "Follow me", then lead; following: talk whenever close; otherwise walk there, then talk.
    private bool talksFirst => step.leadsPlayer || step.followsPlayer;

    private void Awake()
    {
        dialogue = GetComponent<NpcDialogue>();
        interactCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        progress.OnStepChanged += ShowStep;
        dialogue.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        progress.OnStepChanged -= ShowStep;
        dialogue.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void Start()
    {
        ShowStep(progress.currentStep);
    }

    private void ShowStep(GuideStep newStep)
    {
        step = newStep;
        catAnimator.gameObject.SetActive(isHere);
        interactCollider.enabled = isHere;

        changingLines = true;
        dialogue.SetLines(isHere ? step.lines : null);
        changingLines = false;

        if (!isHere) return;

        hasSpoken = step.lines == null || step.lines.Length == 0;
        pathIndex = 0;

        // First step in this scene: appear at the start of the path instead of walking in.
        if (!placed && TryGetMarker(0, out Vector3 start))
        {
            transform.position = start;
            pathIndex = 1;
        }
        placed = true;
    }

    private void Update()
    {
        if (!isHere) return;

        Vector2 toPlayer = player.transform.position - transform.position;
        bool playerClose = toPlayer.magnitude <= talkDistance;
        bool pathDone = !TryGetMarker(pathIndex, out Vector3 target);

        if (!hasSpoken && playerClose && (talksFirst || pathDone))
        {
            hasSpoken = true;
            dialogue.Interact(player);
        }

        if (step.followsPlayer)
        {
            Follow(toPlayer);
            return;
        }

        if (dialogue.isTalking)
        {
            Face(toPlayer, GuideCatAnimationState.Talk);
            return;
        }

        bool mayWalk = !pathDone && (!step.leadsPlayer || (hasSpoken && toPlayer.magnitude <= leadDistance));
        if (!mayWalk)
        {
            Face(toPlayer, GuideCatAnimationState.Idle);
            return;
        }

        Vector3 next = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
        Face(next - transform.position, GuideCatAnimationState.Walk);
        transform.position = next;

        if (Vector3.Distance(transform.position, target) <= arriveDistance) pathIndex++;
    }

    // Stays about followDistance from the player, running when far behind; keeps talking while he walks.
    private void Follow(Vector2 toPlayer)
    {
        float distance = toPlayer.magnitude;
        if (distance <= followDistance)
        {
            Face(toPlayer, dialogue.isTalking ? GuideCatAnimationState.Talk : GuideCatAnimationState.Idle);
            return;
        }

        bool run = distance > runDistance;
        float move = Mathf.Min((run ? runSpeed : walkSpeed) * Time.deltaTime, distance - followDistance);
        transform.position += (Vector3)(toPlayer.normalized * move);
        Face(toPlayer, run ? GuideCatAnimationState.Run : GuideCatAnimationState.Walk);
    }

    private void HandleDialogueEnded()
    {
        if (changingLines || !isHere) return;
        progress.TryComplete(GuideStepCompletion.DialogueEnded);
    }

    private bool TryGetMarker(int index, out Vector3 position)
    {
        position = default;
        if (step.catPath == null || index >= step.catPath.Length) return false;

        GuideMarker marker = GuideMarker.Find(step.catPath[index]);
        if (marker == null)
        {
            Debug.LogWarning($"{name}: no GuideMarker '{step.catPath[index]}' in this scene (step {step.id}).", this);
            return false;
        }

        position = marker.transform.position;
        return true;
    }

    private void Face(Vector2 direction, GuideCatAnimationState state)
    {
        catAnimator.SetMoveDirection(direction);
        catAnimator.SetAnimationState(state);
    }
}
