using System;
using System.Collections.Generic;

namespace EpicLoot;

public class Command
{
    public readonly string description;
    private readonly bool isSecret;
    public readonly bool adminOnly;
    private readonly Action<Terminal.ConsoleEventArgs> command;
    private readonly Func<int, List<string>> tabOptions;

    public bool Run(Terminal.ConsoleEventArgs args)
    {
        if (!IsAdmin())
        {
            return true;
        }
        command(args);
        return true;
    }
    private bool IsAdmin()
    {
        if (!ZNet.m_instance)
        {
            return true;
        }
        if (!adminOnly || ZNet.m_instance.LocalPlayerIsAdminOrHost())
        {
            return true;
        }
        Console.instance.AddString("<color=red>Admin Only</color>");
        return false;
    }
    public bool IsSecret() => isSecret;
    public List<string> GetTabOptions(int indexOfLastWord) => tabOptions == null ? 
        [] : 
        tabOptions(indexOfLastWord);
    public bool HasOptions() => tabOptions != null;
        
    public Command(string input, string description, Action<Terminal.ConsoleEventArgs> command, Func<int, List<string>> optionsFetcher = null, bool adminOnly = false, bool isSecret = false)
    {
        this.description = description;
        this.command = command;
        this.isSecret = isSecret;
        this.adminOnly = adminOnly;
        tabOptions = optionsFetcher;
        TerminalManager.commands[input] = this;
    }
}