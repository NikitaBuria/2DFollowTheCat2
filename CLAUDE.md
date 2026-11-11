# 2DFollowTheCat2

Top-down 2D game in Unity 6 (6000.3), URP, new Input System. The Editor can be driven with the Unity CLI (`unity status`, `unity command ...`).

## How to work with me (the developer)

I'm learning Unity and C# by building this game, and I write all the gameplay code myself.
- Explain concepts, syntax and architecture; review my code when I ask ("check my code").
- Point out bugs with the reason and the fix, but don't edit my gameplay scripts unless I explicitly ask.
- Tooling is fine when asked: git, importing/slicing sprites, animations, project settings.

## Player architecture

One job per script:
- **Player**: wires everything, creates the states, forwards `Update`/`FixedUpdate` to the current state, owns `facingDirection` (changed only through `SetFacingDirection`). Turns `Health.OnDamaged` → Hurt (only while alive, stores `lastDamage`) and `Health.OnDied` → Dead. No game logic. Its data field is `playerData` (a `CharacterData` asset).
- **PlayerInputReader**: the only script that reads input. All input is polled by the current state in `Update` (`ReadValue`, `IsPressed`, `WasPressedThisFrame`); no input events. Each state checks only the inputs it accepts.
- **PlayerStateMachine**: plain class with `Initialize`, `ChangeState` (old `Exit` → swap → new `Enter`) and `Shutdown`; fires `OnStateChanged`. Only it switches states.
- **PlayerState**: abstract base. `Enter`/`Update`/`Exit` abstract, `FixedUpdate` virtual and empty.
- **States** (`Scripts/Player Scripts/PlayerStates/`, one file each): Idle, Walk, Run, Dash, Attack, Hurt, Dead; later Interact and Ability. Dash is still empty. Hurt: starts i-frames (`invincibleTimer`) in `Enter`, pushes along `lastDamage.direction` for its `knockbackDuration`, then Idle; `Stop()` in `Exit`. Idle and Dead call `Stop()` every `FixedUpdate`. Dead: plays Dead, turns off the collider in `Enter` and back on in `Exit`; no transitions of its own (RespawnManager calls `Player.Respawn`, which goes to Idle).
- **PlayerMotor**: Rigidbody only. No `Update`/`FixedUpdate`, no input, speeds passed in as parameters.
- **PlayerAnimator**: `PlayerAnimationState` enum (own file) + switch expression to cached `Animator.StringToHash`, throws on unknown values; `SetMoveDirection` drives the MoveX/MoveY blend trees.
- **PlayerCombat**: one public method, `HandleAttack(direction)`: `OverlapCircleAll` at `attackPoint` with the weapon's range, keeps targets inside the weapon's arc, a `HashSet` so each target is hit once per swing. Stats come from the equipped `WeaponData`; `PlayerAttackState` decides when.
- **Health**: the shared component (see Shared), as `player.playerHealth`. The Rigidbody has Linear Damping 8 so collision shoves fade out.
- **Wallet** and **Inventory**: components on the Player GameObject (see Loot and items).
- **PlayerInteractor**: see Interaction. Later: PlayerAbilities, Equipment.

## Enemy architecture

