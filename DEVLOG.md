# Devlog

## 2026-11-12 → 2026-11-13 — Maps, building art and a code clean-up

**Done**
- Repaired the map tile kit in place so tiles sit next to each other cleanly, added random grass and animated water brushes.
- Painted the Town (forest clearing, plaza, ponds, the three buildings and the gate) and made the Outdoor river, ruins and map edge block the player.
- Astra's building art and rebuild animations in town; a restore now plays the animation through an event.
- Code review clean-up: removed unused code and debug logs, added missing comments, guarded a hotbar input edge case, gave the Wisp its real health.

**Learned**
- Gameplay scripts should announce what happened (an event) and let a view decide how it looks.
- A tilemap only blocks the player on a layer that has a collider; decoration layers stay walk-through.

**Next**
- Enemy Play-mode test and the combat review, then the boss and the win screen.

## 2026-11-07 → 2026-11-11 — From combat to a connected world

**Done**
- Combat, Health/Hurt/Dead and the WispGrin enemy with its own state machine.
- Loot tables, coin and item pickups, Wallet, Inventory grid with drag/merge/swap/split, 7-slot hotbar, health potion.
- Game states (Gameplay, Inventory, Paused, Shop) with `Time.timeScale` and input maps.
- Save system (JSON + backup + validation) and death respawn with a 25% coin penalty.
- Interaction: NPC dialogue with speech bubbles, the merchant shop (buy and sell).
- Scenes: main menu, town, outdoor and dungeon, connected by doors that save and load through the save system.
- Town restoration: 3 buildings (100/250/500 coins), town tier changes the merchant's look and stock; save version 2 still reads version 1 saves.

**Learned**
- Interfaces (`IInteractable`, `ISaveable`) let one script talk to very different objects without knowing their type.
- Check everything that can fail *before* changing anything (buying: coins, then inventory space, then pay).
- Static fields survive `SceneManager.LoadScene`; scene objects don't. A save file can carry the rest.
- UI should only listen to events and report clicks; the game logic lives elsewhere.

**Next**
- Combat review fixes and the enemy Play-mode test, then the boss and the win screen.

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
