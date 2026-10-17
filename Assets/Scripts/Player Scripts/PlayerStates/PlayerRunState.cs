public class PlayerRunState : PlayerState

{
    public PlayerRunState(Player player) : base(player) { }

    public override void Enter()
    {
        // play run animation
    }

    public override void Update()
    {
        // move at sprint speed, check input and switch state
    }

    public override void Exit()
    {
    }
}
