using System.Text.RegularExpressions;
using System.Diagnostics;

namespace OpenProject.ASH;

internal static class CliParser
{
    internal static volatile int LastExitCode;
    internal static volatile string ExitErrorMessage = string.Empty;

    private static readonly Stack<string> LastArguments = new();

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

            var args = SliceArguments(newStr);

            if (args[0].StartsWith('"') && args[0].EndsWith('"'))
                args[0] = args[0][1..^1]; // extract text between quotes

            var lastArg = string.Empty;

            if (LastArguments.Count > 0)
                lastArg = LastArguments.Pop();

            for (var j = 1; j < args.Length; j++)
                args[j] = Regex.Replace(args[j], @"\$\<", lastArg);

            if (args.Length > 1)
                LastArguments.Push(args.Last());

            switch (args[0])
            {
                case "echo":
                    BuiltInCommands.Echo(string.Join(' ', args.Skip(1)));

                    LastExitCode = 0;
                    success = true;

                    break;

                case "print":
                    if (args.Length < 2)
                    {
                        LastExitCode = 1;
                        ExitErrorMessage = "Too few arguments for print.";
                        
                        return false; // return if any command fails (&&)
                    }

                    for (var i = 1; i < args.Length; i++)
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
                    switch (args.Length)
                    {
                        case > 2:
                            LastExitCode = 1;
                            ExitErrorMessage = "Too many arguments for help.";

                            return false; // return if any command fails (&&)
                        case 2:
                            try
                            {
                                BuiltInCommands.Help(args[1]);
                            }
                            catch (Exception e)
                            {
                                LastExitCode = 2;
                                ExitErrorMessage = e.Message;

                                return false;
                            }

                            break;
                        default:
                            BuiltInCommands.Help();

                            LastExitCode = 0;
                            success = true;
                            break;
                    }

                    break;

                case "cd":
                    if (args.Length < 2)
                    {
                        LastExitCode = 1;
                        ExitErrorMessage = "Too few arguments for cd.";

                        return false; // return if any command fails (&&)
                    }

                    if (args[1].StartsWith('"') && args[1].EndsWith('"'))
                        args[1] = args[1][1..^1];

                    try
                    {
                        BuiltInCommands.Cd(args[1]);
                    }
                    catch (Exception e)
                    {
                        LastExitCode = 2;
                        ExitErrorMessage = e.Message;

                        return false;
                    }

                    break;

                case "ls":
                    {
                        bool humanReadable = false, all = false, list = false;
                        var predicate = new Func<string, bool>(each => each is not ("-l" or "-a" or "-h"));
                        var directory = Directory.GetCurrentDirectory();

                        if (args.Contains("-l"))
                            list = true;

                        if (args.Contains("-a"))
                            all = true;

                        if (args.Contains("-h"))
                            humanReadable = true;

                        if (args.Skip(1).Any(predicate))
                        {
                            if (args.Skip(1).Where(predicate).Take(2).Count() > 1)
                            {
                                LastExitCode = 1;
                                ExitErrorMessage = "Too many arguments for ls.";

                                return false;
                            }
                            directory = args.Skip(1).First(predicate);
                        }

                        try
                        {
                            BuiltInCommands.Ls(directory, list, all, humanReadable);
                        }
                        catch (Exception e)
                        {
                            LastExitCode = 2;
                            ExitErrorMessage = e.Message;

                            return false;
                        }
                    }

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

    /// <summary>
    /// Slices arguments into an array of strings. Parses text in quotes correctly.
    /// </summary>
    /// <param name="arg">The string to be sliced.</param>
    /// <param name="escape">The escape character to be used for escaping characters like quotation marks. Default is the backslash (\).</param>
    /// <returns>The array of sliced strings.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    internal static string[] SliceArguments(string arg, char escape = '\\')
    {
        List<string> args = new();

        for (int i = 0, j = 0; i < arg.Length; /* intentionally empty */)
        {
            args.Add(string.Empty); // add empty

            if (i < arg.Length && arg[i] == '"')
            {
                args[j] += arg[i++]; // if arg starts with quote, append to args[j]

                while (i < arg.Length && arg[i] != '"') // until arg is not a closing quote, do the same
                {
                    if (arg[i] == escape)
                        i++;

                    if (i < arg.Length)
                        args[j] += arg[i++];
                }

                args[j] += arg[i++]; // append closing quote
            }
            else
            {
                while (i < arg.Length && arg[i] is not (' ' or '\r' or '\n' or '\t' or '\f' or '\v'))
                    args[j] += arg[i++]; // while not at end or not whitespace, append each char of arg to args[j]
            }

            ++j;

            while (i < arg.Length && arg[i] is ' ' or '\r' or '\n' or '\t' or '\f' or '\v')
                ++i; // skip whitespace
        }

        return args.ToArray();
    }
}
