using UnityEngine;

// Got hit: short knockback, no control.
// -> Idle when knockback ends, -> Dead when health reaches 0
public class PlayerHurtState : PlayerState
{
    public PlayerHurtState(Player player) : base(player) { }

    public override void Enter()
    {
        // apply knockback, play hurt animation, start invincibility
    }

    public override void Update()
    {
        // count down the knockback time, leave the state when it ends
    }

    public override void Exit()
    {
    }
}
