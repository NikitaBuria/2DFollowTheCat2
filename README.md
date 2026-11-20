# 2DFollowTheCat2

A top-down 2D action dungeon crawler made in **Unity 6** (URP 2D renderer, new Input System).

You play the Lost Wanderer, who wakes up in a dark cave. Kitty, a guide cat, leads you to a town abandoned after a monster attack. Walk from the town to the dungeon, fight monsters, collect loot, sell it to the merchant and spend the coins to restore the town. The King Wisp boss drops the Marble Stone the town needs, and restoring every building wins the game.

> **Status:** playable prototype. I'm building it to learn Unity and C#, and it's part of my portfolio. The focus is on clean, readable architecture rather than content.

## Core loop

```
Cave intro → Town → Outdoor → Dungeon → fight → loot → back to town → sell to the merchant → restore the town (better wares) → win
```

## Features

| System | Status |
| --- | --- |
| 4-direction movement (Idle, Walk, Run) and Dash with i-frames and cooldown | ✅ |
| Melee combat with weapon data (damage, range, arc, knockback, timing) | ✅ |
| Health, Hurt (knockback + i-frames), Dead, animated health bar with a cat badge | ✅ |
| Enemy AI: Idle ⇄ Patrol → Chase → Windup → Attack, Hurt, Dead | ✅ |
| Enemy respawn from an object pool | ✅ |
| Boss: King Wisp with super armor, minion summoning (pooled) and a random spawn | ✅ |
| Loot tables, coin and item pickups | ✅ |
| Wallet, inventory grid (drag to move, merge, swap; right-drag to split), 7-slot hotbar | ✅ |
| Equipment: helmet, armor, weapon, boots; armor defense; worn gear drawn on the player | ✅ |
| Usable items: health potion, strength elixir (timed damage buff) | ✅ |
| Game states (Gameplay, Inventory, Paused, Shop, Won) | ✅ |
| Save system (JSON, verified writes, backup file, validation with repair, versioning with migrations) and death respawn | ✅ |
| Interaction: NPC dialogue with speech bubbles, scene doors | ✅ |
| Merchant shop: stock grows as the town is restored, sell by drag and drop, trade feedback | ✅ |
| Town restoration: 3 buildings, town tiers, rebuild animations, win screen | ✅ |
| Guide-cat tutorial: Kitty walks you through a step-by-step intro from the cave to the first loot | ✅ |
| Scenes: main menu, opening cave, town, outdoor, dungeon | ✅ |
| 2D lighting: per-scene darkness, a lantern that gives light, flickering torches | ✅ |
| Audio: mixer with Music / SFX / Ambience, volume settings, sound per character and item | ✅ |
| Pause menu, settings, controls screen | ✅ |
| Abilities | ⏳ |

## Architecture

The code follows a few rules: each script has one job, states decide *what* happens and *when*, and components decide *how*.

### Finite state machines
- The player and the enemies each have their own state machine: a plain C# state machine class, an abstract state with `Enter`/`Update`/`FixedUpdate`/`Exit`, and one class per state.
- Only the state machine switches states. It fires `OnStateChanged`, so other scripts (sound, for example) can react without the states knowing about them.
- The `Player` and `Enemy` hubs only wire things together and pass `Update`/`FixedUpdate` on to the current state.
- Components do the work and hold no game logic: `PlayerMotor` (Rigidbody), `PlayerAnimator`, `PlayerCombat`, `EnemyDetection` and so on.
- Timers and transitions run in `Update`; physics runs in `FixedUpdate`.
- A boss is an `Enemy` subclass: `KingWisp` adds a summon state and an `EnemySummoner` component.

### Input
- `PlayerInputReader` is the only script that reads input (new Input System).
- States poll only the inputs they accept, so a Hurt or Dead player simply ignores attack input.
- The Player action map is only active during Gameplay, so menus block movement automatically.

### Events (Observer pattern)
- Changes are announced with C# events: `Health.OnDamaged` / `OnDied` / `OnHealthChanged`, `Wallet.OnMoneyChanged`, `Inventory.OnChanged`, `Equipment.OnEquipmentChanged`, `GameStateManager.OnGameStateChanged`, `TownProgress.OnProgressChanged`.
- The UI, audio and visuals only listen and never change game data.
- Every `+=` has a matching `-=` in `OnEnable`/`OnDisable`.

### Interfaces
- `IDamageable`: anything that can be hit (player and enemies share one `Health` component).
- `IDamageModifier`: reduces incoming damage (worn armor).
- `IPickable`: coin and item pickups.
- `IUsable`: items that do something when used (health potion, strength elixir).
- `ISaveable`: `Health`, `Wallet`, `Inventory`, `Equipment`, `TownProgress`, `TutorialProgress` and `SceneArrival` save, validate and load their own data.
- `IInteractable`: everything the player can press E on (NPC dialogue, the merchant, scene doors, buildings).

