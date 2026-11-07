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
}