Same pattern as the player (`Scripts/Enemy/`):
- **Enemy**: the hub. Creates the states, forwards `Update`/`FixedUpdate`, calls `enemyDetection.Refresh()`, turns `Health.OnDamaged` → Hurt and `Health.OnDied` → Dead. Holds `facingDirection`, `lastDamage` and the patrol route (positions captured in `Awake`).
- **EnemyStateMachine** / **EnemyState** (`Scripts/Enemy/EnemyStates/`): copies of the player versions.
- **States**: Idle ⇄ Patrol → Chase → Windup → Attack (hit, then `recoverTime`) → Chase; any → Hurt (knockback, not restarted by new hits, so no stun-lock); killing hit → Dead (colliders off, `enemyData.lootTable.DropLoot` once, `Destroy` after `destroyDelay`). Windup locks facing so the player can dodge.
- **Components**: `EnemyMotor` (`MoveTowards` without overshooting, `Move`, `Stop`), `EnemyDetection` (throttled `OverlapCircle` search, immediate loss beyond `loseTargetRange`, `Linecast` line of sight only for spotting), `EnemyCombat` (body attack: range check to the target collider's `ClosestPoint`, re-checked at the moment of the hit), `EnemyAnimator` (same as `PlayerAnimator`, warns once about missing Animator states).
- Enemies attack with their body, never with `WeaponData`; their attack stats live in `EnemyData`.
- **Editor/EnemyInspector**: shows the live state, target and health in Play mode.

## Shared

- **Health** (`Scripts/Combat/`): implements `IDamageable`; `Initialize(maxHealth)` from the owner's data; events `OnDamaged(DamageData)` (before `OnDied`, also on the killing hit) and `OnDied`. I-frames: `StartInvincibility(duration)` stores a `Time.time` timestamp; `TakeDamage` ignores hits while `isInvincible`. States decide when (player Hurt now, Dash later).
- **DamageData**: readonly struct (amount, normalized direction, knockback, knockbackDuration).
- **Data (ScriptableObjects)**: `CharacterData` (maxHealth, walk/run/dash speed, `invincibleTimer`; enemies leave it at 0 for now) → `EnemyData` (detection, patrol, body attack, windup/recover/hurt timing, `lootTable`). `ItemData` (itemName, icon, maxStack, sellPrice, buyPrice). `ShopStock` (merchant's items). `LootTable` (list of `LootDrop`: prefab, optional item, chance, min/max; spawns in a circle). `WeaponData` (damage, knockback, range, arc, attackDuration); later it inherits from `ItemData`.

## Loot and items

Ported from MetrooDvania, adapted to top-down:
- **IPickable** (`Scripts/Items/`): `bool TryPickUp(GameObject collector)`; true = everything taken.
- **Pickup**: abstract base with a `CircleCollider2D` trigger (no gravity). `OnTriggerEnter2D` passes the collider's Rigidbody object as collector and destroys itself when `TryPickUp` returns true. `CoinPickup` → collector's `Wallet`; `ItemPickup` → collector's `Inventory` (stays if it doesn't fit; `Configure(item, amount)` sets item and sprite). Enemies have neither, so they can't collect.
- **Wallet** (`Scripts/Inventory/`): `coins`, `Add`, `TrySpend`, `LoseShare(share)` (death penalty), event `OnMoneyChanged(int)`. Coins are not inventory items.
- **Inventory**: `slotCount` × `InventorySlot` (item + amount, stacks up to `maxStack`); `AddItemAndReturnAccepted`, `HasItem`, `ConsumeItem`, `MoveOrMerge(from, to)` (move to empty, merge same item up to `maxStack`, swap different items), event `OnChanged`. `Split(from, to, amount)` (right-drag: half the stack, rounded up, into an empty slot or onto the same item; refused if it does not fit, never swaps).
- Assets: `Prefabs/Loot/GoldCoinPickup` (animated spin) and `WispEssencePickup`, art in `Art/Items/Loot/GeneratedPickups/`; `ScriptableObjects/Items/WispEssence`, `ScriptableObjects/Loot/WispGrinLoot` (2–4 coins, 50% Wisp Essence).

## Game state and UI

- **GameState** (`Scripts/Core/`): enum `Gameplay`, `Inventory`, `Paused`, `Shop` (Gameplay first = default).
- **GameStateManager**: single source of truth for the mode. Polls `inventoryPressedThisFrame` / `menuPressedThisFrame` in `Update` with a `switch (currentState)` (Tab toggles Inventory; Escape toggles Paused and closes Inventory; Tab or Escape closes Shop, which only `Merchant.Interact` opens). `Awake` resets `Time.timeScale` to 1; `ChangeState` sets it (1 in Gameplay, 0 otherwise), turns the Player action map on only in Gameplay, then fires `OnGameStateChanged`.
- **PlayerInputReader** maps: Player Actions (on only in Gameplay) and UI Actions (always on: Toggle Inventory, Toggle Pause). Maps are switched with `action.actionMap.Enable()/Disable()`.
- **UIManager** (`Scripts/UI/`, on the always-active HUD Canvas): listens to `OnGameStateChanged` and only shows/hides panels with `SetActive` (InventoryPanel in Inventory and Shop, MerchantPanel in Shop, pause menu in Paused). Content scripts never know the game state.
- **CoinCounterUI** (inside InventoryPanel): listens to `Wallet.OnMoneyChanged`, refreshes in `OnEnable`.
- **InventoryUI** (on InventoryPanel): builds `inventory.count` `InventorySlotUI` prefabs into `InventoryArea` (Grid Layout Group) in `Start` (after every `Awake`), redraws all slots on `Inventory.OnChanged` and in `OnEnable`; `Refresh` loops over the built views. Owns drag and drop: `BeginDrag`/`UpdateDrag`/`EndDrag` move the `DragPreview` (last child of InventoryPanel, Raycast Target off) and call `Inventory.MoveOrMerge`; closing the panel cancels a drag.
- **InventorySlotUI** (on the `InventorySlot` prefab): `Show(InventorySlot)` sets every field each time (icon hidden when empty, amount text only above 1). `Setup(owner, index)` makes a grid view draggable via the EventSystem drag interfaces (UI pointer events are the accepted exception to "no input events"); it only reports to its owner. Views without an owner ignore drags.
- **HotbarUI** (always visible): `HotbarSize` (const 7) views of Inventory slots 0–6, built in `Start`, redrawn on `OnChanged`. A view only (no drag target): items get there by being in grid slots 0–6. Keys 1–7 (one `Player HotbarKeys` action, 7 bindings in order; `PlayerInputReader.hotbarSlotPressedThisFrame` returns the binding index via `GetBindingIndexForControl`, -1 if none) are polled by `PlayerState.HandleHotbarInput()` in Idle/Walk/Run → `Inventory.SelectSlot` + `Inventory.UseItem(slot, player)` (the number key acts right away: `IUsable` items like `HealthPotionData` are used and one is removed; a potion at full health is kept) → `OnSelectedSlotChanged(int)` → `HotbarUI` toggles the `Highlight` glow on the `InventorySlot` prefab (warm translucent copy of the slot sprite above the background, off by default). Later: weapons in the hotbar equip from the same call.
- **Equipment slots**: Helmet, Armor, Weapon, Boots boxes in `EquipmentArea` (variants of `Prefabs/UI/Equipment/EquipmentSlot`); visuals only until Equipment (step 9).
- Layout: Diablo 2-style panel (paper doll on top, 1-item-per-cell grid, coins bottom-right) + always-visible 1–7 hotbar = Inventory slots 0–6.

## Save and respawn

Ported from MetrooDvania's Lesson 52 checkpoint save, split into one job per script (`Scripts/Save/`, `Scripts/Core/RespawnManager`):
- **SaveData** / **SlotSaveData**: plain `[Serializable]` data (`version`, `health`, `coins`, `inventory`). Bump `SaveData.CurrentVersion` when the layout changes.
- **SaveFile** (static): the only code that touches the file. JSON in `Application.persistentDataPath/save.json`; writes `.tmp` then `File.Replace` keeping a `.bak`; refuses files over 64 KB.
- **ISaveable**: `Save(data)`, `Validate(data)` (null = OK, else the reason), `Load(data)`. Implemented by `Health` (1..maxHealth), `Wallet` and `Inventory` (items saved by ID = asset name, turned back into assets by the `ItemDatabase` asset; every saveable `ItemData` must be in `ScriptableObjects/Items/ItemDatabase`).
- **SaveManager** (on `SaveSystem`): collects the player's `ISaveable`s in `Awake`, loads in `Start` (`loadOnStart`), F5/F9 debug keys via `PlayerInputReader` (UI map: `Save Game`/`Load Game`). Validates every saveable before loading any, tries `.bak` if the main file is bad; never saves or loads a dead player. Context menu: Delete Save File.
- **RespawnManager** (on `SaveSystem`): on `Health.OnDied` waits `respawnDelay` (scaled time, pauses with the game), `Wallet.LoseShare(coinLossOnDeath)` (0.25), `Player.Respawn(spawnPoint)` (`PlayerMotor.Teleport`, full health, i-frames, Idle), then saves so the penalty sticks.
- Position isn't saved: loading keeps the player where they are; respawn uses `PlayerSpawnPoint`.

## Interaction

Adapted from MetrooDvania's `Scripts/Interaction` (`Scripts/Interaction/`):
- **IInteractable**: `CanInteract(player)` (false = refused, prompt still shows why), `GetPrompt(player)`, `Interact(player)`. Gets the `Player` so each one takes what it needs (Wallet, Inventory...).
- **PlayerInteractor** (on the Player, `player.playerInteractor`): `Refresh()` is called by `Player.Update` (like `EnemyDetection`); `OverlapCircle` at `facingDirection * searchOffset` on the `Interactable` layer (layer 8), nearest wins, none while dead. Exposes `currentTarget`, `promptText`, `canInteract`, `targetPosition`. `TryInteract()` is called by `PlayerState.HandleInteractInput()` from Idle, Walk and Run (E, `Interact` in the Player map).
- **InteractionPromptView** (scene object `InteractionPrompt`, world-space TMP): reads `PlayerInteractor` in `LateUpdate` and draws `[E] Talk`; refused prompts are red without the key. Never called by gameplay code.
- **NpcDialogue** (IInteractable): `speakerName`, `lines`, `lineDuration`; each interact shows the next line, after the last one (or the timer) it ends. Events `OnLineShown(speaker, line)`, `OnDialogueEnded`.
- **SpeechBubbleView** (on `CatSpeechBubble`/`MerchantSpeechBubble`): listens to its `NpcDialogue`, hides with the `CanvasGroup` (not `SetActive`, so it keeps listening).
- Scene: `GuideCat` (Interactable layer, trigger `CircleCollider2D`, `NpcDialogue`, `CatSpeechBubble` child). The cat's Talk animation isn't played yet.
- **Merchant** (`Scripts/Merchant/`, the object's only IInteractable): `Interact` stores the `customer` (Player), says the next `greeting` line (`NpcDialogue` on the `Greeting` child, no collider, so `GetComponentInParent<IInteractable>` can't pick it) and calls `ChangeState(GameState.Shop)`; never touches UI. Unlimited coins and stock. `TrySell(slotIndex)` sells the whole stack (`sellPrice` × amount, 0 = unsellable) via `Inventory.DeleteSlot` + `Wallet.Add`. `TryBuy(item)` buys one: item must be in `shopStock`, coins checked first, then `AddItemAndReturnAccepted` (0 = full, nothing changes), then `TrySpend`.
- **ShopStock** (SO, `ScriptableObjects/Merchant/MerchantStock`): list of `ItemData`, `HasItem`. Dummies until their systems exist: `StrengthElixir` (buff), `LeatherArmor`; the Health Potion works.
- **Shop UI**: `MerchantStorePanel` (Astra's prefab, HUD child, `UIManager.merchantPanel`) has `MerchantStoreUI` (builds one `ShopEntry` prefab per stock item in `Start`, `HorizontalLayoutGroup`) and `MerchantSellArea` (`IDropHandler`: a left-drag of an `InventorySlotUI` dropped anywhere on the panel → `TrySell`; right-drag ignored). `ShopEntryUI`: icon + `buyPrice`, `Button` click → owner → `TryBuy`. Later: feedback when a buy fails.
- **MerchantAppearance** (on the Merchant prefab, art by Astra): one child Animator per town tier, `SetTownTier(1..3)` for town restoration; `Play`s Talk/Idle on the greeting's events (controllers have no transitions).

## Pause menu visual prefab

- `Assets/Prefabs/UI/PauseMenu.prefab`: charcoal/brass/teal panel with editable TMP Resume, Settings and Exit buttons, dimmer and explicit vertical navigation. Art-only: callbacks and initial EventSystem selection are not wired. Assign its scene instance to the existing `UIManager.pauseMenuPanel`. Setup notes: `Assets/Prefabs/UI/PauseMenu.md`.

## Animation rules

- Each Animator Controller has no transitions; every state is a 4-direction blend tree on MoveX/MoveY, and the code picks the state with `Play` in the state's `Enter`.
- Animator state names must equal the enum value names (`Idle`, `Walk`, `Run`, `Windup`, `Attack`, `Hurt`, `Dead`, ...).
- One-shot clips (Attack, Windup, Hurt, Death) don't loop; looping clips are Idle, Walk, Run, Move.
- Clip lengths match their data timing (e.g. Windup = `windupTime`, player Attack = `attackDuration`, `destroyDelay` ≥ Death).

Rules:
- States decide **what/when**; components do **how**.
- Input checks, timers and transitions in `Update`; motor calls in `FixedUpdate`.
- Input → polled by states; game-state changes → events.
- Observer events for changes (`OnDamaged`, `OnDied`, later `OnMoneyChanged`, `OnWeaponChanged`, inventory `OnChanged`); UI only listens.
- Interfaces: `IDamageable`, `IPickable`, `IUsable`, `ISaveable`, `IInteractable`.
- ScriptableObjects for data: `CharacterData` → `EnemyData`; `ItemData` → `WeaponData`/`ArmorData` (slot enums); abstract `Ability` with cooldown.
- States referencing the concrete `Player`/`Enemy` is accepted coupling.

## Code review checklist

- Every `+=` has a matching `-=` (in `OnEnable`/`OnDisable`).
- No magic numbers in states; tunable values in the data ScriptableObjects or the Inspector.
- Private fields with `[SerializeField]`; read-only access via `{ get; private set; }`.
- `[RequireComponent]` where a component is needed.
- Consistent naming, correct spelling (scripts and asset names), class name matches file name.
- Naming (developer's choice, like Unity's own API): fields and properties camelCase, public ones too (`player.playerMotor`, `currentWeapon`, `isAlive`); types, methods, events (`OnDied`), enum values and constants (`const`, e.g. `HotbarSize`; never SCREAMING_CASE) PascalCase; `I` prefix only for interfaces.
- Same-GameObject components: `GetComponent` in `Awake` + `[RequireComponent]`, not serialized; serialize only references to other objects/assets.
- Delete dead code.
- Finish and test one system before starting the next; commit after each working step.
- When a system is finished, update this file in the same step.

## Build order

1. Movement (Idle/Walk/Run) ✅ → 2. Combat (player attack ✅; fix review items) → 3. Health/Hurt/Dead ✅ → 4. Enemy with its own state machine (built early with the Wisp; needs a Play-mode test) → 5. Loot, Inventory, Wallet (core ported; coin counter ✅, game state + inventory panel toggle ✅, inventory grid ✅, equipment slot layout ✅, hotbar + health potion ✅) → 6. Save system (save/load + death respawn ✅; later: autosave in town, checkpoints, restoration progress) → 7. Interactor (base + NPC dialogue ✅, merchant shop: buy + sell ✅; next: door/stairs, town restoration) → 8. Boss + win screen → 9. Dash, Equipment, Abilities → 10. Audio, UI polish

## Team

Astra makes the art (sprites, animations, prefab visuals). The developer and Claude design and write all code.
