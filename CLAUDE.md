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
- **States** (`Scripts/Player Scripts/PlayerStates/`, one file each): Idle, Walk, Run, Dash, Attack, Hurt, Dead; later Interact and Ability. Dash is still empty. Hurt: starts i-frames (`invincibleTimer`) in `Enter`, pushes along `lastDamage.direction` for its `knockbackDuration`, then Idle; `Stop()` in `Exit`. Idle and Dead call `Stop()` every `FixedUpdate`. Dead: plays Dead, turns off the collider (turn it back on when respawn is added), no transitions out.
- **PlayerMotor**: Rigidbody only. No `Update`/`FixedUpdate`, no input, speeds passed in as parameters.
- **PlayerAnimator**: `PlayerAnimationState` enum (own file) + switch expression to cached `Animator.StringToHash`, throws on unknown values; `SetMoveDirection` drives the MoveX/MoveY blend trees.
- **PlayerCombat**: one public method, `HandleAttack(direction)`: `OverlapCircleAll` at `attackPoint` with the weapon's range, keeps targets inside the weapon's arc, a `HashSet` so each target is hit once per swing. Stats come from the equipped `WeaponData`; `PlayerAttackState` decides when.
- **Health**: the shared component (see Shared), as `player.playerHealth`. The Rigidbody has Linear Damping 8 so collision shoves fade out.
- **Wallet** and **Inventory**: components on the Player GameObject (see Loot and items).
- Later: PlayerAbilities, PlayerInteractor, Equipment.

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
- **Data (ScriptableObjects)**: `CharacterData` (maxHealth, walk/run/dash speed, `invincibleTimer`; enemies leave it at 0 for now) → `EnemyData` (detection, patrol, body attack, windup/recover/hurt timing, `lootTable`). `ItemData` (itemName, icon, maxStack, sellPrice). `LootTable` (list of `LootDrop`: prefab, optional item, chance, min/max; spawns in a circle). `WeaponData` (damage, knockback, range, arc, attackDuration); later it inherits from `ItemData`.

## Loot and items

Ported from MetrooDvania, adapted to top-down:
- **IPickable** (`Scripts/Items/`): `bool TryPickUp(GameObject collector)`; true = everything taken.
- **Pickup**: abstract base with a `CircleCollider2D` trigger (no gravity). `OnTriggerEnter2D` passes the collider's Rigidbody object as collector and destroys itself when `TryPickUp` returns true. `CoinPickup` → collector's `Wallet`; `ItemPickup` → collector's `Inventory` (stays if it doesn't fit; `Configure(item, amount)` sets item and sprite). Enemies have neither, so they can't collect.
- **Wallet** (`Scripts/Inventory/`): `coins`, `Add`, `TrySpend`, event `OnMoneyChanged(int)`. Coins are not inventory items. Later: lose 25% on death.
- **Inventory**: `slotCount` × `InventorySlot` (item + amount, stacks up to `maxStack`); `AddItemAndReturnAccepted`, `HasItem`, `ConsumeItem`, event `OnChanged`. Moving/splitting stacks comes with the inventory UI.
- Assets: `Prefabs/Loot/GoldCoinPickup` (animated spin) and `WispEssencePickup`, art in `Art/Items/Loot/GeneratedPickups/`; `ScriptableObjects/Items/WispEssence`, `ScriptableObjects/Loot/WispGrinLoot` (2–4 coins, 50% Wisp Essence).

## Game state and UI

- **GameState** (`Scripts/Core/`): enum `Gameplay`, `Inventory`, `Paused` (Gameplay first = default).
- **GameStateManager**: single source of truth for the mode. Polls `inventoryPressedThisFrame` / `menuPressedThisFrame` in `Update` with a `switch (currentState)` (Tab toggles Inventory; Escape toggles Paused and closes Inventory). `ChangeState` sets `Time.timeScale` (1 in Gameplay, 0 otherwise), turns the Player action map on only in Gameplay, then fires `OnGameStateChanged`.
- **PlayerInputReader** maps: Player Actions (on only in Gameplay) and UI Actions (always on: Toggle Inventory, Toggle Pause). Maps are switched with `action.actionMap.Enable()/Disable()`.
- **UIManager** (`Scripts/UI/`, on the always-active HUD Canvas): listens to `OnGameStateChanged` and only shows/hides panels with `SetActive`. Content scripts never know the game state.
- **CoinCounterUI** (inside InventoryPanel): listens to `Wallet.OnMoneyChanged`, refreshes in `OnEnable`.
- **InventoryUI**: empty shell; next it draws the grid from `Inventory.OnChanged`.
- Layout: Diablo 2-style panel (paper doll on top, 1-item-per-cell grid, coins bottom-right) + always-visible 1–5 hotbar = Inventory slots 0–4.

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
- Interfaces: `IDamageable`, `IPickable`; later `IInteractable`.
- ScriptableObjects for data: `CharacterData` → `EnemyData`; `ItemData` → `WeaponData`/`ArmorData` (slot enums); abstract `Ability` with cooldown.
- States referencing the concrete `Player`/`Enemy` is accepted coupling.

## Code review checklist

- Every `+=` has a matching `-=` (in `OnEnable`/`OnDisable`).
- No magic numbers in states; tunable values in the data ScriptableObjects or the Inspector.
- Private fields with `[SerializeField]`; read-only access via `{ get; private set; }`.
- `[RequireComponent]` where a component is needed.
- Consistent naming, correct spelling (scripts and asset names), class name matches file name.
- Naming (developer's choice, like Unity's own API): fields and properties camelCase, public ones too (`player.playerMotor`, `currentWeapon`, `isAlive`); types, methods, events (`OnDied`) and enum values PascalCase; `I` prefix only for interfaces.
- Same-GameObject components: `GetComponent` in `Awake` + `[RequireComponent]`, not serialized; serialize only references to other objects/assets.
- Delete dead code.
- Finish and test one system before starting the next; commit after each working step.
- When a system is finished, update this file in the same step.

## Build order

1. Movement (Idle/Walk/Run) ✅ → 2. Combat (player attack ✅; fix review items) → 3. Health/Hurt/Dead ✅ → 4. Enemy with its own state machine (built early with the Wisp; needs a Play-mode test) → 5. Loot, Inventory, Wallet (core ported; coin counter ✅, game state + inventory panel toggle ✅; next: inventory grid, then hotbar) → 6. Save system → 7. Interactor (doors, merchant, town restoration) → 8. Boss + win screen → 9. Dash, Equipment, Abilities → 10. Audio, UI polish
