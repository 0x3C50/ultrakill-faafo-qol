using GameConsole;

namespace ExtraCommands.commands;

public class CheatsCommand : ICommand
{
    public void Execute(Console con, string[] args)
    {
        if (CheatsController.Instance.cheatsEnabled)
        {
            con.PrintLine("Cheats are already enabled you goofball");
            return;
        }

        CheatsController.Instance.ActivateCheats();
    }

    public string Name => "sv_cheats";
    public string Description => "Turns cheats on the fast way";
    public string Command => "sv_cheats";
}