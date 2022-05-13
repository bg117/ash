namespace OpenProject.ASH;

internal static class Program
{
    private const string DefaultFormat = @"%u:%m@%c ~% ";

    // List of built-in variables. The order of these won't change, ever.
    private static readonly string[] BuiltInVariables =
    {
        "prompt_fmt",
        "prompt_fmt_u_color",
        "prompt_fmt_m_color",
        "prompt_fmt_c_color",
        "prompt_fmt_e_success_color",
        "prompt_fmt_e_fail_color",
        "quiet_startup"
    };

    private static readonly Version ProgramVersion = new(1, 1, 0);

    private static readonly HelpContext[] HelpContexts =
    {
        new() { Command = "-q", Description = "Runs the program quietly (don't show version and help)." },
        new() { Command = "-ex", Description = "Executes a command BEFORE parsing .apcrc." },
        new() { Command = "-v", Description = "Shows the version and exits the program." },
        new() { Command = "-h", Description = "Shows this help message and exits the program." }
    };

    private static int Main(string[] args)
    {
        var kvEx = new Dictionary<string, string>();
        var kv = new Dictionary<string, string>();

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var rc = Path.Combine(home, ".apcrc");

        var quietStartup = false;

        // if there are -ex arguments, execute them first
        if (args.Contains("-ex"))
        {
            var execute =
                args
                    .Select((a, ind) => a == "-ex" ? ind : -1)
                    .Where(ind => ind != -1); // gets the indices of all -ex arguments

            foreach (var i in execute)
            {
                if (i + 1 >= args.Length)
                {
                    ConsoleExtensions.ColoredWriteLine(Console.Error, "[Error]: -ex requires an argument.",
                        ConsoleColor.Red);

                    return 1;
                }

                CliParser.ExecuteCommand(args[i + 1], ref kvEx);
            }
        }

        if (args.Contains("-q"))
            quietStartup = true;

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
                new []
                {
                    "#RC VERSION 001",
                    $"{BuiltInVariables[0]} = \"{DefaultFormat}\"",
                    $"{BuiltInVariables[1]} = blue",
                    $"{BuiltInVariables[2]} = cyan",
                    $"{BuiltInVariables[3]} = magenta",
                    $"{BuiltInVariables[4]} = green",
                    $"{BuiltInVariables[5]} = red",
                    $"{BuiltInVariables[6]} = false"
                });
        }

        Config.ReadConfigFile(rc, ref kv);

        // merge kvEx and kv, prioritizing kvEx values if there are duplicates
        kvEx.ToList().ForEach(x => kv[x.Key] = x.Value);

        if (!kv.ContainsKey(BuiltInVariables[0]))
            kv.Add(BuiltInVariables[0], DefaultFormat);

        // second check
        if (kv.ContainsKey(BuiltInVariables[6]))
            _ = bool.TryParse(kv[BuiltInVariables[6]], out quietStartup);

        ConsoleColor userColor = ConsoleColor.Blue, machineColor = ConsoleColor.Cyan, cwdColor = ConsoleColor.Magenta,
            exitColorSuccess = ConsoleColor.Green, exitColorFail = ConsoleColor.Red;

        if (quietStartup == false /* WHY?? nullable bool */)
        {
            Console.WriteLine($"ASH (Application shell) version {ProgramVersion}");
            Console.WriteLine("To hide this message, run ASH with the -q flag (overrides .apcrc) " +
                              $"or -ex \"{BuiltInVariables[6]} = true\" (overrides both -q and .apcrc.)");
            Console.WriteLine();

            Console.WriteLine("To see the names and descriptions of every built-in command, type \"help\" then press ENTER.");
            Console.WriteLine();
        }

        while (true)
        {
            if (kv.ContainsKey(BuiltInVariables[1]))
                _ = Enum.TryParse(kv[BuiltInVariables[1]], true, out userColor);

            if (kv.ContainsKey(BuiltInVariables[2]))
                _ = Enum.TryParse(kv[BuiltInVariables[2]], true, out machineColor);

            if (kv.ContainsKey(BuiltInVariables[3]))
                _ = Enum.TryParse(kv[BuiltInVariables[3]], true, out cwdColor);

            if (kv.ContainsKey(BuiltInVariables[4]))
                _ = Enum.TryParse(kv[BuiltInVariables[4]], true, out exitColorSuccess);

            if (kv.ContainsKey(BuiltInVariables[5]))
                _ = Enum.TryParse(kv[BuiltInVariables[5]], true, out exitColorFail);

            var user = Environment.UserName;
            var host = Environment.MachineName;

            var cwd = Environment.CurrentDirectory;

            ConsoleExtensions.ColoredWrite(Console.Out, kv[BuiltInVariables[0]]
                                                    .Replace("%u", $"[{user}]")
                                                    .Replace("%m", $"[{host}]")
                                                    .Replace("%c", $"[{cwd}]")
                                                    .Replace("%nl", "\n")
                                                    .Replace("%e", $"[{CliParser.LastExitCode}]"),
                                           userColor,
                                               machineColor,
                                               cwdColor,
                                               CliParser.LastExitCode == 0 ? exitColorSuccess : exitColorFail);

            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input == "exit")
                return 0;

            input = input.TrimStart();

            try
            {
                if (!CliParser.ExecuteCommand(input, ref kv))
                    ConsoleExtensions.ColoredWriteLine(Console.Error, $"[Error]: {CliParser.ExitErrorMessage}",
                        ConsoleColor.Red);
            }
            catch (Exception e)
            {
                ConsoleExtensions.ColoredWriteLine(Console.Error, $"[Error]: {e.Message}",
                    ConsoleColor.Red);
            }

            Console.WriteLine();
        }
    }
}
