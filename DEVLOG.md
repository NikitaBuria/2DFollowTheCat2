# Devlog

## 2026-10-18 — Player state machine: Idle / Walk / Run

**Done**
- Moved `PlayerState` and `PlayerStateMachine` into `PlayerStates/`.
- `PlayerStateMachine.ChangeState` now calls `Enter()` on the new state (it only called `Exit()` on the old one before).
- `Player` forwards Unity's `Update`/`FixedUpdate` to the current state, and initializes the state machine in `Start` instead of `OnEnable`.
- `PlayerWalkState`: transitions checked in `Update`, movement in `FixedUpdate`; switches to Run while sprint is held.
- `PlayerRunState` implemented (mirror of Walk at run speed).
- Walk/Run speeds moved out of the states into Inspector fields on `Player`.
- `PlayerMotor` reduced to Rigidbody only (`Move`, `Stop`); removed unused fields.
- `PlayerInputReader`: `OnSprintPerformed` renamed to `IsSprintHeld`.
- Added `[RequireComponent]` to `Player` and `PlayerMotor`.

**Learned**
- Unity only calls `Update`/`FixedUpdate` on MonoBehaviours attached to a GameObject. Plain C# classes (states, the state machine) need a MonoBehaviour to call them each frame; `Player` does that.
- `Initialize` runs once: it sets the current state and calls `Enter`. Nothing keeps running unless something calls the state every frame.
- Input checks and state transitions go in `Update`; Rigidbody movement goes in `FixedUpdate`.
- Watch the order in a ternary: `condition ? valueIfTrue : valueIfFalse`.

**Next**
- Test Idle ↔ Walk ↔ Run in Play mode.
- Build step 2: Dash.
