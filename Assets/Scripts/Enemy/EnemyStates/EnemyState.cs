// Base class for every enemy state. Each state gets the Enemy so it can reach its components.
public abstract class EnemyState
{
    protected readonly Enemy enemy;

    protected EnemyState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public virtual void FixedUpdate() { }
}