### ScriptableObjects for data
- `CharacterData` → `EnemyData` → `BossData`: health, speeds, detection, attack timing, loot and sounds.
- `ItemData` → `EquipmentData` → `WeaponData` / `ArmorData`, plus usable items, `LootTable`, `ShopStock`, `BuildingData`, `GuideStep` and `SoundData`.
- A new weapon or enemy brings its own stats and sounds; values are tuned in the Inspector, so there are no magic numbers in states.

### Object pooling
- `EnemySpawner` keeps one Wisp on each spawn point from an `ObjectPool<Enemy>`; a dead Wisp goes back to the pool and comes back after a delay.
- The boss's `EnemySummoner` recycles its minions the same way.

### Equipment
- `Equipment` holds one item per slot and swaps with the inventory, so equipping never needs free space.
- `EquipmentLayerView` draws worn gear as paper-doll layers: each frame it finds the body's current sprite and shows the matching frame of the item, so one set of animations works for every item.
- Weapons can give light: `LanternLight` reads the light settings from the held weapon's data.

### Audio
- One `SoundData` asset per sound (clips, mixer group, volume, pitch range).
- One sound controller per character (`PlayerSoundController`, `EnemySoundController`) listens to gameplay events and plays the sound from the data; gameplay code never touches audio.
- Each sound plays on its own voice, so a footstep never changes a sound that is still playing.
- `VolumeSettings` stores the Music, SFX and Ambience sliders and applies them to the Audio Mixer.

### Game state and UI
- `GameStateManager` is the single source of truth for the current mode and sets `Time.timeScale`.
- `UIManager` only shows and hides panels.
- Content scripts (`InventoryUI`, `HotbarUI`, `CoinCounterUI`, `HealthBarUI`) only draw data and never need to know the game state.

### Save system
- `SaveFile` is the only code that touches the disk: JSON written to a temp file, read back to verify it, then swapped in with a `.bak` copy kept. A file locked for a moment (antivirus, cloud sync) is retried.
- `SaveManager` asks every `ISaveable` to validate the data before anything loads, and falls back to the backup if the main file is broken, so a bad save never half-loads.
- Repair, don't reject: validation refuses only what can't be fixed. Single bad entries (an unknown item, an out-of-range value) are repaired while loading, so one renamed item costs that item, not the whole save.
- Saves are versioned. Older saves load: each system fills in the parts an old save lacks, and `SaveMigration` upgrades renamed item IDs step by step. A save from a newer version of the game is never loaded or overwritten.
- The game saves at every door, on Exit from the pause menu (it stays in the menu if the save fails), after restoring a building, after a respawn and when the window closes. The save also stores the scene and position, so Continue returns you there.
- Items are saved by ID and turned back into assets through an `ItemDatabase`.
- `RespawnManager`: on death the player loses 25% of their coins and respawns at the scene's spawn point; the game then saves so the penalty sticks.

### Scenes
- `MainMenu → OpeningCave → Town ⇄ Outdoor ⇄ Dungeon`. Each game scene has its own copy of a `Core` prefab (player, HUD, managers, cameras), so there is no `DontDestroyOnLoad`.
- A `SceneDoor` saves the game, then loads the next scene. That scene's `SaveManager` loads the data back, and `SceneArrival` places the player at the matching `SpawnPoint`. Every door is an autosave.

