


public  abstract class PlayerState

{
    protected readonly Player player;
    protected PlayerState(Player player) {this.player = player ; } 
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update(); 
    public virtual void FixedUpdate() {}



}
