using UnityEngine;

// Attacking; movement is locked until the attack finishes.
// -> Idle or Walk when the attack ends, -> Hurt when hit
public class PlayerAttackState : PlayerState
{
    public PlayerAttackState(Player player) : base(player) { }

    public override void Enter()
    {
        // stop movement, play attack animation, enable hitbox
    }

    public override void Update()
    {
        // wait for the attack to finish
    }

    public override void Exit()
    {
        // disable hitbox
    }
}
