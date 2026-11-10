// Base class for every player state. Each state gets the Player so it can reach its components.
public abstract class PlayerState
{
    protected readonly Player player;

    protected PlayerState(Player player)
    {
        this.player = player;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public virtual void FixedUpdate() { }

    // Hotbar keys 1-7 select a slot and use its item (a potion is drunk; non-usable items do nothing).
    // Called only by the states that allow it (Idle, Walk, Run).
    protected void HandleHotbarInput()
    {
        int slot = player.playerInputReader.hotbarSlotPressedThisFrame;
        if (slot == -1) return;

        player.playerInventory.SelectSlot(slot);
        player.playerInventory.UseItem(slot, player.gameObject);
    }
}
