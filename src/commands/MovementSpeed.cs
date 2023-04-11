using GameConsole;

namespace ExtraCommands.commands;

public class MovementSpeed : CommandRoot
{
    public override string Name => "movementSpeed";
    public override string Description => "Changes your movement speed";

    protected override void BuildTree(Console con)
    {
        Branch("walkSpeed", () =>
        {
            Leaf("get", delegate { con.PrintLine($"Current walk speed: {NewMovement.Instance.walkSpeed}"); });
            Leaf("set", delegate(float newSpeed)
            {
                NewMovement.Instance.walkSpeed = newSpeed;
                con.PrintLine($"Set walk speed to {newSpeed}");
            });
        });
        Branch("jumpPower", () =>
        {
            Leaf("get", delegate { con.PrintLine($"Current jump power: {NewMovement.Instance.jumpPower}"); });
            Leaf("set", delegate(float newPower)
            {
                NewMovement.Instance.jumpPower = newPower;
                con.PrintLine($"Set jump power to {newPower}");
            });
        });
        Branch("airAcceleration", () =>
        {
            Leaf("get",
                delegate { con.PrintLine($"Current air acceleration: {NewMovement.Instance.airAcceleration}"); });
            Leaf("set", delegate(float newAccel)
            {
                NewMovement.Instance.airAcceleration = newAccel;
                con.PrintLine($"Set air acceleration to {newAccel}");
            });
        });
        Branch("wallJumpPower", () =>
        {
            Leaf("get", delegate { con.PrintLine($"Current wall jump power: {NewMovement.Instance.wallJumpPower}"); });
            Leaf("set", delegate(float newPower)
            {
                NewMovement.Instance.wallJumpPower = newPower;
                con.PrintLine($"Set wall jump power {newPower}");
            });
        });
    }
}