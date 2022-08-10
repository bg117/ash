using System.Reflection;

namespace OpenProject.ASH;

internal static class Program {
    private const string DefaultFormat = @"%u@%m:%c$ ";
    private const string ConfigFile    = "ash.cfg";

    private static readonly string ProgramVersion =
        (Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0))
       .ToString(3);

    private static readonly HelpContext[] HelpContexts =
    {
        new()
        {
            Command = "--quiet|-q",
            Description =
                "Runs the program quietly."
        },
        new()
        {
            Command = "--execute|-x",
            Description =
                $"Executes a command before parsing {ConfigFile}. Optionally pass -c if the shell must exit when a command fails."
        },
        new()
        {
            Command     = "--version|-v",
            Description = "Shows the version and exits the program."
        },
        new()
        {
            Command     = "--help|-h",
            Description = "Shows this help message and exits the program."
        }
    };

    private static int Main(string[] args)
    {
        var kvEx = new Dictionary<string, string>();
        var kv   = new Dictionary<string, string>();

        var home =
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var rc = Path.Combine(home, ConfigFile);

        var quietStartup    = args.Contains("--quiet") || args.Contains("-q");
        var exitImmediately = args.Contains("-c");

        // if there are -ex arguments, execute them first
        if (args.Contains("--execute") ||
            args.Contains("-x")) {
            var execute =
                args
                   .Select((a, ind) => a is "--execute" or "-x" ? ind : -1)
                   .Where(ind => ind !=
                                 -1); // gets the indices of all -ex arguments

            foreach (var i in execute) {
                if ((i + 1) >= args.Length) {
                    ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                       "[Error]: --execute requires an argument.",
                                                       ConsoleColor.Red);

                    return 1;
                }

                Shell.ExecuteCommand(args[i + 1], ref kvEx);
            }

            if (exitImmediately)
                return 0;
        }

        if (args.Contains("--help") ||
            args.Contains("-h")) {
            Console.WriteLine($"ASH version {ProgramVersion}");
            foreach (var ctx in HelpContexts)
                Console.WriteLine($"{ctx.Command}: {ctx.Description}");
            return 0;
        }

        if (args.Contains("--version") ||
            args.Contains("-v")) {
            Console.WriteLine($"ASH version {ProgramVersion}");
            return 0;
        }

        if (!File.Exists(rc)) {
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

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[0]))
            kv.Add(ShellProperties.BuiltInVariables[0], DefaultFormat);

        if (!quietStartup) {
            if (kv.ContainsKey(ShellProperties.BuiltInVariables[6])) {
                _ = bool.TryParse(kv[ShellProperties.BuiltInVariables[6]],
                                  out quietStartup);
            }
        }

        var userColor        = ConsoleColor.White;
        var machineColor     = ConsoleColor.White;
        var cwdColor         = ConsoleColor.White;
        var exitColorSuccess = ConsoleColor.White;
        var exitColorFail    = ConsoleColor.White;

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[1]))
            kv.Add(ShellProperties.BuiltInVariables[1], userColor.ToString());

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[2])) {
            kv.Add(ShellProperties.BuiltInVariables[2],
                   machineColor.ToString());
        }

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[3]))
            kv.Add(ShellProperties.BuiltInVariables[3], cwdColor.ToString());

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[4])) {
            kv.Add(ShellProperties.BuiltInVariables[4],
                   exitColorSuccess.ToString());
        }

        if (!kv.ContainsKey(ShellProperties.BuiltInVariables[5])) {
            kv.Add(ShellProperties.BuiltInVariables[5],
                   exitColorFail.ToString());
        }

        if (!quietStartup) {
            Console.WriteLine($"ASH (Application shell) version {ProgramVersion}");
            Console.WriteLine($"To hide this message, run ASH with --execute \"{ShellProperties.BuiltInVariables[6]} = true\" (overrides {ConfigFile}) " +
                              $"or the -q flag (overrides both --execute and {ConfigFile}).");
            Console.WriteLine();

            Console.WriteLine("To print a list of all built-in commands, type \"help\" then press ENTER.");
            Console.WriteLine();
        }

        while (true) {
            var user = Environment.UserName;
            var host = Environment.MachineName;

            var cwd = Environment.CurrentDirectory;

            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[1]], true,
                              out userColor);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[2]], true,
                              out machineColor);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[3]], true,
                              out cwdColor);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[4]], true,
                              out exitColorSuccess);
            _ = Enum.TryParse(kv[ShellProperties.BuiltInVariables[5]], true,
                              out exitColorFail);

            ConsoleExtensions.ColoredWrite(Console.Out,
                                           kv[ShellProperties.BuiltInVariables[0]]
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

            var input     = string.Empty;
            var firstIter = true;
            do {
                if (!firstIter)
                    Console.Write(">>> ");

                firstIter = false;

                var tmp = Console.ReadLine();
                if (tmp == null)
                    break;

                tmp = tmp.Trim();
                if (tmp.EndsWith("\\"))
                    input += ' ' + tmp[..^1] /* don't include backslash */;
                else {
                    input += ' ' + tmp;
                    break;
                }
            } while (true);

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input == "exit")
                return 0;

            input = input.Trim();

            try {
                if (!Shell.ExecuteCommand(input, ref kv)) {
                    ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                       $"[Error]: {ShellProperties.ExitErrorMessage}",
                                                       ConsoleColor.Red);
                }
            }
            catch (Exception e) {
                ConsoleExtensions.ColoredWriteLine(Console.Error,
                                                   $"[Error]: {e.Message}",
                                                   ConsoleColor.Red);
            }
        }
    }
}
