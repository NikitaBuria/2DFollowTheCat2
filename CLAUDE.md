# 2DFollowTheCat2

Top-down 2D game in Unity 6 (6000.3), URP, new Input System. The Editor can be driven with the Unity CLI (`unity status`, `unity command ...`).

## How to work with me (the developer)

I'm learning Unity and C# by building this game, and I write all the gameplay code myself.
- Explain concepts, syntax and architecture; review my code when I ask ("check my code").
- Point out bugs with the reason and the fix, but don't edit my gameplay scripts unless I explicitly ask.
- Tooling is fine when asked: git, importing/slicing sprites, project settings.

## Player architecture

One job per script:
- **Player**: wires everything, creates the states, forwards `Update`/`FixedUpdate` to the current state. No game logic.
- **PlayerInputReader**: the only script that reads input. Continuous input is polled (`ReadValue`, `IsPressed`); one-off presses are C# events.
- **PlayerStateMachine**: plain class with `Initialize` and `ChangeState` (old `Exit` → swap → new `Enter`). Only it switches states.
- **PlayerState**: abstract base. `Enter`/`Update`/`Exit` abstract, `FixedUpdate` virtual and empty.
- **States** (`Scripts/Player Scripts/PlayerStates/`, one file each): Idle, Walk, Run, Dash, Attack, Hurt, Dead; later Interact and Ability.
- **PlayerMotor** (converted from `Movement.cs`): Rigidbody only. No `Update`/`FixedUpdate`, no input, speeds passed in as parameters.
- **PlayerAnimator**: `PlayerAnim` enum + switch expression to cached `Animator.StringToHash`, throws on unknown values; `SetFacing` drives MoveX/MoveY blend trees.
- Later: PlayerCombat, PlayerAbilities, PlayerInteractor, Health, Inventory, Equipment, Wallet.

Rules:
- States decide **what/when**; components do **how**.
- Input checks, timers and transitions in `Update`; motor calls in `FixedUpdate`.
- Observer events for changes (`OnMoneyChanged`, `OnWeaponChanged`, `OnDied`, inventory `OnChanged`); UI only listens.
- Interfaces: `IInteractable`, `IDamageable`.
- ScriptableObjects for data: `ItemData` → `WeaponData`/`ArmorData` (slot enums), abstract `Ability` with cooldown, `PlayerStats`.
- States referencing the concrete `Player` is accepted coupling.

## Code review checklist

- Every `+=` has a matching `-=` (in `OnEnable`/`OnDisable`).
- No magic numbers in states; tunable values in `PlayerStats` or the Inspector.
- Private fields with `[SerializeField]`; read-only access via `{ get; private set; }`.
- `[RequireComponent]` where a component is needed.
- Consistent naming, correct spelling, class name matches file name.
- Delete dead code (e.g. old `Movement.cs` logic once states replace it).
- Finish and test one system before starting the next; commit after each working step.

## Build order

1. Movement (Idle/Walk/Run) → 2. Dash → 3. Health/Hurt/Dead → 4. Items, Inventory, Wallet → 5. Equipment → 6. Combat → 7. Interactor (doors, merchant) → 8. Abilities → 9. UI
