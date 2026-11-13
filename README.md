# 2DFollowTheCat2

A top-down 2D action dungeon crawler made in **Unity 6** (URP, new Input System).

You play the Lost Wanderer, who finds a town abandoned after a monster attack. Walk from the town to the dungeon, fight monsters, collect loot, sell it to the merchant and spend the coins to restore the town. Every restored building unlocks better gear. The final boss drops the last piece the town needs.

> **Status:** work-in-progress prototype. I'm building it to learn Unity and C#, and it's part of my portfolio. The focus is on clean, readable architecture rather than content.

## Core loop

```
Town → Outdoor → Dungeon → fight → loot → back to town → sell to the merchant → restore the town → better gear → boss
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
| Game states (Gameplay, Inventory, Paused, Shop) | ✅ |
| Usable items (health potion, keys 1–7) | ✅ |
| Save system (JSON, backup file, validation) and death respawn | ✅ |
| Interaction: NPC dialogue with speech bubbles | ✅ |
| Merchant shop: buy from stock, sell by drag and drop | ✅ |
| Scenes: main menu, town, outdoor, dungeon, connected by doors | ✅ |
| Town restoration: 3 buildings, town tiers, better merchant stock, rebuild animations | ✅ |
| Painted maps: town, outdoor river crossing, dungeon cave with collision | ✅ |
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
- `ISaveable`: `Health`, `Wallet`, `Inventory` and `TownProgress` save, validate and load their own data.
- `IInteractable`: everything the player can press E on (NPC dialogue, the merchant, scene doors, buildings).

### ScriptableObjects for data
- `CharacterData` → `EnemyData`: health, speeds, detection, attack timing and loot.
- `ItemData` → `HealthPotionData`, plus `WeaponData`, `LootTable` and the merchant's `ShopStock`.
- Designers can tune values in the Inspector without touching code, so there are no magic numbers in states.

### Game state and UI
- `GameStateManager` is the single source of truth for the current mode and sets `Time.timeScale`.
- `UIManager` only shows and hides panels.
- Content scripts (`InventoryUI`, `HotbarUI`, `CoinCounterUI`) only draw data and never need to know the game state.

### Save system
- `SaveFile` is the only code that touches the disk: JSON, written to a temp file first, with a `.bak` copy kept.
- `SaveManager` asks every `ISaveable` to validate the data before anything loads, and falls back to the backup if the main file is broken.
- Items are saved by ID and turned back into assets through an `ItemDatabase`.
- `RespawnManager`: on death the player loses 25% of their coins and respawns at the dungeon start; the game then saves so the penalty sticks.

### Scenes
- `MainMenu → Town ⇄ Outdoor ⇄ Dungeon`. Each game scene has its own copy of a `Core` prefab (player, HUD, managers, cameras), so there is no `DontDestroyOnLoad`.
- A `SceneDoor` saves the game, then loads the next scene. That scene's `SaveManager` loads the data back, and `SceneArrival` places the player at the matching `SpawnPoint`. Every door is an autosave.

### Town restoration
- Each building is a `BuildingData` ScriptableObject (name, cost, optional required item).
- `TownProgress` keeps which buildings are restored, saves them through `ISaveable` and announces changes with an event. The town tier (1 + restored buildings) changes the merchant's look and what he sells.
- `RestorableBuilding` only asks `TownProgress` and swaps between its ruined and restored art. When the player restores it, it fires `OnRestored`; `BuildingRebuildView` listens and plays the rebuild animation, so the gameplay script never touches its visuals.

### Interaction and the merchant
- `PlayerInteractor` finds the nearest `IInteractable` in front of the player; a world-space prompt shows `[E] Talk`, `[E] Trade` or `[E] Enter`.
- The `Merchant` only asks for the Shop game state and does the trading; the shop UI builds its entries from `ShopStock` and reports clicks and drops back.
- Buying checks coins and inventory space before anything changes, so a failed purchase never loses coins.

### Maps
- Scenes are painted with Unity's Tilemap and Tile Palette, using one fantasy pixel-art kit (128 px per cell, the same scale as the characters).
- Each scene has a `Ground` layer (walkable), a `Walls` layer with a merged collider (trees, water, ruins block the player) and a `Decor` layer (bushes and flowers you walk through).
- Random brushes (Rule Tiles) and animated water tiles keep large areas from looking repeated.

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
│   ├── Core/                GameStateManager, RespawnManager, scene loading and spawn points
│   ├── Save/                SaveFile, SaveManager, SaveData, ISaveable
│   ├── Interaction/         PlayerInteractor, NpcDialogue, SceneDoor, IInteractable
│   ├── Merchant/            Merchant, ShopStock, MerchantAppearance
│   ├── Town/                BuildingData, TownProgress, RestorableBuilding, BuildingRebuildView
│   ├── UI/                  UIManager, inventory, hotbar, shop, main menu
│   └── ScriptableObjects/   Character, enemy, item, weapon and loot data
├── Editor/                  Custom inspectors and asset builders
├── Prefabs/
└── Art/                     Characters, items, UI, map tiles and palettes
```

## Controls

| Action | Key |
| --- | --- |
| Move | WASD |
| Run | Shift (hold) |
| Attack | Left mouse |
| Dash *(coming soon)* | Space |
| Use hotbar item | 1–7 |
| Interact (talk, trade, enter) | E |
| Inventory | Tab |
| Pause | Escape |
| Inventory: move / split a stack | Left-drag / right-drag |
| Shop: buy / sell | Click an item / drag a stack onto the merchant panel |
| Debug save / load | F5 / F9 |

## Getting started

1. Clone the repository.
2. Open it with **Unity 6000.3** or newer, using Unity Hub.
3. Open `Assets/Scenes/MainMenu.unity` and press Play. Play starts in the town and continues your save if there is one.

## Credits

- Code: me, as a learning and portfolio project.
- Art and animations: Astra.