### Tutorial
- The intro is a list of `GuideStep` assets (scene, the cat's path, lines, how the step completes).
- `TutorialTriggers` only listens to game events (item picked up, item equipped, shop opened, enemy killed) and reports them; `TutorialProgress` decides whether the current step is done and saves it.
- The `GuideCat` (Kitty) walks ahead, follows or talks depending on the step.

### Town restoration
- Each building is a `BuildingData` ScriptableObject (name, cost, optional required item).
- `TownProgress` keeps which buildings are restored, saves them through `ISaveable` and announces changes with an event. The town tier (1 + restored buildings) changes the merchant's look and what he sells: each restored building unlocks better wares.
- `RestorableBuilding` only asks `TownProgress` and swaps between its ruined and restored art. `BuildingRebuildView` listens and plays the rebuild animation, so the gameplay script never touches its visuals.
- Restoring the last building fires `OnTownRestored`, and `WinCondition` opens the win screen.

### Interaction and the merchant
- `PlayerInteractor` finds the nearest `IInteractable` in front of the player; a world-space prompt shows `[E] Talk`, `[E] Trade` or `[E] Enter`.
- World-space text (the prompt, speaker names) uses a separate TextMesh Pro material with a dark outline and soft shadow, so it stays readable on bright maps and on Kitty's see-through speech bubble. The shared default font material is never tinted; each text sets its own colour.
- The `Merchant` only asks for the Shop game state and does the trading; he sells from one `ShopStock` per town tier, and the shop UI builds its entries from it and reports clicks and drops back.
- Buying checks coins and inventory space before anything changes, so a failed purchase never loses coins.

### Maps and performance
- Town and Outdoor are painted with Unity's Tilemap and Tile Palette (128 px per cell, the same scale as the characters), with a `Ground` layer, a `Walls` layer with a merged collider and walk-through `Decor`.
- The Dungeon is one painted picture with an edge-collider outline; an overlay and Y-sorted props let the player walk behind walls. Replacing about 1,500 individually sorted tiles with one sprite, plus atlas and texture cleanup, cut batches and draw calls by about 90%.
- All gameplay scenes share one orthographic camera framing from the `Core` prefab.
- No texture imports below its real size. Images far larger than they are ever drawn (AI-generated icons and UI art) were shrunk in place, with pixels-per-unit scaled by the same factor, so every sprite keeps its on-screen size and its references.

### Animation
- Animator Controllers have no transitions. Every state is a 4-direction blend tree.
- The code picks the animation with `Animator.Play` in each state's `Enter`, using cached hashes from an enum.

### Editor tools
- **Tools → Follow The Cat** (`DevTools`): add coins, give every item, full heal, god mode and restore the next building, all through the game's own API, in Play mode only.
- `EnemyInspector` shows an enemy's live state, target and health in Play mode.

## Project structure

```
Assets/
├── Scripts/
│   ├── Player Scripts/      Player hub, input, motor, animator, combat, states, equipment layers, lantern
│   ├── Enemy/               Enemy hub, components, states, spawners, King Wisp boss
│   ├── Combat/              Health, DamageData, IDamageable, IDamageModifier, StrengthBuff
│   ├── Items/               Pickups, IPickable, IUsable
│   ├── Inventory/           Inventory, InventorySlot, Wallet, Equipment
│   ├── Core/                GameStateManager, RespawnManager, scene loading and spawn points
│   ├── Save/                SaveFile, SaveManager, SaveData, SaveMigration, ISaveable
│   ├── Interaction/         PlayerInteractor, NpcDialogue, SceneDoor, IInteractable
│   ├── Merchant/            Merchant, ShopStock, MerchantAppearance
│   ├── Town/                BuildingData, TownProgress, RestorableBuilding, BuildingRebuildView
│   ├── Tutorial/            GuideStep, TutorialProgress, TutorialTriggers, GuideCat
│   ├── Audio/               SoundData, sound controllers, VolumeSettings
│   ├── Lighting/            LightFlicker
│   ├── UI/                  UIManager, inventory, hotbar, shop, menus, health bar
│   ├── Editor/              EnemyInspector
│   └── ScriptableObjects/   Character, enemy, item, weapon and loot data
├── Editor/                  DevTools
├── Audio/                   Music, sound effects, mixer, SoundData assets
├── Prefabs/
├── Scenes/
└── Art/                     Characters, items, UI, maps and tile palettes
```

## Controls

| Action | Key |
| --- | --- |
| Move | WASD |
| Run | Shift (hold) |
| Attack | Left mouse |
| Dash | Space |
| Use or equip hotbar item | 1–7 |
| Interact (talk, trade, enter, restore) | E |
| Inventory | Tab |
| Pause | Escape |
| Inventory: move / split a stack | Left-drag / right-drag |
| Equip / unequip | Drag between the grid and the equipment boxes |
| Shop: buy / sell | Click an item / drag a stack onto the merchant panel |
| Debug save / load | F5 / F9 |

## Getting started

1. Clone the repository (it uses **Git LFS** for large textures and music, so install Git LFS first).
2. Open it with **Unity 6000.3** or newer, using Unity Hub.
3. Open `Assets/Scenes/MainMenu.unity` and press Play. **New Game** starts the cave intro; **Continue** picks up your save.

## Credits

- Code: I designed the architecture of Follow The Cat and wrote its core systems myself: the state machines, the event-driven structure, the data-driven design built on ScriptableObjects and reusable prefabs, combat, health, the inventory and the save system. Once that foundation was in place, I used Claude AI agents (Claude Opus 5.5 and Claude Fable 5.2) to build more systems on top of it, following the rules, logic and architecture I had already set. I worked on the basic monthly plan and kept token usage efficient, using less than 40% of the month’s limit. I read and reviewed every change before merging it to keep the codebase clean, consistent and as bug-free as possible.
- Art and animations: made with Astra, an AI art tool.
