using UnityEngine;

// Every player sound in one place. Gameplay only fires events; this listens and plays the SoundData
// that belongs to them. The sounds live in data assets (CharacterData, WeaponData, EquipmentData,
// ItemData) and play through the mixer groups set on each SoundData, so the Settings sliders control them.
// Only footsteps and coins are set here: the step sound depends on the scene's ground (grass in Core,
// stone in the Dungeon's override) and coins have no data asset.
[RequireComponent(typeof(Player), typeof(Health), typeof(PlayerCombat))]
[RequireComponent(typeof(Equipment), typeof(Inventory), typeof(Rigidbody2D))]
public class PlayerSoundController : SoundPlayer
{
    [Header("Footsteps")]
    [SerializeField] private SoundData stepSound;
    [Tooltip("World units walked between two steps, so running steps faster than walking.")]
    [SerializeField, Min(0.1f)] private float stepDistance = 1.2f;
    [Tooltip("Slower than this counts as standing (collision shoves fading out).")]
    [SerializeField, Min(0f)] private float minStepSpeed = 0.5f;

    [Header("Pickups")]
    [SerializeField] private SoundData coinSound;

    private Player player;
    private Health health;
    private PlayerCombat playerCombat;
    private Equipment equipment;
    private Inventory inventory;
    private Rigidbody2D body;
    private float distanceSinceStep;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        health = GetComponent<Health>();
        playerCombat = GetComponent<PlayerCombat>();
        equipment = GetComponent<Equipment>();
        inventory = GetComponent<Inventory>();
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        health.OnDamaged += PlayHit;
        health.OnDied += PlayDeath;
        playerCombat.OnSwing += PlaySwing;
        player.OnDashStarted += PlayDash;
        equipment.OnItemEquipped += PlayEquip;
        inventory.OnItemUsed += PlayItemUse;
        CoinPickup.OnAnyCoinCollected += PlayCoin;
    }

    private void OnDisable()
    {
        health.OnDamaged -= PlayHit;
        health.OnDied -= PlayDeath;
        playerCombat.OnSwing -= PlaySwing;
        player.OnDashStarted -= PlayDash;
        equipment.OnItemEquipped -= PlayEquip;
        inventory.OnItemUsed -= PlayItemUse;
        CoinPickup.OnAnyCoinCollected -= PlayCoin;
    }

    // Footsteps: one every stepDistance units the Rigidbody actually moves. Reads movement, never input.
    private void Update()
    {
        float speed = body.linearVelocity.magnitude;
        if (stepSound == null || speed < minStepSpeed)
        {
            // The first step sounds as soon as the player starts moving.
            distanceSinceStep = stepDistance;
            return;
        }

        distanceSinceStep += speed * Time.deltaTime;
        if (distanceSinceStep < stepDistance) return;

        distanceSinceStep = 0f;
        Play(stepSound);
    }

    private void PlayHit(DamageData damage) => Play(player.playerData.hitSound);
    private void PlayDeath() => Play(player.playerData.deathSound);
    private void PlayDash() => Play(player.playerData.dashSound);
    private void PlaySwing(WeaponData weapon) => Play(weapon != null ? weapon.swingSound : null);
    private void PlayEquip(EquipmentData item) => Play(item.equipSound);
    private void PlayItemUse(ItemData item) => Play(item.useSound);

    // The event is static (the coin destroys itself), so only react to this player's pickups.
    private void PlayCoin(GameObject collector, int amount)
    {
        if (collector == gameObject) Play(coinSound);
    }
}
