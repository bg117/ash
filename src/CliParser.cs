using System.Text.RegularExpressions;
using System.Diagnostics;

namespace OpenProject.ASH;

internal static class CliParser
{
    internal static volatile int LastExitCode;
    internal static volatile string ExitErrorMessage = string.Empty;

    /// <summary>
    /// Executes the command specified.
    /// </summary>
    /// <param name="command">The command to be executed. May be semicolon-separated.</param>
    /// <param name="vars">The key-value pairs that correspond to variables. The contents may change depending on the command executed.</param>
    /// <returns>The exit code of the command.</returns>
    internal static bool ExecuteCommand(string command, ref Dictionary<string, string> vars)
    {
        if (command.TrimStart().StartsWith("#")) // skip comment
            return true;

        var semicolonSep = 
            from value in command.Split(';')
            where !string.IsNullOrWhiteSpace(value)
            select value.Trim(); // split using ; and remove all the empty parts and trim the resulting strings
        
        var success = true;

        foreach (var s in semicolonSep)
            success = ExecuteCommandInner(s, ref vars);

        return success;
    }

    /// <summary>
    /// Handles the actual command parsing. Also parses logical operator &&.
    /// </summary>
    /// <param name="command">The command to be executed.</param>
    /// <param name="vars">The key-value pairs that correspond to variables. The contents may change depending on the command executed.</param>
    /// <returns></returns>
    private static bool ExecuteCommandInner(string command, ref Dictionary<string, string> vars)
    {
        var andCommands =
            from value in command.Split("&&")
            where !string.IsNullOrWhiteSpace(value)
            select value.Trim(); // split using && and remove all the empty parts and trim the resulting strings

        var success = true;

        foreach (var s in andCommands)
        {
            var newStr = vars.Aggregate(s,
                (current, keyValuePair) => Regex.Replace(current, $@"\${keyValuePair.Key}\b", keyValuePair.Value));

            if (Regex.IsMatch(newStr, @"\s*\w+\s*=\s*.*")) // captures assignment
            {
                var kv = newStr.Split("="); // split using =

                // trim each side and then remove quotes
                string var = kv[0].Trim(), value = kv[1].Trim().Replace("\"", "");

                if (!vars.ContainsKey(var))
                    vars.Add(var, value);
                else
                    vars[var] = value;

                LastExitCode = 0;
                success = true;

                continue; // no more commands (since this is an assignment), proceed to next command
            }

            List<string> args = new();

            for (int i = 0, j = 0; i < newStr.Length; /* intentionally empty */)
            {
                args.Add(string.Empty); // add empty

                if (i < newStr.Length && newStr[i] == '"')
                {
                    args[j] += newStr[i++]; // if newStr starts with quote, append to args[j]

                    while (i < newStr.Length && newStr[i] != '"') // until newStr is not a closing quote, do the same
                        args[j] += newStr[i++];

                    args[j] += newStr[i++]; // append closing quote
                }
                else
                {
                    while (i < newStr.Length && newStr[i] is not (' ' or '\r' or '\n' or '\t' or '\f' or '\v'))
                        args[j] += newStr[i++]; // while not at end or not whitespace, append each char of newStr to args[j]
                }

                ++j;

                while (i < newStr.Length && newStr[i] is ' ' or '\r' or '\n' or '\t' or '\f' or '\v')
                    ++i; // skip whitespace
            }

            if (args[0].StartsWith('"') && args[0].EndsWith('"'))
                args[0] = args[0][1..^1]; // extract text between quotes

            switch (args[0])
            {
                case "echo":
                    BuiltInCommands.Echo(string.Join(' ', args.Skip(1)));

                    LastExitCode = 0;
                    success = true;

                    break;

                case "print":
                    if (args.Count < 2)
                    {
                        LastExitCode = 1;
                        ExitErrorMessage = "Too few arguments for print.";
                        
                        return success; // return if any command fails (&&)
                    }

                    for (var i = 1; i < args.Count; i++)
                        if (args[i].StartsWith('"') && args[i].EndsWith('"'))
                            args[i] = args[i][1..^1];

                    try
                    {
                        BuiltInCommands.Print(args[1], args.Skip(2).ToArray());
                    }
                    catch (Exception e)
                    {
                        LastExitCode = 2;
                        ExitErrorMessage = e.Message;

                        return false;
                    }

                    LastExitCode = 0;
                    success = true;

                    break;

                case "help":
                    BuiltInCommands.Help();

                    LastExitCode = 0;
                    success = true;

                    break;

                default:
                    {
                        Process process = new();
                        process.StartInfo.FileName = args[0];

                        // this is the reason why we didn't extract the text between the quotes earlier
                        process.StartInfo.Arguments = string.Join(' ', args.Skip(1));

                        try
                        {
                            if (!process.Start())
                                return false;
                        }
                        catch (Exception e)
                        {
                            ExitErrorMessage = e.Message; // set error message if we encounter an exception
                            return false;
                        }

                        process.WaitForExit();

                        LastExitCode = process.ExitCode;
                        success = true;
                    }
                    break;
            }
        }

        return success;
    }
}
