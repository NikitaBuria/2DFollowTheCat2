# 2DFollowTheCat2

A top-down 2D action dungeon crawler made in **Unity 6** (URP, new Input System).

You play the Lost Wanderer, who finds a town abandoned after a monster attack. Go down into the dungeon, fight monsters, collect loot, sell it for coins and spend the coins to restore the town. Every restored building unlocks better gear. The final boss drops the last piece the town needs.

> **Status:** work-in-progress prototype. I'm building it to learn Unity and C#, and it's part of my portfolio. The focus is on clean, readable architecture rather than content.

## Core loop

```
Dungeon → fight → loot → sell to the merchant → restore the town → better gear → deeper floors → boss
```

## Features

| System | Status |
| --- | --- |
| 4-direction movement (Idle, Walk, Run) | ✅ |
| Melee combat with weapon data (range, arc, knockback) | ✅ |
| Health, Hurt (knockback + i-frames), Dead | ✅ |
| Enemy AI: Idle ⇄ Patrol → Chase → Windup → Attack, Hurt, Dead | ✅ |
| Loot tables, coin and item pickups | ✅ |
| Wallet and coin counter | ✅ |
| Inventory grid: drag to move, merge or swap; right-drag to split | ✅ |
| 7-slot hotbar | ✅ |
| Game states (Gameplay, Inventory, Paused) | ✅ |
| Usable items (health potion, keys 1–7) | 🚧 |
| Save system | ⏳ |
| Interaction: doors, merchant, town restoration | ⏳ |
| Boss and win screen | ⏳ |
| Dash, equipment, abilities | ⏳ |
| Audio and UI polish | ⏳ |

## Architecture

The code follows a few rules: each script has one job, states decide *what* happens and *when*, and components decide *how*.

### Finite state machines
- The player and the enemies each have their own state machine: a plain C# `StateMachine`, an abstract `State` with `Enter`/`Update`/`FixedUpdate`/`Exit`, and one class per state.
- The `Player` and `Enemy` hubs only wire things together and pass `Update`/`FixedUpdate` on to the current state.
- Components do the work and hold no game logic: `PlayerMotor` (Rigidbody), `PlayerAnimator`, `PlayerCombat`, `EnemyDetection` and so on.
- Timers and transitions run in `Update`; physics runs in `FixedUpdate`.

### Input
- `PlayerInputReader` is the only script that reads input (new Input System).
- States poll only the inputs they accept, so a Hurt or Dead player simply ignores attack input.
- The Player action map is only active during Gameplay, so menus block movement automatically.

### Events (Observer pattern)
- Changes are announced with C# events: `Health.OnDamaged` / `OnDied`, `Wallet.OnMoneyChanged`, `Inventory.OnChanged` and `GameStateManager.OnGameStateChanged`.
- The UI only listens and never changes game data.
- Every `+=` has a matching `-=` in `OnEnable`/`OnDisable`.

### Interfaces
- `IDamageable`: anything that can be hit (player and enemies share one `Health` component).
- `IPickable`: coin and item pickups.
- `IUsable`: items that do something when used (`HealthPotionData`).

### ScriptableObjects for data
- `CharacterData` → `EnemyData`: health, speeds, detection, attack timing and loot.
- `ItemData` → `HealthPotionData`, plus `WeaponData` and `LootTable`.
- Designers can tune values in the Inspector without touching code, so there are no magic numbers in states.

### Game state and UI
- `GameStateManager` is the single source of truth for the current mode and sets `Time.timeScale`.
- `UIManager` only shows and hides panels.
- Content scripts (`InventoryUI`, `HotbarUI`, `CoinCounterUI`) only draw data and never need to know the game state.

### Animation
- Animator Controllers have no transitions. Every state is a 4-direction blend tree.
- The code picks the animation with `Animator.Play` in each state's `Enter`, using cached hashes from an enum.

## Project structure

```
Assets/
├── Scripts/
│   ├── Player Scripts/      Player hub, input, motor, animator, combat, states
│   ├── Enemy/               Enemy hub, components, states
│   ├── Combat/              Health, DamageData, IDamageable
│   ├── Items/               Pickups, IPickable, IUsable
│   ├── Inventory/           Inventory, InventorySlot, Wallet
│   ├── Core/                GameState, GameStateManager
│   ├── UI/                  UIManager, InventoryUI, HotbarUI, slot views
│   └── ScriptableObjects/   Character, enemy, item, weapon and loot data
├── Editor/                  Custom inspectors and asset builders
├── Prefabs/
└── Art/
```

## Controls

| Action | Key |
| --- | --- |
| Move | WASD |
| Run | Shift (hold) |
| Attack | Left mouse |
| Dash *(coming soon)* | Space |
| Use hotbar item *(coming soon)* | 1–7 |
| Inventory | Tab |
| Pause | Escape |
| Inventory: move / split a stack | Left-drag / right-drag |

## Getting started

1. Clone the repository.
2. Open it with **Unity 6000.3** or newer, using Unity Hub.
3. Open `Assets/Scenes/SampleScene.unity` and press Play.

## Credits

- Code: me, as a learning and portfolio project.
- Animations: Astra.
