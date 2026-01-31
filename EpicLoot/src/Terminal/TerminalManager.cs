using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace EpicLoot;

public static class TerminalManager
{
    internal const string START_COMMAND = "epicloot";
    internal static readonly Dictionary<string, Command> commands = new();

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    private static class Terminal_InitTerminal
    {
        private static void Postfix()
        {
            _ = new Terminal.ConsoleCommand(START_COMMAND, "use help to find available commands", args =>
            {
                if (args.Length < 2) return false;
                if (!commands.TryGetValue(args[1], out Command data))
                {
                    return false;
                }
                return data.Run(args);
            },  optionsFetcher: commands
                .Where(x => !x.Value.IsSecret())
                .Select(x => x.Key)
                .ToList);

            _ = new Command("help", "list of available commands", args =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, Command> command in commands.OrderBy(c => c.Key))
                {
                    if (command.Key == "help")
                    {
                        continue;
                    }
                    if (command.Value.IsSecret())
                    {
                        continue;
                    }

                    sb.Clear();
                    sb.AppendFormat("<color=yellow>{0}</color> - {1}", command.Key, command.Value.description);
                    if (command.Value.adminOnly)
                    {
                        sb.Append(" <color=red>(admin only)</color>");
                    }
                    args.Context.AddString(sb.ToString());
                }
            });
            
            MagicCommands.Init();
        }
    }
    
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.updateSearch))]
    private static class Terminal_updateSearch
    {
        private static bool Prefix(Terminal __instance, string word)
        {
            if (__instance.m_search == null)
            {
                return true;
            }
            string[] strArray = __instance.m_input.text.Split(' ');
            if (strArray.Length < 3)
            {
                return true;
            }

            if (strArray[0] != START_COMMAND)
            {
                return true;
            }
            return HandleSearch(__instance, word, strArray);
        }
    }
    
    private static bool HandleSearch(Terminal __instance, string word, string[] strArray)   
    {
        if (!commands.TryGetValue(strArray[1], out Command command))
        {
            return true;
        }
        if (command.HasOptions() && strArray.Length > 2)
        {
            List<string> list = command.GetTabOptions(strArray.Length - 1);
            List<string> filteredList;
            string currentSearch = strArray[strArray.Length - 1];
            
            if (!string.IsNullOrEmpty(currentSearch))
            {
                int indexOf = list.IndexOf(currentSearch);
                filteredList = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                filteredList = filteredList.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
            }
            else
            {
                filteredList = list;
            }
            
            if (filteredList.Count <= 0) __instance.m_search.text = command.description;
            else
            {
                __instance.m_lastSearch.Clear();
                __instance.m_lastSearch.AddRange(filteredList);
                __instance.m_lastSearch.Remove(word);
                __instance.m_search.text = "";
                int maxShown = 10;
                int count = Math.Min(__instance.m_lastSearch.Count, maxShown);
                for (int index = 0; index < count; ++index)
                {
                    string text = __instance.m_lastSearch[index];
                    __instance.m_search.text += text + " ";
                }

                if (__instance.m_lastSearch.Count <= maxShown)
                {
                    return false;
                }
                int remainder = __instance.m_lastSearch.Count - maxShown;
                __instance.m_search.text += $"... {remainder} more.";
            }
        }
        else
        {
            __instance.m_search.text = command.description;
        }
                
        return false;
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.tabCycle))]
    private static class Terminal_tabCycle
    {
        private static void Prefix(Terminal __instance, ref List<string> options)
        {
            if (string.IsNullOrEmpty(__instance.m_input.text))
            {
                return;
            }
            string[] strArray = __instance.m_input.text.Split(' ');
            if (strArray.Length < 2)
            {
                return;
            }
            string startCommand = strArray[0];
            if (!string.Equals(startCommand, START_COMMAND, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            string magicCommand = strArray[1];
            
            if (commands.TryGetValue(magicCommand, out Command command))
            {
                options = command.GetTabOptions(strArray.Length - 1);
            }
        }
    }
    
    public static string GetString(this Terminal.ConsoleEventArgs args, int index, string defaultValue = "")
    {
        if (args.Length < index + 1)
        {
            return defaultValue;
        }
        return args[index];
    }

    public static float GetFloat(this Terminal.ConsoleEventArgs args, int index, float defaultValue = 0f)
    {
        if (args.Length < index + 1)
        {
            return defaultValue;
        }
        string arg = args[index];
        return float.TryParse(arg, out float result) ? result : defaultValue;
    }

    public static int GetInt(this Terminal.ConsoleEventArgs args, int index, int defaultValue = 0)
    {
        if (args.Length < index + 1)
        {
            return defaultValue;
        }
        string arg = args[index];
        return int.TryParse(arg, out int result) ? result : defaultValue;
    }
}