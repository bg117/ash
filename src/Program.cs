using System.Reflection;

namespace OpenProject.ASH;

internal static class Program
{
    private const string DefaultFormat = @"%u@%m:%c$ ";

    private static readonly Version ProgramVersion =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);

    private static readonly HelpContext[] HelpContexts =
    {
        new()
        {
            Command = "-q",
            Description =
                "Runs the program quietly (don't show version and help)."
        },
        new()
        {
            Command     = "-ex",
            Description = "Executes a command BEFORE parsing .apcrc."
        },
        new()
        {
            Command     = "-v",
            Description = "Shows the version and exits the program."
        },
        new()
        {
            Command     = "-h",
            Description = "Shows this help message and exits the program."
        }
    };

    private static int Main(string[] args)
    {
        var kvEx = new Dictionary<string, string>();
        var kv = new Dictionary<string, string>();

        var home =
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var rc = Path.Combine(home, ".apcrc");

        var quietStartup = args.Contains("-q");

        // if there are -ex arguments, execute them first
        if (args.Contains("-ex"))
        {
            var exitImmediately = args.Contains("-c");
            var execute =
                args
                   .Select((a, ind) => a == "-ex" ? ind : -1)
                   .Where(ind => ind !=
                                 -1); // gets the indices of all -ex arguments

            foreach (var i in execute)
            {
                if ((i + 1) >= args.Length)
                {
                    ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                       "[Error]: -ex requires an argument.",
                                                       ConsoleColor.Red);

                    return 1;
                }

                Shell.ExecuteCommand(args[i + 1], ref kvEx);
            }

            if (exitImmediately)
                return 0;
        }

        if (args.Contains("-h"))
        {
            Console.WriteLine($"ASH version {ProgramVersion}");
            foreach (var ctx in HelpContexts)
                Console.WriteLine($"{ctx.Command}: {ctx.Description}");
            return 0;
        }

        if (args.Contains("-v"))
        {
            Console.WriteLine($"ASH version {ProgramVersion}");
            return 0;
        }

        if (!File.Exists(rc))
        {
            File.AppendAllLines(
                                rc,
                                new[]
                                {
                                    "#RC VERSION 001",
                                    $"{ShellProperties.BuiltInVariables[0]} = \"{DefaultFormat}\"",
                                    $"{ShellProperties.BuiltInVariables[1]} = white",
                                    $"{ShellProperties.BuiltInVariables[2]} = white",
                                    $"{ShellProperties.BuiltInVariables[3]} = white",
                                    $"{ShellProperties.BuiltInVariables[4]} = white",
                                    $"{ShellProperties.BuiltInVariables[5]} = white",
                                    $"{ShellProperties.BuiltInVariables[6]} = false",
                                    $"{ShellProperties.BuiltInVariables[7]} = {Environment.GetEnvironmentVariable("PATH")?.Replace(Path.PathSeparator, '|')}",
                                    $"{ShellProperties.BuiltInVariables[8]} = {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}"
                                });
        }

        Shell.RunShellFile(rc, ref kv);

        // merge kvEx and kv, prioritizing kvEx values if there are duplicates
        kv = kvEx.Merge(kv);

        if (args.Contains("-run"))
        {
            var exitWhenError = args.Contains("-e");

            var execute =
                args
                   .Select((a, ind) => a == "-run" ? ind : -1)
                   .Where(ind => ind !=
                                 -1).ToList(); // gets the indices of all -run arguments
            // (but we'll get the first only)
            var index = execute[0] + 1;

            if (index >= args.Length)
            {
                ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                   "[Error]: -run requires a shell script to run.",
                                                   ConsoleColor.Red);
                return 2;
            }

            var script = args[index];
            if (!File.Exists(script))
            {
                ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                   $"[Error]: script \"{script}\"" +
                                                   "does not exist in the current directory",
                                                   ConsoleColor.Red);
                return 3;
            }

            var lines = File.ReadAllLines(script);

            foreach (var line in lines.Where(line => !Shell.ExecuteCommand(line, ref kv)))
            {
                ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                   $"[Error]: {ShellProperties.ExitErrorMessage}",
                                                   ConsoleColor.Red);

                if (exitWhenError)
                    return ShellProperties.LastExitCode;
            }

            return 0;
        }

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[0]))
            kv.Add(ShellProperties.BuiltInVariables[0], DefaultFormat);

        if (!quietStartup)
        {
            if (kv.ContainsKey(ShellProperties.BuiltInVariables[6]))
                _ = bool.TryParse(kv[ShellProperties.BuiltInVariables[6]], out quietStartup);
        }

        var userColor = ConsoleColor.White;
        var machineColor = ConsoleColor.White;
        var cwdColor = ConsoleColor.White;
        var exitColorSuccess = ConsoleColor.White;
        var exitColorFail = ConsoleColor.White;

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[1]))
            kv.Add(ShellProperties.BuiltInVariables[1], userColor.ToString());

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[2]))
            kv.Add(ShellProperties.BuiltInVariables[2], machineColor.ToString());

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[3]))
            kv.Add(ShellProperties.BuiltInVariables[3], cwdColor.ToString());

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[4]))
            kv.Add(ShellProperties.BuiltInVariables[4], exitColorSuccess.ToString());

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[5]))
            kv.Add(ShellProperties.BuiltInVariables[5], exitColorFail.ToString());

        if (!quietStartup)
        {
            Console.WriteLine($"ASH (Application shell) version {ProgramVersion}");
            Console.WriteLine($"To hide this message, run ASH with -ex \"{ShellProperties.BuiltInVariables[6]} = true\" (overrides .apcrc) " +
                              "or the -q flag (overrides both -ex and .apcrc.)");
            Console.WriteLine();

            Console.WriteLine("To see the names and descriptions of every built-in command, type \"help\" then press ENTER.");
            Console.WriteLine();
        }

        while (true)
        {
            var user = Environment.UserName;
            var host = Environment.MachineName;

            var cwd = Environment.CurrentDirectory;

            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[1]], true, out userColor);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[2]], true, out machineColor);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[3]], true, out cwdColor);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[4]], true, out exitColorSuccess);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[5]], true, out exitColorFail);

            ConsoleExtensions.ColoredWrite(Console.Out, kv[ShellProperties.BuiltInVariables[0]]
                                                       .Replace("%u",
                                                                $"[{user}]")
                                                       .Replace("%m",
                                                                $"[{host}]")
                                                       .Replace("%c",
                                                                $"[{cwd}]")
                                                       .Replace("%nl", "\n")
                                                       .Replace("%e",
                                                                $"[{ShellProperties.LastExitCode}]"),
                                           userColor,
                                           machineColor,
                                           cwdColor,
                                           ShellProperties.LastExitCode == 0
                                               ? exitColorSuccess
                                               : exitColorFail);

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input == "exit")
                return 0;

            input = input.TrimStart();

            try
            {
                if (!Shell.ExecuteCommand(input, ref kv))
                {
                    ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                       $"[Error]: {ShellProperties.ExitErrorMessage}",
                                                       ConsoleColor.Red);
                }
            }
            catch (Exception e)
            {
                ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                   $"[Error]: {e.Message}",
                                                   ConsoleColor.Red);
            }

            Console.WriteLine();
        }
    }
}
