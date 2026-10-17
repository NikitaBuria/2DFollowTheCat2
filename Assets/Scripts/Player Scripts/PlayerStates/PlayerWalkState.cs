
// Moving at walk speed.
// -> Idle when no input, -> Run while sprint held, -> Dash, -> Attack, -> Hurt
public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(Player player) : base(player) { }

    public override void Enter()
    {
        // play walk animation
    }

    public override void Update()
    {
        
    }
    public override void FixedUpdate()
    {

        


    }


    public override void Exit()
    {
    }
}
