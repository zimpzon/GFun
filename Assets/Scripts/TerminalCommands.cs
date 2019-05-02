using CommandTerminal;
using UnityEngine;

public static class TerminalCommands
{
    public static void Log(CommandArg[] args)
    {
        DebugLinesScript.Show("arg", args[0]);
    }

    public static void RegisterCommands()
    {
        if (Terminal.Shell == null)
            return;

        Terminal.Shell.AddCommand("log", Log, 1, 1, "Show text");
    }
}
